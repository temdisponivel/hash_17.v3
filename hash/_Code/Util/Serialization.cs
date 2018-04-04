using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace hash.Util
{
    public class MemberIgnoreAttribute : Attribute
    {
    }

    public class MemberVersionAttribute : Attribute
    {
        public int MinVersion;
        public int MaxVersion;

        public MemberVersionAttribute(int version)
        {
            MinVersion = version;
            MaxVersion = version;
        }

        public MemberVersionAttribute(int minVersion, int maxVersion)
        {
            MinVersion = minVersion;
            MaxVersion = maxVersion;
        }
    }

    public class MemberNameAttribute : Attribute
    {
        public string Name;

        public MemberNameAttribute(string name)
        {
            Name = name;
        }
    }

    public class SerializableMember<T>
    {
        public string Name;
        public T Value;
    }

    public class SerializableObject
    {
        public List<SerializableMember<bool>> BoolValues;
        public List<SerializableMember<int>> IntValues;
        public List<SerializableMember<float>> FloatValues;
        public List<SerializableMember<string>> StringValues;
        public List<SerializableMember<SerializableObject>> ComplexValues;
        public List<SerializableMember<Array>> ArrayValues;
        public List<SerializableMember<Enum>> EnumValues;
    }

    public class DeseriazableMember
    {
        public string Name;
        public Type Type;
    }

    public class DeserializableObject
    {
        public List<DeseriazableMember> BoolMembers;
        public List<DeseriazableMember> IntMembers;
        public List<DeseriazableMember> FloatMembers;
        public List<DeseriazableMember> StringMembers;
        public List<DeseriazableMember> ComplexMembers;
        public List<DeseriazableMember> ArrayMembers;
    }

    public struct SerializableMemberInfo
    {
        public bool IsSerializable;
        public string Name;
        public object MemberValue;
        public Type MemberType;
    }

    public static class Serialization
    {
        public static string ByteArrayToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] StringToByteArray(string obj)
        {
            return Encoding.UTF8.GetBytes(obj);
        }

        public static byte[] Serialize<T>(T obj)
        {
            SerializableObject serializableObject = GetSerializableObjectFromType(obj.GetType(), obj);
            StringBuilder builder = new StringBuilder(); // TODO: Figure a way to pass a reasonable capacity
            WriteObject(serializableObject, builder);
            string serialized = builder.ToString();
            return StringToByteArray(serialized);
        }

        public static T Deserialize<T>(byte[] obj)
        {
            return default(T);
        }

        public static DeserializableObject GetDeserializableObject(Type objectType)
        {
            DeserializableObject result = new DeserializableObject();

            result.BoolMembers = new List<DeseriazableMember>();
            result.IntMembers = new List<DeseriazableMember>();
            result.FloatMembers = new List<DeseriazableMember>();
            result.StringMembers = new List<DeseriazableMember>();
            result.ComplexMembers = new List<DeseriazableMember>();
            result.ArrayMembers = new List<DeseriazableMember>();

            MemberInfo[] members = objectType.GetMembers();

            int len = members.Length;
            for (int i = 0; i < len; i++)
            {
                MemberInfo memberInfo = members[i];

                SerializableMemberInfo info = GetMemberInfo(
                    objectType,
                    null,
                    memberInfo
                );

                if (info.IsSerializable)
                {
                    bool isPrimitive = info.MemberType.IsPrimitive || 
                                       info.MemberType == typeof(string);

                    if (isPrimitive)
                    {
                        if (info.MemberType == typeof(bool))
                        {
                            DeseriazableMember member = new DeseriazableMember();
                            member.Name = info.Name;
                            member.Type = typeof(bool);
                            result.BoolMembers.Add(member);
                        }
                        else if (info.MemberType == typeof(int))
                        {
                            DeseriazableMember member = new DeseriazableMember();
                            member.Name = info.Name;
                            member.Type = typeof(int);
                            result.IntMembers.Add(member);
                        }
                        else if (info.MemberType == typeof(float))
                        {
                            DeseriazableMember member = new DeseriazableMember();
                            member.Name = info.Name;
                            member.Type = typeof(float);
                            result.FloatMembers.Add(member);
                        }
                        else if (info.MemberType == typeof(string))
                        {
                            DeseriazableMember member = new DeseriazableMember();
                            member.Type = typeof(string);
                            result.StringMembers.Add(member);
                        }
                    }
                    else if (info.MemberType.IsArray)
                    {
                        DeseriazableMember member = new DeseriazableMember();
                        member.Name = info.Name;
                        member.Type = info.MemberType;
                        result.ArrayMembers.Add(member);
                    }
                    else
                    {
                        DeseriazableMember member = new DeseriazableMember();
                        member.Name = info.Name;
                        member.Type = info.MemberType;
                        result.ComplexMembers.Add(member);
                    }
                }
            }

            return result;
        }

        public static SerializableObject GetSerializableObjectFromType(Type type, object obj)
        {
            SerializableObject result = new SerializableObject();

            result.BoolValues = new List<SerializableMember<bool>>();
            result.IntValues = new List<SerializableMember<int>>();
            result.FloatValues = new List<SerializableMember<float>>();
            result.StringValues = new List<SerializableMember<string>>();
            result.ComplexValues = new List<SerializableMember<SerializableObject>>();
            result.ArrayValues = new List<SerializableMember<Array>>();
            result.EnumValues = new List<SerializableMember<Enum>>();

            MemberInfo[] members = type.GetMembers();

            int len = members.Length;
            for (int i = 0; i < len; i++)
            {
                MemberInfo info = members[i];

                SerializableMemberInfo serializableInfo = GetMemberInfo(
                    type,
                    obj,
                    info
                );

                if (serializableInfo.IsSerializable)
                {
                    HandleMember(
                        serializableInfo.Name,
                        serializableInfo.MemberType,
                        serializableInfo.MemberValue,
                        result
                    );
                }
            }

            return result;
        }

        public static SerializableMemberInfo GetMemberInfo(
            Type objectType,
            object obj,
            MemberInfo info
        )
        {
            SerializableMemberInfo result = new SerializableMemberInfo();

            result.Name = info.Name;
            result.IsSerializable = true;

            if ((info.MemberType & (MemberTypes.Field | MemberTypes.Property)) == 0)
                result.IsSerializable = false;
            else
            {
                IEnumerable<CustomAttributeData> attributes = info.CustomAttributes;
                foreach (var attributeData in attributes)
                {
                    if (attributeData.AttributeType == typeof(MemberIgnoreAttribute))
                    {
                        result.IsSerializable = false;
                        break;
                    }
                    else if (attributeData.AttributeType == typeof(MemberNameAttribute))
                        result.Name = attributeData.ConstructorArguments[0].Value as string;
                }

                if ((info.MemberType & MemberTypes.Property) != 0)
                {
                    PropertyInfo property = objectType.GetProperty(info.Name);
                    if (!property.CanWrite)
                        result.IsSerializable = false;
                }
            }

            if (result.IsSerializable)
            {
                if ((info.MemberType & MemberTypes.Field) != 0)
                {
                    FieldInfo field = objectType.GetField(info.Name);
                    result.MemberType = field.FieldType;

                    if (obj != null)
                        result.MemberValue = field.GetValue(obj);
                }
                else
                {
                    PropertyInfo property = objectType.GetProperty(info.Name);
                    result.MemberType = property.PropertyType;

                    if (obj != null)
                        result.MemberValue = property.GetValue(obj);
                }

                if (result.MemberType.IsGenericType)
                    result.IsSerializable = false;
            }

            return result;
        }

        public static void HandleMember(string name, Type type, object value, SerializableObject obj)
        {
            bool isPrimitive = type.IsPrimitive || type == typeof(string);

            if (isPrimitive)
            {
                if (type == typeof(bool))
                {
                    SerializableMember<bool> member = new SerializableMember<bool>();
                    member.Name = name;
                    member.Value = (bool) value;
                    obj.BoolValues.Add(member);
                }
                else if (type == typeof(int))
                {
                    SerializableMember<int> member = new SerializableMember<int>();
                    member.Name = name;
                    member.Value = (int) value;
                    obj.IntValues.Add(member);
                }
                else if (type == typeof(float))
                {
                    SerializableMember<float> member = new SerializableMember<float>();
                    member.Name = name;
                    member.Value = (float) value;
                    obj.FloatValues.Add(member);
                }
                else if (type == typeof(string))
                {
                    SerializableMember<string> member = new SerializableMember<string>();
                    member.Name = name;

                    if (type.IsEnum)
                        member.Value = Enum.GetName(type, value);
                    else
                        member.Value = (string) value;

                    obj.StringValues.Add(member);
                }
            }
            else if (type.IsArray)
            {
                SerializableMember<Array> member = new SerializableMember<Array>();
                member.Name = name;
                member.Value = value as Array;
                obj.ArrayValues.Add(member);
            }
            else if (type.IsEnum)
            {
                SerializableMember<Enum> member = new SerializableMember<Enum>();
                member.Name = name;
                member.Value = value as Enum;
                obj.EnumValues.Add(member);
            }
            else
            {
                SerializableMember<SerializableObject> member = new SerializableMember<SerializableObject>();
                member.Name = name;
                member.Value = GetSerializableObjectFromType(type, value);
                obj.ComplexValues.Add(member);
            }
        }

        public static void WriteObject(SerializableObject serializableObject, StringBuilder builder)
        {
            for (int i = 0; i < serializableObject.BoolValues.Count; i++)
            {
                SerializableMember<bool> boolValue = serializableObject.BoolValues[i];
                WriteBool(builder, boolValue.Name, boolValue.Value);
            }

            for (int i = 0; i < serializableObject.IntValues.Count; i++)
            {
                SerializableMember<int> intValue = serializableObject.IntValues[i];
                WriteInt(builder, intValue.Name, intValue.Value);
            }

            for (int i = 0; i < serializableObject.FloatValues.Count; i++)
            {
                SerializableMember<float> floatValue = serializableObject.FloatValues[i];
                WriteFloat(builder, floatValue.Name, floatValue.Value);
            }

            for (int i = 0; i < serializableObject.StringValues.Count; i++)
            {
                SerializableMember<string> stringValue = serializableObject.StringValues[i];
                WriteString(builder, stringValue.Name, stringValue.Value);
            }
            
            for (int i = 0; i < serializableObject.EnumValues.Count; i++)
            {
                SerializableMember<Enum> numValue = serializableObject.EnumValues[i];
                WriteEnum(builder, numValue.Name, numValue.Value);
            }

            for (int i = 0; i < serializableObject.ArrayValues.Count; i++)
            {
                SerializableMember<Array> arrayValues = serializableObject.ArrayValues[i];
                WriteArray(builder, arrayValues.Name, arrayValues.Value);
            }

            for (int i = 0; i < serializableObject.ComplexValues.Count; i++)
            {
                SerializableMember<SerializableObject> complexValue = serializableObject.ComplexValues[i];
                WriteComplex(builder, complexValue.Name, complexValue.Value);
            }
        }

        public static void WriteBool(StringBuilder builder, string name, bool value)
        {
            builder.AppendFormat("{0} : {1}\n", name, value ? "true" : "false");
        }

        public static void WriteInt(StringBuilder builder, string name, int value)
        {
            builder.AppendFormat("{0} : {1:D}\n", name, value);
        }

        public static void WriteFloat(StringBuilder builder, string name, float value)
        {
            builder.AppendFormat("{0} : {1:F}\n", name, value);
        }

        public static void WriteString(StringBuilder builder, string name, string value)
        {
            builder.AppendFormat("{0} : \"{1}\"\n", name, value);
        }

        public static void WriteArray(StringBuilder builder, string name, Array value)
        {
            builder.AppendFormat("{0} : [\n", name);

            int len = value.Length;
            for (int i = 0; i < len; i++)
            {
                object elementValue = value.GetValue(i);
                Type elementType = elementValue.GetType();

                bool isPrimitive = elementType.IsPrimitive ||
                                   elementType == typeof(string);

                if (isPrimitive)
                {
                    if (elementType == typeof(bool))
                    {
                        WriteBool(builder, i.ToString(), (bool) elementValue);
                    }
                    else if (elementType == typeof(int))
                    {
                        WriteInt(builder, i.ToString(), (int) elementValue);
                    }
                    else if (elementType == typeof(float))
                    {
                        WriteFloat(builder, i.ToString(), (float) elementValue);
                    }
                    else if (elementType == typeof(string))
                    {
                        string valueStr = (string) elementValue;
                        WriteString(builder, i.ToString(), valueStr);
                    }
                }
                else if (elementType.IsEnum)
                {
                    WriteEnum(builder, i.ToString(), (Enum) elementValue);
                }
                else
                {
                    SerializableObject serializableObject = GetSerializableObjectFromType(
                        elementType,
                        elementValue
                    );

                    WriteComplex(builder, i.ToString(), serializableObject);
                }
            }

            builder.Append("]\n");
        }
        
        public static void WriteComplex(StringBuilder builder, string name, SerializableObject value)
        {
            builder.AppendFormat("{0} : {{\n", name);
            WriteObject(value, builder);
            builder.AppendFormat("}}\n");
        }

        public static void WriteEnum(StringBuilder builder, string name, Enum value)
        {
            builder.AppendFormat("{0} : {1}\n", name, value.ToString());
        }
    }
}