using Package_Item;
using Socket_Class;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SocketClass
{
    public class SocketClient : ISocketClass
    {
        public Socket MainSocket { get; set; }
        public string CommandStr { get; set; }

        public event Action<Socket> Disconnection;
        public event Action<string> GetStatusCMD;

        public IPAddress IP_Remote { get; set; }
        public ushort Port_Remote { get; set; }
        public string FileName { get; set; }
        public string SavePath { get; set; }
        List<byte[]> DataPackages;

        public SocketClient()
        {
            DataPackages = new List<byte[]>();
        }

        public void StartService(IPAddress IP, ushort Port)
        {
            IP_Remote = IP;
            Port_Remote = Port;
            StartConnect();
        }
        public void StartConnect()
        {
            if (MainSocket == null || !MainSocket.Connected)
            {
                Trace.WriteLine("Client Start Connect");
                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                MainSocket.BeginConnect(IP_Remote, Port_Remote, new AsyncCallback(StableConnection), MainSocket);
            }
        }
        public void StableConnection(IAsyncResult ar)
        {
            Socket EndAcceptSocket = (Socket)ar.AsyncState;
            EndAcceptSocket.EndConnect(ar);
            Trace.WriteLine("Client Connect Stable");
            Task.Factory.StartNew(() =>
            {
                WaitReciveData(EndAcceptSocket);
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
                if (!IsCmd)
                {
                    DataPackages.Add(tempData);
                }
                else
                {
                    string temp = Encoding.ASCII.GetString(tempData, 0, tempData.Length);
                    CommandStr += temp;
                }


                if (tempData.Length < PackageClass.PackageSize - 5)
                {
                    if (!IsCmd)
                    {
                        Task.Factory.StartNew(() => { StartProcessData(); });
                    }
                    else
                    {
                        if (GetStatusCMD != null)
                            GetStatusCMD(CommandStr);
                        Trace.WriteLine(CommandStr);
                        CommandStr = string.Empty;
                    }


                }


                WaitReciveData(ReciveSocket.ConnectSocket);
            }
            catch
            {

            }
        }
        public void StartProcessData()
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(SavePath + FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                foreach (byte[] item in DataPackages)
                {
                    bw.Write(item);
                }
                bw.Flush();
                bw.Close();
            }
            DataPackages.Clear();
            GC.Collect();
        }
        public void SendData()
        {
            StopService();
            StartConnect(); 
            //string fileName = txtGetFileName.Text;
            byte[] tempBytes = Encoding.ASCII.GetBytes(FileName);

            foreach (byte[] Onetime in tempBytes.Chunk(PackageClass.PackageSize - 5))
            {
                MainSocket.SendBufferSize = PackageClass.PackageSize;
                byte[] TempSendData = PackageClass.ToPack(true, Onetime);
                MainSocket.BeginSend(TempSendData, 0, TempSendData.Length, SocketFlags.None, new AsyncCallback(EndSendData), MainSocket);
            }
        }
        public void EndSendData(IAsyncResult ar)
        {
            Socket tempSend = (Socket)ar.AsyncState;
            tempSend.EndSend(ar);
        }
        public void StopService()
        {
            MainSocket.Close();
        }


    }
}
