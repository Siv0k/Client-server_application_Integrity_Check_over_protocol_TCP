using System;
using System.Windows.Forms;
using Clients;
namespace Kursach
{
    public partial class MainForm : Form
    {
        private Client client;
        private Form activeForm = null;

        public MainForm()
        {
            InitializeComponent();
            openChildForm(new Connect());
        }

        private void openChildForm(Form child)
        {
            if (ActiveForm != null)
            {
                activeForm.Close();
            }
            activeForm = child;
            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;
            panelForm.Controls.Add(child);
            panelForm.Tag = child;
            child.BringToFront();
            if (child is Connect)
            {
                ((Connect)child).ConnectEvent += connect_ConnectEvent;
            }
            child.Show();
        }

        private void connect_ConnectEvent(object sender, Connect.ConnectEventArgs e)
        {
            startClient(e.IP, e.Port);
        }

        private void startClient(string ip, int port)
        {
            client = new Client(ip, port); 
            client.Connect();
            openChildForm(new GlobalForm(client));
        }

        private void panelForm_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
