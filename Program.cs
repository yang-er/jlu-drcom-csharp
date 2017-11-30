using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Windows.Forms;

namespace DrClient
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(new string[] {});
        }

        /// <summary>
        /// 应用程序的单实例管理器。
        /// </summary>
        public class SingleInstanceManager : WindowsFormsApplicationBase
        {
            MainForm app;

            public SingleInstanceManager()
            {
                this.IsSingleInstance = true;
            }

            protected override bool OnStartup(StartupEventArgs e)
            {
                app = new MainForm();
                Application.Run(app);
                return false;
            }

            protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
            {
                base.OnStartupNextInstance(eventArgs);
                app.Show();
                app.Activate();
            }

        }
    }
}
