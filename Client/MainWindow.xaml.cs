using SocketClass;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketClient client;
        public MainWindow()
        {
            InitializeComponent();
            txtGetFileName.Text = "ServerTest.exe";
            client = new SocketClient();
            client.GetStatusCMD += Client_GetStatusCMD;
            client.StartService(IPAddress.Parse("127.0.0.1"), 5000);
        }

        private void Client_GetStatusCMD(string obj)
        {
            Dispatcher.BeginInvoke(() => 
            {
                txtStatus.Text = obj;
            });
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            client.FileName = txtGetFileName.Text;
            client.SavePath = txtSaveFilePath.Text;
            client.SendData();
        }

        private void txtSaveFilePath_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtSaveFilePath.Text = folderBrowserDialog.SelectedPath + "\\";
            }
        }
    }
}
