using Package_Item;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socket_Class
{
    public class SocketServer : ISocketClass
    {
        ManualResetEvent ConnectionHandle;
        ManualResetEvent SendDone;
        public Socket MainSocket { get; set; }
        public string CommandStr { get; set; }

        public List<Socket> ConnectClients { get; private set; }

        public event Action<Socket> Connection;
        public event Action<Socket> Disconnection;
        public event Action<Socket, string> SocketCMD;

        public SocketServer()
        {
            ConnectionHandle = new ManualResetEvent(false);
            SendDone = new ManualResetEvent(false);
            CommandStr = string.Empty;
            ConnectClients = new List<Socket>();
        }



        public void StartService(IPAddress IP, ushort Port)
        {
            MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            MainSocket.Bind(new IPEndPoint(IP, Port));
            MainSocket.Listen(100);

            Task.Factory.StartNew(() =>
            {
                Trace.WriteLine("Server Start Listen");
                while (true)
                {
                    ConnectionHandle.Reset();
                    MainSocket.BeginAccept(new AsyncCallback(StableConnection), MainSocket);
                    ConnectionHandle.WaitOne();
                }
            });
        }

        public void StableConnection(IAsyncResult ar)
        {
            Socket EndAcceptSocket = (Socket)ar.AsyncState;
            Socket BeginReciveSocket = EndAcceptSocket.EndAccept(ar);

            ConnectClients.Add(BeginReciveSocket);
            BeginReciveSocket.ReceiveBufferSize = PackageClass.PackageSize;
            ConnectionHandle.Set();
            Trace.WriteLine(string.Format("Server Connection Stable IP=>{0}", BeginReciveSocket.RemoteEndPoint.ToString()));

            if (Connection != null)
                Connection(BeginReciveSocket);

            Task.Factory.StartNew(() =>
            {
                WaitReciveData(BeginReciveSocket);
            });
        }
        public void WaitReciveData(Socket ReciveSocket)
        {
            PackageClass RecivePackage = new PackageClass() { ConnectSocket = ReciveSocket };
            ReciveSocket.BeginReceive(RecivePackage.Data, 0, PackageClass.PackageSize, SocketFlags.None, new AsyncCallback(ReceiveData), RecivePackage);
        }
        public void ReceiveData(IAsyncResult ar)
        {
            try
            {
                PackageClass ReciveSocket = (PackageClass)ar.AsyncState;
                Socket EndReciveSocket = ReciveSocket.ConnectSocket;
                int byteCount = EndReciveSocket.EndReceive(ar);

                byte[] tempData = PackageClass.UnPack(ReciveSocket.Data, out bool IsCmd);
                if (byteCount > 0)
                {
                    string tempStr = Encoding.ASCII.GetString(tempData, 0, tempData.Length);
                    CommandStr += tempStr;
                    Trace.WriteLine("GetCMD:" + CommandStr);
                    if (SocketCMD != null)
                        SocketCMD(EndReciveSocket, CommandStr);
                    string Status = "Error! Not Such File Name.";
                    string TempPath = string.Format("..\\net6.0-windows\\{0}", CommandStr);
                    if (File.Exists(TempPath))
                    {
                        FileSendSocket(TempPath, EndReciveSocket);
                        Status = "Successful";
                    }

                    byte[] tempStatusData = Encoding.ASCII.GetBytes(Status);
                    foreach (byte[] Onetime in tempStatusData.Chunk(PackageClass.PackageSize - 5))
                    {
                        byte[] tempSendData = PackageClass.ToPack(true, Onetime);
                        Thread.Sleep(1000);
                        EndReciveSocket.BeginSend(tempSendData, 0, tempSendData.Length, SocketFlags.None, new AsyncCallback(EndSendData), EndReciveSocket);
                    }

                    Trace.WriteLine("SendCMD");
                    CommandStr = string.Empty;
                    GC.Collect();
                    WaitReciveData(EndReciveSocket);
                }
            }
            catch (SocketException se)
            {
                PackageClass ReciveSocket = (PackageClass)ar.AsyncState;
                Socket endSocket = ReciveSocket.ConnectSocket;
                endSocket.BeginDisconnect(false, new AsyncCallback(DisconnectSocket), endSocket);              
            }
        }
        private void FileSendSocket(string SendFilePath, Socket SendSocket)
        {
            using (FileStream SendFS = new FileStream(SendFilePath, FileMode.Open, FileAccess.Read))
            {
                byte[] TempDataBytes = new byte[SendFS.Length];
                SendFS.Read(TempDataBytes, 0, TempDataBytes.Length);
                SendFS.Close();

                List<byte[]> tempBytesList = TempDataBytes.Chunk(PackageClass.PackageSize - 5).ToList();
                foreach (byte[] tempData in tempBytesList)
                {
                    SendDone.Reset();
                    byte[] tempSendData = PackageClass.ToPack(false, tempData);
                    SendSocket.BeginSend(tempSendData, 0, tempSendData.Length, SocketFlags.None, new AsyncCallback(EndSendData), SendSocket);
                    SendDone.WaitOne();
                }
                tempBytesList.Clear();
                Trace.WriteLine("SendFile");
            }
        }
        public void EndSendData(IAsyncResult ar)
        {
            Socket SendEndSocket = (Socket)ar.AsyncState;
            int temp = SendEndSocket.EndSend(ar, out SocketError errorCode);
            SendDone.Set();
        }

        private void DisconnectSocket(IAsyncResult asyncResult)
        {
            Socket DisSocket = (Socket)asyncResult.AsyncState;
            DisSocket.EndDisconnect(asyncResult);

            DisSocket.Shutdown(SocketShutdown.Both);
            DisSocket.Close();

            ConnectClients.Remove(DisSocket);

            if (Disconnection != null)
                Disconnection(DisSocket);
        }

        public void StopService()
        {
            throw new NotImplementedException();
        }

        public void SendData()
        {
        }
    }
}
