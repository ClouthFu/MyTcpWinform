using Package_Item;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socket_Class
{
    public class SocketServer: ISocketClass
    {
        ManualResetEvent ConnectionHandle;
        public Socket MainSocket { get; set; }
        public string CommandStr { get; set; }

        public event Action<byte[]> ReciveDataEven;

        public SocketServer()
        {
            ConnectionHandle = new ManualResetEvent(false);
            CommandStr = string.Empty;
        }


        //public void EndStableConnection(IAsyncResult ar)
        //{
        //    Socket EndAcceptSocket = (Socket)ar.AsyncState;
        //    Socket BeginReciveSocket = EndAcceptSocket.EndAccept(ar);

        //    BeginReciveSocket.ReceiveBufferSize = PackageClass.PackageSize;

        //    Task.Factory.StartNew(() =>
        //    {
        //        WaitReciveData(BeginReciveSocket);
        //    });
        //}

        //public void WaitReciveData(Socket waitSocket)
        //{
        //    try
        //    {
        //        PackageClass pkg = new PackageClass()
        //        {
        //            ConnectSocket = waitSocket
        //        };
        //        waitSocket.BeginReceive(pkg.Data, 0, pkg.Data.Length, SocketFlags.None, new AsyncCallback(ReciveData), pkg);
        //    }
        //    catch (SocketException se)
        //    {

        //    }
        //}

        //public void ReciveData(IAsyncResult ar)
        //{
        //    PackageClass pkg = (PackageClass)ar.AsyncState;
        //    int GetLenght = pkg.ConnectSocket.EndReceive(ar);

        //    if (GetLenght >= 1)
        //    {
        //        if (ReciveDataEven != null)
        //            ReciveDataEven(pkg.Data);
                
        //        WaitReciveData(pkg.ConnectSocket);
        //    }

        //}

        public void SendData(IAsyncResult ar)
        {

            
        }

        public void StartService(IPAddress IP, ushort Port)
        {
            MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            MainSocket.Bind(new IPEndPoint(IP, Port));
            MainSocket.Listen(100);

            Task.Factory.StartNew(() =>
            {
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

            BeginReciveSocket.ReceiveBufferSize = PackageClass.PackageSize;
            ConnectionHandle.Set();

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
                int byteCount = ReciveSocket.ConnectSocket.EndReceive(ar);

                byte[] tempData = PackageClass.UnPack(ReciveSocket.Data, out bool IsCmd);
                if (byteCount > 0)
                {
                    string tempStr = Encoding.ASCII.GetString(tempData, 0, tempData.Length);
                    CommandStr += tempStr;
                    Trace.WriteLine("GetCMD:" + CommandStr);

                    string Status = "Error! Not Such File Name.";
                    string TempPath = string.Format("..\\{0}", CommandStr);
                    if (File.Exists(TempPath))
                    {
                        FileSendSocket(TempPath);
                        Status = "Successful";                    
                    }

                    byte[] tempStatusData = Encoding.ASCII.GetBytes(Status);
                    foreach (byte[] Onetime in tempStatusData.chunk(tempStatusData, 0, PackageClass.PackageSize-5))
                    {
                        byte[] tempSendData = PackageClass.ToPack(true, Onetime);
                        MainClient.BeginSend(tempSendData, 0, tempSendData.Length, SocketFlags.None, new AsyncCallback(SendEnd), MainClient);
                    }

                    Trace.WriteLine("SendCMD");
                    CommandStr = string.Empty;

                    SocketBeginRecive(ReciveSocket.ConnectSocket);
                }
            }
            catch (SocketException se)
            {
                PackageClass ReciveSocket = (PackageClass)asyncResult.AsyncState;
                Socket endSocket = ReciveSocket.ConnectSocket;
                endSocket.BeginDisconnect(false, new AsyncCallback(DisconnectSocket), endSocket);
            }
        }
        private void FileSendSocket(string SendFilePath)
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
                    //PackageClass tempSend = new PackageClass();
                    byte[] tempSendData = PackageClass.ToPack(false, tempData);
                    MainClient.BeginSend(tempSendData, 0, tempSendData.Length, SocketFlags.None, new AsyncCallback(SendEnd), MainClient);
                    SendDone.WaitOne();
                }
                tempBytesList.Clear();
                Trace.WriteLine("SendFile");
            }
        }
        public void StopService()
        {
            throw new NotImplementedException();
        }




    }
}
