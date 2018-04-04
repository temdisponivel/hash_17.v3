using System;
using hash.Util;

namespace hash
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(@"Opening: C:\code\hash\hash\Data\test_file_img.png");
            File file = new File();
            file.Id = 50004;
            file.Name = "DOIDO_MEMO";
            file.RealFilePath = @"C:\code\hash\hash\_Data\test_file_img.png";
            file.FloatValue = 1003.44f;
            file.Device = new Device();
            file.Device.Values = new[] {5, 9, 8, 52};
            file.Device.Id = 39;
            file.Device.IP = "156.568.4.5";
            file.Device.StorageId = "888";
            file.FileType = FileType.Image;
            file.Devices = new Device[10];
            for (int i = 0; i < 10; i++)
            {
                file.Devices[i] = file.Device;
            }
            file.BoolValueFalse = false;
            file.BoolValueTrue = true;
            file.Directory = new Directory();
            file.Directory.Id = 98982236;
            file.Directory.Name = "DIR_NAME_MALUCO";
            file.Directory.FilesIds = new string[] {"10", "55", "99","85"};
            file.Directory.ChildrenDirectoriesIds = new string[] {"ALLLALALAL"};
            DeviceUtil.OpenFile(file);
            byte[] serialized = Serialization.Serialize(file);
            Console.WriteLine(Serialization.ByteArrayToString(serialized));
            Console.ReadKey();
        }
    }
}