using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Kursach
{
    public partial class Connect : Form
    {
        public event EventHandler<ConnectEventArgs> ConnectEvent;

        public Connect()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (textBoxIp.Text != "" && textBoxPort.Text != "")
            {
                Regex ipregex = new Regex(@"^(?:25[0-5]|2[0-4]\d|[0-1]?\d{1,2})(?:\.(?:25[0-5]|2[0-4]\d|[0-1]?\d{1,2})){3}$");
                Regex portregex = new Regex(@"[0-9]{1,5}");
                if (ipregex.IsMatch(textBoxIp.Text) && portregex.IsMatch(textBoxPort.Text))
                {
                    string ip = textBoxIp.Text;
                    int port = int.Parse(textBoxPort.Text);
                    ConnectEvent?.Invoke(this, new ConnectEventArgs(ip, port));
                }
                else
                {
                    MessageBox.Show("Некорректный IP-адрес или порт.");
                }
            }
        }

        public class ConnectEventArgs : EventArgs
        {
            public string IP { get; set; }
            public int Port { get; set; }

            public ConnectEventArgs(string ip, int port)
            {
                IP = ip;
                Port = port;
            }
        }
    }
}
