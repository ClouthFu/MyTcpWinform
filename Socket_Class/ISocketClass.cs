using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Socket_Class
{
    internal interface ISocketClass
    {
        Socket MainSocket { get; set; }
        string CommandStr { get; set; }

        void StartService(IPAddress IP, ushort Port);
        void StopService();
        void StableConnection(IAsyncResult ar);
        void SendData(IAsyncResult ar);
        void WaitReciveData(Socket ReciveSocket);
        void ReceiveData(IAsyncResult ar);
    }
}
