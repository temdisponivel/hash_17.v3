using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.AccessControl;
using hash.Util;

namespace hash
{
    public enum FileType
    {
        Image,
        Text,
    }
    
    public struct File
    {
        public FileType FileType; 
        [MemberName("ID_DOIDO")]
        public int Id;
        public string Name;
        public string RealFilePath;
        public float FloatValue;
        [MemberName("COMPLEX_DIRECTORY")]
        public Device Device;
        [MemberName("COMPLEX_DIRECTORY")]
        public Device[] Devices;
        [MemberIgnore]
        public int NonSerializable;
        public bool BoolValueTrue;
        public bool BoolValueFalse;
        public char CharValue;
        public Directory Directory;
        [MemberName("FILE_TYPES")]
        public FileType[] FileTypes;
    }

    public struct Directory
    {
        public int Id;
        public string Name;
        public string[] ChildrenDirectoriesIds;
        public string[] FilesIds;
    }
    
    public struct Storage
    {
        public int Id;
        public string[] AllFilesIds;
        public string[] AllDirectoriesIds;
    }
    
    public struct Device
    {
        public int[] Values;
        public int Id;
        public string Name;
        public string IP;
        public string StorageId;
    }
    
    public struct DeviceData
    {
        public Device[] AllDevices;
        public Storage[] AllStorages;
        public File[] AllFiles;
        public Directory[] AllDirectories;
    }
    
    public static class DeviceUtil 
    {
        public static bool FindDevice(int id, DeviceData deviceData, out Device device)
        {
            device = default(Device);
            
            int len = deviceData.AllDevices.Length;
            for (int i = 0; i < len; i++)
            {
                device = deviceData.AllDevices[i];
                if (device.Id == id)
                    return true;
            }

            return false;
        }

        public static bool FindStorage(int  id, DeviceData deviceData, out Storage storage)
        {
            storage = default(Storage);
            
            int len = deviceData.AllStorages.Length;
            for (int i = 0; i < len; i++)
            {
                storage = deviceData.AllStorages[i];
                if (storage.Id == id)
                    return true;
            }

            return false;
        }
        
        public static bool FindDirectory(int  id, DeviceData deviceData, out Directory directory)
        {
            directory = default(Directory);
            
            int len = deviceData.AllDirectories.Length;
            for (int i = 0; i < len; i++)
            {
                directory = deviceData.AllDirectories[i];
                if (directory.Id == id)
                    return true;
            }

            return false;
        }
        
        public static bool FindFile(int  id, DeviceData deviceData, out File file)
        {
            file = default(File);
            
            int len = deviceData.AllFiles.Length;
            for (int i = 0; i < len; i++)
            {
                file = deviceData.AllFiles[i];
                if (file.Id == id)
                    return true;
            }

            return false;
        }
        
        public static void OpenFile(File file)
        {
            Process.Start(file.RealFilePath);
        }    
    }

    public static class DeviceSerializationUtil
    {
        
    }
}