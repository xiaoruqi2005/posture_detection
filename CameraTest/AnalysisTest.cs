using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analysis;

namespace CameraTest
{
    class AnalysisTest
    {
        public static void Main()
        {
            /*               Console.WriteLine("--- 开始进行测试  ---");
                        Posenalyzer ana = new Posenalyzer();
                       await ana.StartAsync();*/
            /*  new Thread(()=> {
                   Posenalyzer ana = new Posenalyzer();
                   ana.StartAsync().Wait();
               }).Start();*/


            Posenalyzer ana = new Posenalyzer();
            // 最终优化版
            ana.StartAsync();
            while (true)
            {//Console.WriteLine("程序没有阻塞！"); } 

            }
        }
    }
}
