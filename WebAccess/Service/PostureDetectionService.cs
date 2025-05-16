using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using WebAccess.DTO;
using WebAccess.Hubs;
using Analysis;
using System.Collections.Concurrent;

namespace WebAccess.Service
{
    public class PostureDetectionService : IDisposable
    {
        private readonly IHubContext<PostureHub> _hubContext;
        private Timer? _detectionTimer;
        private bool _isDetecting = false;
        private readonly object _lock = new object();

        /*static {
         Posenalyzer ana = new Posenalyzer();
       // new Thread(() => { ana.StartAsync(); }).Start();
        }*/
        static void  MyStart()
        {
            Posenalyzer ana = new Posenalyzer();
            _= ana.StartAsync();
        }

        // 简单的内存历史数据存储 (课设适用)
        private static readonly ConcurrentQueue<PostureData> _history = new ConcurrentQueue<PostureData>();
        private const int MaxHistorySize = 20; // 最多保存20条

        // 注入你自己的 Analysis 和 Camera.Client 服务
        // private readonly YourNamespace.Analysis.Analysis _analyzer;
        // private readonly YourNamespace.Camera.Client _cameraClient;
     
        public PostureDetectionService(IHubContext<PostureHub> hubContext
                                       /*, YourNamespace.Analysis.Analysis analyzer,
                                       YourNamespace.Camera.Client cameraClient */)
        {
            _hubContext = hubContext;
            // _analyzer = analyzer;
            // _cameraClient = cameraClient;
            MyStart();
            Console.WriteLine("PostureDetectionService Initialized.");
        }

        public void StartDetection()
        {
            lock (_lock)
            {
                if (_isDetecting) return;
                _isDetecting = true;
            }
            Console.WriteLine("Starting posture detection simulation...");

            _detectionTimer = new Timer(async (_) =>
            {
                if (!_isDetecting) return;

                // --- 模拟获取和分析数据 ---
                // 替换为你实际的逻辑:
                // 1. landmarks = await _cameraClient.GetLandmarksAsync();
                // 2. analysisResult = _analyzer.PerformAnalysis(landmarks);
                // 3. postureData = MapToPostureData(analysisResult);
                
                PostureData postureData =new PostureData();
                postureData.Timestamp = Posenalyzer.result.Timestamp;
                postureData.ShoulderTiltAngle = Posenalyzer.result.ShoulderTiltAngle;
                postureData.ShoulderState = Posenalyzer.result.ShoulderState;
                postureData.HeadTiltAngle = Posenalyzer.result.HeadTiltAngle;
                postureData.HeadTiltState = Posenalyzer.result.HeadTiltState;
                postureData.HunchbackState = Posenalyzer.result.HunchbackState;
                // --- 模拟结束 ---

                Console.WriteLine(postureData.ShoulderState);
                // 保存到历史记录 (简单实现)
                _history.Enqueue(postureData);
                while (_history.Count > MaxHistorySize)
                {
                    //_history.TryDequeue(out _);

                }

                // 通过 SignalR 推送数据
                await _hubContext.Clients.All.SendAsync("ReceivePostureData", postureData);

            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000)); // 每秒推送一次
        }

        public void StopDetection()
        {
            lock (_lock)
            {
                if (!_isDetecting) return;
                _isDetecting = false;
            }
            _detectionTimer?.Change(Timeout.Infinite, 0);
            Console.WriteLine("Posture detection simulation stopped.");
        }

        public IEnumerable<PostureData> GetPostureHistory(int limit)
        {
            // 返回历史记录的副本，按时间倒序
            return _history.OrderByDescending(p => p.Timestamp).Take(limit).ToList();
        }

        public async Task<string> GetLLMSuggestionAsync(string prompt)
        {
            // 模拟调用大模型 API
            await Task.Delay(1500); // 模拟网络延迟和处理时间
            if (prompt.ToLower().Contains("驼背"))
            {
                return "关于驼背，建议您保持正确的坐姿，抬头挺胸，收紧腹部。每隔一段时间站起来活动一下，并进行一些针对性的背部伸展运动，如猫式伸展、靠墙站立等。长期坚持会有改善效果。";
            }
            else if (prompt.ToLower().Contains("坐姿"))
            {
                return "正确的坐姿非常重要。请确保您的双脚平放在地面上，膝盖与臀部同高或略低。背部应挺直并靠在椅背上，双肩放松。显示器应位于视线正前方或略低于视线水平。";
            }
            return $"收到您的问题：'{prompt}'。这是一个通用的健康建议，请注意多喝水，保持适度运动。";
        }

        public void Dispose()
        {
            _detectionTimer?.Dispose();
        }
    }
}
