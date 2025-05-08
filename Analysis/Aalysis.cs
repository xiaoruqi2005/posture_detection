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

        public const int LEFT_SHOULDER_INDEX = 11;  //基于33个点的各个关键点对应的索引
        public const int RIGHT_SHOULDER_INDEX = 12;
        public const int LEFT_EYE_INDEX = 2;
        public const int RIGHT_EYE_INDEX = 5;
        public const int NOSE_INDEX = 0;
        public const int Width = 1920;  //宽度和高度的像素值 
        public const int Height = 1200;

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
        private  void PoseClient_PeriodicDataUPdate(object? sender, HolisticData data) //这个函数中书写分析逻辑，每隔 33 ms 会触发一次
        {
            Console.WriteLine("成功进入分析逻辑");
            //  throw new NotImplementedException();

     
            //体态分析逻辑-----------------------
            if (!e.HasPoseData) return;//pose数据未获取到
            
            //获取 需要的几个关键点的坐标
            Landmark nose = e.pose[NOSE_INDEX];
            Landmark leftShouder = e.pose[LEFT_SHOULDER_INDEX];
            Landmark rightShoudler = e.pose[RIGHT_SHOULDER_INDEX];
            Landmark leftEye = e.pose[LEFT_EYE_INDEX];
            Landmark rightEye = e.pose[RIGHT_EYE_INDEX];


        }

        #region 检测算法
        //两肩水平检测
        private bool CheckShoulder(Landmark leftShoulder, Landmark rightShoulder)
        {
           
        }

        //颈部前倾检测
        private bool CheckNeckForward(Landmark nose, Landmark ls, Landmark rs)
        {
            // 计算头部水平偏移量（使用z坐标的相对值）
            float headForward = nose.z ?? 0;

            // 计算肩膀基准位置
            float shoulderMidZ = (ls.z + rs.z) / 2 ?? 0;

            // z值越大表示越靠近摄像头（前倾）
            return (headForward - shoulderMidZ) > NECK_FORWARD_THRESHOLD;
        }
        //面部情绪识别




        #endregion

        #region 辅助算法
        //带可见性检查的关键点获取
        private Landmark GetValidLandmark(List<Landmark> landmarks, int index, float minVisibility = 0.5f)//设定最小可见性为0.5
        {
            if (landmarks == null || index >= landmarks.Count) return null;
            var lm = landmarks[index];
            return (lm.visibility ?? 0) >= minVisibility ? lm : null;//返回null或点本身
        }
        //姿势持续时间检查
        private void CheckPostureDuration(ref DateTime? startTime, bool isBadPosture, string postureName) {
            if (isBadPosture) { 
                startTime??=DateTime.Now;
            }
        
        }


        #endregion
    }
}
