namespace Common
{
    public class Constant  //定义一些需要的常量类型
    {
        public static string pythonScriptFileName = "pythonScripts\\Holistic_server.py";         // python 脚本路径
        public  static  string pythonInterpreterPath = "D:\\python_project\\pythonProject\\.venv\\Scripts\\python.exe"; // Python 解释器路径 注意对应的python环境下需要有mediapipe包
        public static int InterValMs = 33; // 解析间隔时间
        public static int waitTime = 5000; // 等待时间 ，单位 秒
    }
}
