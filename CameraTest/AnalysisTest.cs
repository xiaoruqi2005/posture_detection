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
        public static void Man()
        {
            Console.WriteLine("--- 开始进行测试  ---");
            Posenalyzer ana = new Posenalyzer();
             ana.StartAsync().Wait();

        }
    }
}
