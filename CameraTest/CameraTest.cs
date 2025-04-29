using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Camera; // 引入 PoseTcpClient 命名空间
namespace CameraTest
{
    class Test
    {
        private static PoseTcpClient poseClient;
        private const string pythonScriptFileName = "pythonScripts\\Holistic_server.py";
       // private const string pythonScriptFileName = "D:\\postureDetect_py\\pythonProject\\pose_server.py";
        private static readonly string pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pythonScriptFileName);
        private const string pythonInterpreterPath = "D:\\python_project\\pythonProject\\.venv\\Scripts\\python.exe";
        private static int WaitTime = 2000; // 等待时间，单位为毫秒

        //private const int AnalysisReteMs = 33;
        private System.Timers.Timer _timer;


        private static readonly object printLock = new object();

        static async Task Main(string[] args)
        {
            poseClient  = new PoseTcpClient(pythonScriptPath, pythonInterpreterPath);
           // poseClient.PoseDataReceived += PoseClient_PeriodicDataUpdate;
            poseClient.SetUpdateInterval(33);
            Console.WriteLine("--- 客户端开始测试 --- ");
           
            poseClient.ConnectionStatusChanged += PoseClient_ConnectionStatusChanged; // 订阅连接状态变化事件;
            poseClient.PeriodicDataUPdate += PoseClient_OnPoseDataReceived;
            Console.WriteLine("正在启动 python 脚本");
            poseClient.StartPythonScript();
            await Task.Delay(WaitTime);
            _ = poseClient.ConnectAndReceiveAsync();

            // --- 保持控制台应用程序运行，直到用户按下任意键 ---
            Console.ReadKey();

            // --- 6. 程序退出时的清理 ---
            Console.WriteLine("正在停止客户端并进行清理...");
            // Dispose 方法会处理停止内部计时器、断开 Socket 连接以及终止 Python 脚本。
            poseClient?.Dispose(); // 使用 ?.Dispose() 确保在 poseClient 为 null 时不会出错

            Console.WriteLine("客户端已停止。按任意键关闭窗口。");
            Console.ReadKey();

        }

        private static  void PoseClient_ConnectionStatusChanged(object? sender, bool e)
        {
            Console.WriteLine(" --- 连接状态改变  ---");
           
        }

        private static  void PoseClient_OnPoseDataReceived(object? sender, HolisticData e)
        {
            Console.WriteLine("--- 数据刷新 --- ");
          
                    Console.WriteLine(e);
                 
        }
    }





    //    Console.WriteLine("C# Pose Client Console Application");
    //    Console.WriteLine("Press 'q' + Enter to quit.");

    //    // --- 检查 Python 脚本文件是否存在 ---
    //    if (!File.Exists(pythonScriptPath))
    //   {
    //        Console.ForegroundColor = ConsoleColor.Red;
    //        Console.WriteLine($"Error: Python script not found at {pythonScriptPath}");
    //        Console.WriteLine("Please ensure 'pose_server.py' is added to the C# project");
    //        Console.WriteLine("and its 'Copy to Output Directory' property is set correctly.");
    //        Console.ResetColor();
    //        Console.WriteLine("Press Enter to exit.");
    //        Console.ReadLine();
    //        return; // Exit if script is not found
    //    }
    //    Console.WriteLine($"Python script path: {pythonScriptPath}");


    //    // 创建 PoseTcpClient 实例
    //    poseClient = new PoseTcpClient(pythonScriptPath, pythonInterpreterPath);

    //    poseClient.PoseDataReceived += PoseClient_PoseDataReceived;  //订阅数据接收事件
    //    poseClient.ConnectionStatusChanged += PoseClient_ConnectionStatusChanged; // 订阅连接状态变化事件
    //    Console.WriteLine("正在启动 python 脚本 ……");
    //    poseClient.StartPythonScript();
    //    Console.WriteLine("Python 脚本已启动，正在初始化服务端  ……");
    //    await Task.Delay(WaitTime); // 等待 2 秒以确保服务端初始化完成

    //    //尝试连接服务器端并开始接收数据
    //    Console.WriteLine("尝试连接服务端……");
    //    _ = Task.Run(() => poseClient.ConnectAndReceiveAsync()); // 忽略警告
    //    bool running = true;
    //    while (running)
    //    {
    //        string input = Console.ReadLine()?.ToLower(); //读取标准用户输入
    //        if (input == "q")
    //        {
    //            running = false;

    //        }
    //    }
    //    //程序退出前进行清理
    //    Console.WriteLine("正在关闭……");
    //    poseClient?.Dispose(); // 释放资源
    //    Console.WriteLine("软件 关闭 ");
    //}
    //private static void PoseClient_PoseDataReceived(object sender, HolisticData data)   //接收数据并打印到控制台
    //{
    //    //if (data?.keypoints != null && data.keypoints.Any())
    //    //{
    //    //    int pointsToShow = Math.Min(data.keypoints.Count, 5); // 显示点数小于 等于 5
    //    //    StringBuilder sb = new StringBuilder("Frame Keypoints ;[");
    //    //    for (int i = 0; i < pointsToShow; i++)
    //    //    {
    //    //        var p = data.keypoints[i];
    //    //        sb.Append($"P{i}:({p.x:F2},{p.y:F2},{p.visibility:F2}) ");
    //    //    }
    //    //    if (data.keypoints.Count > pointsToShow)
    //    //    {
    //    //        sb.Append("……");
    //    //    }
    //    //    sb.Append("]");
    //    //    Console.WriteLine(sb.ToString());
    //    //}  封装接受的数据
    //}
}
