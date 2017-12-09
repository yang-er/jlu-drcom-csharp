using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DrClient
{

    public partial class MainForm : Form
    {

        private void LoadMacFromRegistry()
        {
            RegistryKey mac = Registry.CurrentUser.CreateSubKey("Software\\YangerSoft\\drcom\\mac");
            foreach(string p in mac.GetValueNames())
            {
                comboBox1.Items.Add(new MacAddress(p, (long) mac.GetValue(p)));
            }
        }

        private void LoadUserFromRegister()
        {
            RegistryKey user = Registry.CurrentUser.CreateSubKey("Software\\YangerSoft\\drcom\\user");
            string[] list = user.GetValueNames();
            if (list.Length == 0) return;
            textBox4.Text = list[0];
            textBox3.Text = (string) user.GetValue(textBox4.Text);
            checkBox2.Checked = true;
        }

        private void SaveUserFromRegister()
        {
            if (!checkBox2.Checked) return;
            RegistryKey user = Registry.CurrentUser.CreateSubKey("Software\\YangerSoft\\drcom\\user");
            user.SetValue(textBox4.Text, textBox3.Text);
        }

    }

}
