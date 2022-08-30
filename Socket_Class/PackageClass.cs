using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Package_Item
{
    public class PackageClass : IDisposable
    {
        public const int PackageSize = 1024 * 1024 * 10;
        public byte[] Data { get; set; }
        public Socket ConnectSocket { get; set; }

        public PackageClass()
        {
            Data = new byte[PackageSize];
        }

        public void Dispose()
        {

        }

        public static byte[] ToPack(bool IsCMD, byte[] dataByte)
        {
            byte[] DataLenght = new byte[5];
            DataLenght[0] = Convert.ToByte(IsCMD);
            Array.Copy(BitConverter.GetBytes(dataByte.Length), 0, DataLenght, 1, 4);
            return DataLenght.Concat(dataByte).ToArray();
        }

        public static byte[] UnPack(byte[] dataByte, out bool IsCMD)
        {
            IsCMD = Convert.ToBoolean(dataByte[0]);

            byte[] DataLenght = dataByte;
            int DataCarry = BitConverter.ToInt32(dataByte.Skip(1).Take(4).ToArray(), 0);
            return dataByte.Skip(5).Take(DataCarry).ToArray();
        }
    }
}
