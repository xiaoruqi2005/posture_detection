using Camera;
using Common;
using System.Threading.Tasks;
namespace Analysis
{
    public class Posenalyzer: IDisposable
    {
        private  PoseTcpClient poseClient;
        //发出停止信号的 TaskCompletionSource
        private TaskCompletionSource<bool> stopSignal = new TaskCompletionSource<bool>();

        private static readonly string pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.pythonExecutable);
        

        public Posenalyzer()
        {
            poseClient = new PoseTcpClient(pythonScriptPath,Constants.pythonScriptFileName);
            poseClient.PeriodicDataUPdate += PoseClient_PeriodicDataUPdate;
            poseClient.SetUpdateInterval(Constants.InterValMs);
            poseClient.ConnectionStatusChanged += PoseClient_ConnectionStatusChanged;
        }

        public  async Task StartAsync()
        {
            Console.WriteLine("---开始进行姿态分析---");
            Console.WriteLine("---正在启动python 脚本---");
            poseClient.StartPythonScript();
            Console.WriteLine($"等待{Constants.waitTime}ms让python服务器启动……");
            await Task.Delay(Constants.waitTime); //异步操作并且需要等待操作完毕
            Console.WriteLine("---正在尝试来连接到服务器---");
            Task receiveTask = poseClient.ConnectAndReceiveAsync();
            _ = receiveTask.ContinueWith(t =>
            {
                Console.WriteLine($"socket 接受失败{t.Exception.GetBaseException().Message}");
                SignalStop();
            }, TaskContinuationOptions.OnlyOnFaulted);
            await stopSignal.Task; //返回一个 Task 对象，表示分析的结果
        }
        //发出停止分析的信号
         public void SignalStop()
        {
            Console.WriteLine("接收到停止分析的信号 ");
            stopSignal.TrySetResult(true);  //设置TaskCompletionSource的 结果
            poseClient?.Dispose(); //释放资源
        }
        private void PoseClient_ConnectionStatusChanged(object? sender, bool isConnected)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] --- 连接状态改变: {(isConnected ? "已连接" : "已断开")} ---");
            // 如果连接断开，可以考虑在此处触发重连逻辑或停止分析
            if (!isConnected)
            {
                Console.WriteLine("与服务器的连接已断开。");
                // 如果不是主动调用 SignalStop() 导致的断开，可能需要发出停止信号
                // 例如： SignalStop(); // 如果需要断开就停止整个分析流程
            }
        }
        public void Dispose()
        {
            SignalStop();

            poseClient?.Dispose(); //释放资源
            poseClient = null; //将 poseClient 设置为 null，确保不再引用
            stopSignal = null;
            Console.WriteLine("所有资源被释放");
            GC.SuppressFinalize(this);
        }
        private  void PoseClient_PeriodicDataUPdate(object? sender, HolisticData e) //这个函数中书写分析逻辑，每隔 33 ms 会触发一次
        {
            Console.WriteLine("成功进入分析逻辑");
          //  throw new NotImplementedException();
        }
    }
}
