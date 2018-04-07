using System;
using System.Collections.Generic;
using System.Diagnostics;
using SimpleCollections.Lists;

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
        public int Id;
        public string Name;
        public string RealFilePath;
    }

    public struct Directory
    {
        public int Id;
        public string Name;
        public SimpleList<int> ChildrenDirectoriesIds;
        public SimpleList<int> FilesIds;
    }

    public struct Storage
    {
        public int Id;
        public SimpleList<int> AllFilesIds;
        public SimpleList<int> AllDirectoriesIds;
    }

    public struct Device
    {
        public int Id;
        public string Name;
        public string IP;
        public int StorageId;
    }

    public struct DeviceData
    {
        public SimpleList<Device> AllDevices;
        public SimpleList<Storage> AllStorages;
        public SimpleList<File> AllFiles;
        public SimpleList<Directory> AllDirectories;
    }

    public static class DeviceUtil
    {
        public const string ROOT_DIR_NAME = "/";

        public static bool FindDevice(int id, DeviceData deviceData, out Device device)
        {
            device = default(Device);

            int len = deviceData.AllDevices.Count;
            for (int i = 0; i < len; i++)
            {
                device = deviceData.AllDevices[i];
                if (device.Id == id)
                    return true;
            }

            return false;
        }

        public static bool FindStorage(int id, DeviceData deviceData, out Storage storage)
        {
            storage = default(Storage);

            int len = deviceData.AllStorages.Count;
            for (int i = 0; i < len; i++)
            {
                storage = deviceData.AllStorages[i];
                if (storage.Id == id)
                    return true;
            }

            return false;
        }

        public static bool FindDirectory(int id, DeviceData deviceData, out Directory directory)
        {
            directory = default(Directory);

            int len = deviceData.AllDirectories.Count;
            for (int i = 0; i < len; i++)
            {
                directory = deviceData.AllDirectories[i];
                if (directory.Id == id)
                    return true;
            }

            return false;
        }

        public static bool FindFile(int id, DeviceData deviceData, out File file)
        {
            file = default(File);

            int len = deviceData.AllFiles.Count;
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

        public static int CreateDevice(DeviceData deviceData)
        {
            Device newDevice = new Device();
            newDevice.Id = new Random().Next(); // TODO: Real id
            newDevice.Name = "NEW_DEVICE";
            newDevice.StorageId = CreateStorage(deviceData);
            newDevice.IP = "192.168.0.1";

            SList.Add(deviceData.AllDevices, newDevice);

            return newDevice.Id;
        }

        public static int CreateStorage(DeviceData deviceData)
        {
            Storage storage = new Storage();
            storage.Id = new Random().Next(); // TODO: Real id
            storage.AllDirectoriesIds = new SimpleList<int>();
            storage.AllFilesIds = new SimpleList<int>();

            int root = CreateDirectory(deviceData, ROOT_DIR_NAME);
            SList.Add(storage.AllDirectoriesIds, root);

            SList.Add(deviceData.AllStorages, storage);

            return storage.Id;
        }

        public static int CreateDirectory(DeviceData deviceData, string name)
        {
            Directory dir = new Directory();
            dir.Id = new Random().Next(); // TODO: Real id
            dir.ChildrenDirectoriesIds = new SimpleList<int>();
            dir.FilesIds = new SimpleList<int>();
            dir.Name = name;

            SList.Add(deviceData.AllDirectories, dir);

            return dir.Id;
        }
    }
}