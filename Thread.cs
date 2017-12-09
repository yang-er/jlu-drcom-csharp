using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DrClient
{
    public partial class MainForm
    {

        volatile DrProtocol proc = new DrProtocol();
        AddItemDelegate aid;
        SetStateDelegate ssd;
        UpdateIPDelegate uipd;

        Thread pThread;
        int trytimes = 0;
        
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
                Log("logout", "logging out...", false);
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
            textBox3.ReadOnly = isLogin;
            textBox4.ReadOnly = isLogin;
            comboBox1.Enabled = !isLogin;
            checkBox2.Enabled = !isLogin;
            notifyIcon1.Icon = isLogin ? Properties.Resources.Icon1 : Properties.Resources.Icon2;
            if (!isLogin)
                notifyIcon1.ShowBalloonTip(10000, "校园网", "已登出。" + toolTip, toolTip.Length == 0 ? ToolTipIcon.Info : ToolTipIcon.Error);
        }
        
    }
}
