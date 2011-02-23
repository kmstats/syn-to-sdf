using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace com.echo.PDA
{
    static class ExportSdf
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
