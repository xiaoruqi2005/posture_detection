using Microsoft.AspNetCore.Mvc;
using WebAccess.DTO;
using WebAccess.Service;
using Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebAccess.Respoository;
using ZstdSharp.Unsafe;

namespace WebTest.Controllers
{
    [Route("api/[controller]")] // /api/posture
    [ApiController]
    public class PostureController : ControllerBase
    {
        private readonly PostureDetectionService _postureService;
        private readonly PostureAnalysisRepository _repository;

        public PostureController(PostureDetectionService postureService, PostureAnalysisRepository repository)
        {
            _repository = repository;
            InitializeMockData();
            _postureService = postureService;
        }

        private static List<PostureData> _mockPostureHistory = new List<PostureData>();
        private static bool _dataInitialized = false;
        private static readonly Random _random = new Random();


        private  async Task InitializeMockData() // 修改为异步方法 从而加快效率并且解决报错问题
        {
                _mockPostureHistory = await _repository.GetAllValidAnalysisdataAsync();
                _mockPostureHistory = _mockPostureHistory.OrderBy(p => p.Timestamp).ToList(); // 按时间排序
        }
        private  void InitializeMockData_test()
        {
            if (_dataInitialized) return;

            lock (_mockPostureHistory) // 简单线程安全
            {
                if (_dataInitialized) return;

                // 生成过去一周的数据，每天大约10-30条记录
                //测试用 后续需要改为从数据库查询数据
                DateTime endDate = DateTime.UtcNow;
                for (int dayOffset = 0; dayOffset < 7; dayOffset++) // 过去7天
                {
                    DateTime currentDateBase = endDate.AddDays(-dayOffset).Date; // 取当天的开始
                    int recordsForDay = _random.Next(30, 80); // 每天30-80条

                    for (int i = 0; i < recordsForDay; i++)
                    {
                        // 在一天内随机分布时间
                        TimeSpan randomTimeSpan = TimeSpan.FromHours(_random.NextDouble() * 24);
                        DateTime recordTimestamp = currentDateBase.Add(randomTimeSpan);
                        if (recordTimestamp > endDate) recordTimestamp = endDate; // 不超过当前时间

                        _mockPostureHistory.Add(new PostureData
                        {
                            // Id = _mockPostureHistory.Count + 1,
                            Timestamp = recordTimestamp,
                            HeadTiltAngle = (float)(_random.NextDouble() * 20 - 10), // -10 to +10
                            HeadTiltState = (Constants.HeadTiltSeverity)_random.Next(Enum.GetNames(typeof(Constants.HeadTiltSeverity)).Length),
                            ShoulderTiltAngle = (float)(_random.NextDouble() * 10 - 5), // -5 to +5
                            ShoulderState = (Constants.TiltSeverity)_random.Next(Enum.GetNames(typeof(Constants.TiltSeverity)).Length),
                            HunchbackState = (Constants.HunchbackSeverity)_random.Next(Enum.GetNames(typeof(Constants.HunchbackSeverity)).Length)
                        });
                    }
                }
                _mockPostureHistory = _mockPostureHistory.OrderBy(p => p.Timestamp).ToList(); // 按时间排序
                _dataInitialized = true;
            }
        }
        // --- 模拟数据存储结束 ---


        // GET: api/posture/history
        [HttpGet("history")]
        public ActionResult<IEnumerable<PostureData>> GetHistory([FromQuery] int limit = 2000)
        {
            // 实际应用中，这里会有分页、按时间范围查询等逻辑
            // 为了简化，我们只返回最近的 `limit` 条记录
            var recentHistory = _mockPostureHistory
                                    .OrderByDescending(p => p.Timestamp) // 获取最新的
                                    .Take(limit)
                                    .OrderBy(p => p.Timestamp) // 再按时间升序返回给前端
                                    .ToList();
            return Ok(recentHistory);
        }


        // GET: api/posture/stats/hunchback?range={today|thisWeek|allTime}
        [HttpGet("stats/hunchback")]
        public ActionResult<IEnumerable<PostureStateStatistic>> GetHunchbackStatistics([FromQuery] string range = "today")
        {
            IEnumerable<PostureData> dataToProcess;
            DateTime now = DateTime.UtcNow;

            switch (range.ToLower())
            {
                case "today":
                    DateTime todayStart = now.Date;
                    DateTime todayEnd = todayStart.AddDays(1).AddTicks(-1);
                    dataToProcess = _mockPostureHistory.Where(p => p.Timestamp >= todayStart && p.Timestamp <= todayEnd);
                    break;
                case "thisweek":
                    // 简单的本周定义：从本周一到本周日
                    // DayOfWeek: Sunday = 0, Monday = 1, ..., Saturday = 6
                    int diffToMonday = (now.DayOfWeek == DayOfWeek.Sunday ? -6 : (int)DayOfWeek.Monday - (int)now.DayOfWeek);
                    DateTime weekStart = now.AddDays(diffToMonday).Date;
                    DateTime weekEnd = weekStart.AddDays(7).AddTicks(-1);
                    dataToProcess = _mockPostureHistory.Where(p => p.Timestamp >= weekStart && p.Timestamp <= weekEnd);
                    break;
                case "alltime":
                default:
                    dataToProcess = _mockPostureHistory;
                    break;
            }

            if (!dataToProcess.Any())
            {
                return Ok(new List<PostureStateStatistic>()); // 如果没有数据，返回空列表
            }

            var statistics = dataToProcess
                .GroupBy(p => p.HunchbackState) // 按驼背状态分组
                .Select(g => new PostureStateStatistic
                {
                    Name = g.Key.ToString(), // 枚举成员的名称 (例如 "NoHunchback")
                    Value = g.Count()        // 该状态的数量
                })
                .OrderBy(s => s.Name) // 可以按名称排序，可选
                .ToList();

            return Ok(statistics);
        }


        [HttpPost("start")] // POST /api/posture/start
        public IActionResult Start()
        {
            _postureService.StartDetection();
            return Ok(new { message = "Posture detection started." });
        }

        [HttpPost("stop")] // POST /api/posture/stop
        public IActionResult Stop()
        {
            _postureService.StopDetection();
            return Ok(new { message = "Posture detection stopped." });
        }

        [HttpPost("llm/analyze")] // POST /api/posture/llm
        public async Task<IActionResult> QueryLLM([FromBody] LLMQueryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { message = "Prompt cannot be empty." });
            }
            String message = await _postureService.GetLLMSuggestionAsync(request.Prompt);
            return Ok(new { response = message });
         
         
        }

    }
}
