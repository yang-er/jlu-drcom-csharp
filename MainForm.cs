using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DrClient
{
    public partial class MainForm : Form
    {
        
        public MainForm()
        {
            InitializeComponent();
            aid = AddItem;
            ssd = SetState;
            uipd = UpdateIP;
            proc.OnMakeLog += Log;
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.Speed < 0) continue;
                if (ni.OperationalStatus == OperationalStatus.Down) continue;

                if (ni.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet
                    || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                    || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    MacAddress n = new MacAddress(ni);
                    comboBox1.Items.Add(n);
                    if (comboBox1.SelectedItem == null)
                        comboBox1.SelectedItem = n;
                }
            }
            LoadMacFromRegistry();
            LoadUserFromRegister();
            notifyIcon1.Icon = Properties.Resources.Icon2;
        }
        
        private void Button1_Click(object sender, EventArgs e)
        {
            if(!CheckUserInfo()) return;
            pThread = new Thread(ThreadProcess);
            pThread.Start(proc);
        }
        
        private void Button2_Click(object sender, EventArgs e)
        {
            pThread.Abort("logout");
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            listBox1.Visible = checkBox1.Checked;
            if (checkBox1.Checked)
            {
                Height = 700;
            }
            else
            {
                listBox1.Items.Clear();
                Height = 432;
            }
        }

        private bool CheckUserInfo()
        {
            if (textBox4.Text == "" || textBox3.Text == "") return false;
            proc.Cert = new DrCert(textBox4.Text, textBox3.Text, ((MacAddress)comboBox1.SelectedItem).MAC.GetAddressBytes(), Log);
            SaveUserFromRegister();
            return true;
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (button2.Enabled) {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void ComboBox1_SelectedValueChanged(object sender, EventArgs e)
        {

        }

    }
}
