using Socket_Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server
{
    public partial class MainWindow : Window
    {
        SocketServer server;
        public MainWindow()
        {
            InitializeComponent();
            server = new SocketServer();
            server.Connection += Server_Connection;
            server.Disconnection += Server_Disconnection;
            server.SocketCMD += Server_SocketCMD;

            server.StartService(IPAddress.Parse("127.0.0.1"), 5000);
        }

        private void Server_SocketCMD(System.Net.Sockets.Socket arg1, string arg2)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ConnetedItem temp = (from ConnetedItem item in lvConnectionViewer.Items
                                     where item.keepSocket == arg1
                                     select item).First();
                temp.GetFileName = arg2;
            }));
        }

        private void Server_Connection(System.Net.Sockets.Socket obj)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ConnetedItem item = new ConnetedItem(obj);
                lvConnectionViewer.Items.Add(item);
            }));
        }

        private void Server_Disconnection(System.Net.Sockets.Socket obj)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ConnetedItem temp = (from ConnetedItem item in lvConnectionViewer.Items
                                     where item.keepSocket == obj
                                     select item).First();

                lvConnectionViewer.Items.Remove(temp);
            }));
        }


    }
}
