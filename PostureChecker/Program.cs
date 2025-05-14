using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analysis;

namespace PostureChecker
{
    internal static class Program
    {
        

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //DataBase data = new DataBase();
            //data.MySqlOp();
            ApplicationConfiguration.Initialize();

            //Menu
            //Application.Run(new Menu());
            Application.Run(new Menu());

        }
    }
}