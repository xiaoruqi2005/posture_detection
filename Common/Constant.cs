using Analysis;

namespace Common
{
    public class Constants  //定义一些需要的常量类型
    {
        public static string pythonScriptFileName = "pythonScripts\\Holistic_server.py";         // python 脚本路径
        public static string pythonScriptPath2 = "pythonScripts\\pose_server.py";
        public  static  string pythonInterpreterPath = "D:\\python_project\\pythonProject\\.venv\\Scripts\\python.exe"; // Python 解释器路径 注意对应的python环境下需要有mediapipe包
        public static string pythonExecutable = "pythonScripts\\Holistic_server\\Holistic_server.exe"; // python 可执行文件路径 
        public static int InterValMs = 33; // 解析间隔时间
        public static int waitTime = 5000; // 等待时间 ，单位 秒

        public static AnalysisResult result;
        public static float maxvertical;
            //水平最大夹角 竖直最大夹角
    }
}
