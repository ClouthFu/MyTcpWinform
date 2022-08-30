using Package_Item;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
    /// <summary>
    /// ConnetedItem.xaml 的互動邏輯
    /// </summary>
    public partial class ConnetedItem : UserControl
    {
        public Socket keepSocket { get; private set; }

        public string GetFileName 
        {
            get { return lbl_FileName.Text; }
            set { lbl_FileName.Text = value; }
        }

        public ConnetedItem(Socket InConnection)
        {
            InitializeComponent();
            keepSocket = InConnection;
            lbl_IP_Port.Text = InConnection.RemoteEndPoint.ToString();
            lbl_FileName.Text = "";

        }
    }

    public enum EnumStatus
    {
        Idle,
        Done,
        Error,
    }
}
