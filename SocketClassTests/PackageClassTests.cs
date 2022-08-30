using Microsoft.VisualStudio.TestTools.UnitTesting;
using Package_Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Package_Item.Tests
{
    [TestClass()]
    public class PackageClassTests
    {
        [TestMethod()]
        public void ToPackSocketDataBytes_ToPack_IsCMD()
        {
            byte[] testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            byte[] Header = new byte[5];
            Header[0] = Convert.ToByte(true);
            BitConverter.GetBytes(testData.Length).CopyTo(Header, 1);
            byte[] ans = Header.Concat(testData).ToArray();

            byte[] getPack = PackageClass.ToPack(true, testData);


            bool pass = true;
            for (int i = 0; i < ans.Length; i++)
            {
                if (ans[i] != getPack[i])
                {
                    pass = false;
                    break;
                }
            }

            Assert.IsTrue(pass);
        }

        [TestMethod()]
        public void UnPackSocketDataBytes_UnPack_IsCMD()
        {
            byte[] testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            byte[] ToPack = PackageClass.ToPack(true, testData);
            byte[] UnPack = PackageClass.UnPack(ToPack, out bool IsCMD);

            bool pass = true;
            for (int i = 0; i < testData.Length; i++)
            {
                if (testData[i] != UnPack[i])
                {
                    pass = false;
                    break;
                }
            }

            if (!IsCMD)
                pass = false;


            Assert.IsTrue(pass);
        }
    }
}