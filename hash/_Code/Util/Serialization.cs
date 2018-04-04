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

        private static SerializableObject GetSerializableObjectFromType(Type type, object obj)
        {
            SerializableObject result = new SerializableObject();

            result.BoolValues = new List<SerializableMember<bool>>();
            result.IntValues = new List<SerializableMember<int>>();
            result.FloatValues = new List<SerializableMember<float>>();
            result.StringValues = new List<SerializableMember<string>>();
            result.ComplexValues = new List<SerializableMember<SerializableObject>>();
            result.ArrayValues = new List<SerializableMember<Array>>();

            MemberInfo[] members = type.GetMembers();

            int len = members.Length;
            for (int i = 0; i < len; i++)
            {
                MemberInfo info = members[i];
                MemberTypes memberTypeType = info.MemberType;

                if ((memberTypeType & (MemberTypes.Field | MemberTypes.Property)) != 0)
                {
                    bool serialize = true;
                    string name = info.Name;

                    IEnumerable<CustomAttributeData> attributes = info.CustomAttributes;
                    foreach (var attributeData in attributes)
                    {
                        if (attributeData.AttributeType == typeof(MemberIgnoreAttribute))
                        {
                            serialize = false;
                            break;
                        }
                        else if (attributeData.AttributeType == typeof(MemberNameAttribute))
                        {
                            // Name attribute only has one argument
                            name = attributeData.ConstructorArguments[0].Value as string;
                        }
                    }

                    if ((memberTypeType & MemberTypes.Property) != 0)
                    {
                        PropertyInfo property = type.GetProperty(info.Name);
                        if (!property.CanWrite)
                            serialize = false;
                    }

                    if (serialize)
                    {
                        Type memberType;
                        object memberValue;
                        if ((memberTypeType & MemberTypes.Field) != 0)
                        {
                            FieldInfo field = type.GetField(info.Name);
                            memberType = field.FieldType;
                            memberValue = field.GetValue(obj);
                        }
                        else
                        {
                            PropertyInfo property = type.GetProperty(info.Name);
                            memberType = property.PropertyType;
                            memberValue = property.GetValue(obj);
                        }

                        HandleMember(name, memberType, memberValue, result);
                    }
                }
            }

            return result;
        }

        private static void HandleMember(string name, Type type, object value, SerializableObject obj)
        {
            // Don't handle generic types yet!
            if (type.IsGenericType)
                return;

            bool isPrimitive = type.IsPrimitive || type == typeof(string) || type.IsEnum;

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
                else if (type == typeof(string) || type.IsEnum)
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

        private static void WriteBool(StringBuilder builder, string name, bool value)
        {
            builder.AppendFormat("{0} : {1}\n", name, value ? "true" : "false");
        }

        private static void WriteInt(StringBuilder builder, string name, int value)
        {
            builder.AppendFormat("{0} : {1:D}\n", name, value);
        }

        private static void WriteFloat(StringBuilder builder, string name, float value)
        {
            builder.AppendFormat("{0} : {1:F}\n", name, value);
        }

        private static void WriteString(StringBuilder builder, string name, string value)
        {
            builder.AppendFormat("{0} : \"{1}\"\n", name, value);
        }

        public static void WriteComplex(StringBuilder builder, string name, SerializableObject value)
        {
            builder.AppendFormat("{0} : {{\n", name);
            WriteObject(value, builder);
            builder.AppendFormat("}}\n");
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
                                   elementType == typeof(string) || 
                                   elementType.IsEnum;

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
                    else if (elementType == typeof(string) || elementType.IsEnum)
                    {
                        string valueStr;
                        if (elementType.IsEnum)
                            valueStr = Enum.GetName(elementType, elementValue);
                        else
                            valueStr = (string) elementValue;
                        
                        WriteString(builder, i.ToString(), valueStr);
                    }
                }
                else
                {
                    SerializableObject serializableObject =  GetSerializableObjectFromType(
                        elementType, 
                        elementValue
                    );
                    
                    WriteComplex(builder, i.ToString(), serializableObject);
                }
            }

            builder.Append("]\n");
        }
    }
}