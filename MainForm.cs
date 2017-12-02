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

        volatile DrProtocol proc = new DrProtocol();
        AddItemDelegate aid;
        SetStateDelegate ssd;
        UpdateIPDelegate uipd;

        Thread pThread;
        int trytimes = 0;

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
            notifyIcon1.Icon = Properties.Resources.Icon2;
        }

        void ThreadProcess(object args)
        {
            this.Invoke(ssd, true, "");
            try
            {
                proc = (DrProtocol)args;
                proc.Initialize();
                int ret = 0;
                trytimes = 5;
                while (true)
                {
                    if (trytimes >= 0 && ret++ > trytimes)
                    {
                        Log("login", "! try over times, login fail!", false);
                        throw new DrException() { Source = "登录失败次数过多。" };
                    }

                    int p = proc.Challenge(ret);
                    if (p == -5) throw new DrException() { Source = proc.InnerSource };
                    if (p < 0) continue;

                    p = proc.Login();
                    if (p == -5) throw new DrException() { Source = proc.InnerSource };
                    if (p < 0) continue;

                    this.Invoke(uipd);
                    break;
                }
                while (true)
                {
                    ret = 0;
                    int p;
                    while ((p = proc.Alive()) != 0)
                    {
                        if (p == -5) throw new DrException() { Source = proc.InnerSource };
                        if (trytimes >= 0 && ret++ > trytimes)
                        {
                            Log("alive", "alive(): fail;", false);
                            throw new DrException() { Source = "Keep-alive包发送超时多次。" };
                        }
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(20000);
                }
            }
            catch (ThreadAbortException)
            {
                Log("logout", "logging out, may need at least 20 seconds... ", false);
                this.Invoke(ssd, false, "");
            }
            catch (DrException e)
            {
                Log("drcom", "Socket closed. Please redail. ", false);
                this.Invoke(ssd, false, e.Source);
            }
            // this.Invoke(ssd, false, " ");
        }

        private void Log(string app, object args, bool toHex)
        {
            if (!checkBox1.Checked) return;
            string ept;
            if (toHex)
            {
                char[] chars = "0123456789ABCDEF".ToCharArray();
                byte[] bs = (byte[])args;
                StringBuilder sb = new StringBuilder("");
                int bit;
                for (int i = 0; i < bs.Length; i++)
                {
                    bit = (bs[i] & 0x0f0) >> 4;
                    sb.Append(chars[bit]);
                    bit = bs[i] & 0x0f;
                    sb.Append(chars[bit]);
                }
                ept = sb.ToString();
                sb.Clear();
            }
            else
            {
                ept = (string)args;
            }
            try
            {
                this.Invoke(aid, string.Format("[{0}] {1}", app, ept));
            }
            catch (InvalidOperationException)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private delegate void AddItemDelegate(string str);

        private void AddItem(string str)
        {
            listBox1.Items.Add(str);
        }

        private delegate void UpdateIPDelegate();

        private void UpdateIP()
        {
            textBox2.Text = proc.Cert.ClientIP;
        }

        private delegate void SetStateDelegate(bool isLogin, string toolTip = "");

        private void SetState(bool isLogin, string toolTip = "")
        {
            button1.Enabled = !isLogin;
            button2.Enabled = isLogin;
            notifyIcon1.Icon = isLogin ? Properties.Resources.Icon1 : Properties.Resources.Icon2;
            if (!isLogin)
                notifyIcon1.ShowBalloonTip(10000, "校园网", "已登出。" + toolTip, toolTip.Length == 0 ? ToolTipIcon.Info : ToolTipIcon.Error);
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
            proc.Cert = new DrCert(textBox4.Text, textBox3.Text, ((MacAddress)comboBox1.SelectedItem).MAC, Log);
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

    }
}
