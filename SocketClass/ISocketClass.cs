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
        event Action<Socket> Disconnection;

        void StartService(IPAddress IP, ushort Port);
        void StopService();
        void StableConnection(IAsyncResult ar);
        //void SendData(Socket SendSocket);
        void SendData();
        void EndSendData(IAsyncResult ar);
        void WaitReciveData(Socket ReciveSocket);
        void ReceiveData(IAsyncResult ar);
    }
}
