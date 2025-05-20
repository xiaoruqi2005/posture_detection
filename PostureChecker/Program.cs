using PostureChecker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Data());
            DataBase db = new DataBase();
            List<string[]> result = db.Table();
            foreach (var row in result)
            {
                Console.WriteLine(string.Join("  ", row));
            }
        }
    }

}
