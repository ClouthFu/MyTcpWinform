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
        public void ToPacketTest()
        {
            byte[] Result = PackageClass.ToPacket(new byte[10]);

            byte[] LenghtByte = new byte[10];
            byte[] Ans = new byte[] { (byte)10 >> 24 , (byte)10 >>16, (byte)10 >>8, (byte)10 };

            byte[] AnsLike = Ans.Concat<byte>(LenghtByte).ToArray();

            Assert.AreSame(AnsLike, PackageClass.ToPacket(10));
        }
    }
}