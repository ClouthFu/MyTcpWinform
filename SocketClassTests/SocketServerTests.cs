using Microsoft.VisualStudio.TestTools.UnitTesting;
using Socket_Class;
using SocketClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket_Class.Tests
{
    [TestClass()]
    public class SocketServerTests
    {
        [TestMethod()]
        public void StartServiceSocket_ServerAndClientConnection()
        {
            SocketServer socketServer = new SocketServer();
            socketServer.StartService(System.Net.IPAddress.Parse("127.0.0.1"), 5000);

            SocketClient socketClient = new SocketClient();
            socketClient.StartService(System.Net.IPAddress.Parse("127.0.0.1"), 5000);

            string serverInfoStr = socketServer.MainSocket.RemoteEndPoint.ToString();
            string clientInfoStr = socketClient.MainSocket.LocalEndPoint.ToString();


            Assert.Fail();
        }
    }
}