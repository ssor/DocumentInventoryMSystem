using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace InventoryMSystem
{
    static class Program
    {
      public static  DateTime timeBase = DateTime.Now;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
