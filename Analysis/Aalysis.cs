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
        //左肩 右肩 左眼 右眼 鼻子 左髋 右髋
        public const int LEFT_SHOULDER_INDEX = 11;  //基于33个点的各个关键点对应的索引
        public const int RIGHT_SHOULDER_INDEX = 12;
        public const int LEFT_EYE_INDEX = 2;
        public const int RIGHT_EYE_INDEX = 5;
        public const int NOSE_INDEX = 0;

        public const int LEFT_HIP = 23;
        public const int RIGHT_HIP = 24;

        public const int Width = 1920;  //宽度和高度的像素值 
        public const int Height = 1200;

        public bool _isStandardPosture = false;

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
            if (!data.HasPoseData) return;//pose数据未获取到
            
            //获取 需要的几个关键点的坐标
            Landmark nose = e.pose[NOSE_INDEX];
            Landmark leftShouder = e.pose[LEFT_SHOULDER_INDEX];
            Landmark rightShoudler = e.pose[RIGHT_SHOULDER_INDEX];
            Landmark leftEye = e.pose[LEFT_EYE_INDEX];
            Landmark rightEye = e.pose[RIGHT_EYE_INDEX];
        }

        #region 检测算法
        //两肩水平检测
        private bool CheckShoulder(Landmark ls, Landmark rs)
        {
           
        }
        //两眼水平
        private bool CheckEye(Landmark le, Landmark re) { 
        
        }
        //头部倾斜（两眼中心与鼻子夹角）
        private bool CheckEyeAndNose(Landmark le, Landmark re,Landmark nose)
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


/*简化散点图
           [0.72688544, 0.56508335],   // 0.43491665 → 1-0.43491665
            [0.7729777, 0.67853543],    // 0.32146457 → 1-0.32146457
            [0.802631, 0.67663142],     // 0.32336858 → 1-0.32336858
            [0.8322169, 0.67561328],
            [0.6759644, 0.68899593],
            [0.6404269, 0.6926907],
            [0.612177, 0.69484726],
            [0.8706611, 0.64611772],
            [0.56374156, 0.64994043],
            [0.7663146, 0.45809066],
            [0.6576844, 0.4726838],
            [0.9806801, 0.22711265],
            [0.36423257, 0.1411013],
            [1.192946, -0.2421926],     // 注意：这里出现负坐标
            [0.16992977, -0.3461021],
            [1.0122304, 0.3209457],
            [0.26204428, -0.8555199],
            [0.9827927, 0.4736041],
            [0.26773474, -1.0074146],
            [0.9343287, 0.4962695],
            [0.32092103, -0.9395112],
            [0.9220913, 0.44563067],
            [0.33349714, -0.8853474],
            [0.9275886, -0.9709373],
            [0.47438377, -0.9700596],
            [0.91220236, -1.9174216],
            [0.5305889, -1.8933508],
            [0.90531284, -2.748955],
            [0.5502607, -2.7395797],
            [0.90718186, -2.8877609],
            [0.544782, -2.8716595],
            [0.8527028, -3.0198174],
            [0.62977326, -3.0169187]
 
 
 */