using PostureChecker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Analysis;

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
            // 在需要插入数据的位置调用
            AnalysisResult result = new AnalysisResult();// 实际应使用分析后的结果
            Console.WriteLine(result.ToString());
            DataBase db = new DataBase();
            db.InsertAnalysisResult(result);
            //List<string[]> result_1 = db.Table();
            //foreach (var row in result_1)
            //{
            //    Console.WriteLine(string.Join("  ", row));
            //}
        }
    }

}
