using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using WebAccess.DTO;
using WebAccess.Hubs;
using Analysis;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebAccess.Service
{
    public class PostureDetectionService : IDisposable
    {
        private readonly IHubContext<PostureHub> _hubContext;
        private Timer? _detectionTimer;
        private bool _isDetecting = false;
        private readonly object _lock = new object();
        private  readonly ILogger<PostureDetectionService> _logger;
        private static readonly object _posenalyzerLock = new object();
        static void  MyStart()
        {
            Posenalyzer ana = new Posenalyzer();
            _= ana.StartAsync();
        }

        // 简单的内存历史数据存储 (课设适用)
        private static readonly ConcurrentQueue<PostureData> _history = new ConcurrentQueue<PostureData>();
        private const int MaxHistorySize = 2000; // 最多保存20条

     
        public PostureDetectionService(IHubContext<PostureHub> hubContext, ILogger<PostureDetectionService> logger
                                       /*, YourNamespace.Analysis.Analysis analyzer,
                                       YourNamespace.Camera.Client cameraClient */)
        {
            _hubContext = hubContext;
            _logger = logger;
           MyStart();
            _logger.LogInformation("PostureDetectionService Initialized.");
        }

        public void StartDetection()
        {
            lock (_lock)
            {
                if (_isDetecting) return;
                _isDetecting = true;
            }
            Console.WriteLine("Starting posture detection simulation...");
            _detectionTimer?.Dispose(); 
            _detectionTimer = new Timer(async (state) =>
            {
                var service = (PostureDetectionService)state;
                if (service!=null && !service._isDetecting)
                {
                    service._logger.LogTrace("Detection stopped, timer tick skipped.");
                    return;
                }
                if (!_isDetecting) return;
                try
                {
                    // --- 模拟获取和分析数据 ---
                    // 替换为你实际的逻辑:
                    // 1. landmarks = await _cameraClient.GetLandmarksAsync();
                    // 2. analysisResult = _analyzer.PerformAnalysis(landmarks);
                    // 3. postureData = MapToPostureData(analysisResult);

                    PostureData postureData = new PostureData();
                    postureData.Timestamp = Posenalyzer.result.Timestamp;
                    postureData.ShoulderTiltAngle = Posenalyzer.result.ShoulderTiltAngle;
                    postureData.ShoulderState = Posenalyzer.result.ShoulderState;
                    postureData.HeadTiltAngle = Posenalyzer.result.HeadTiltAngle;
                    postureData.HeadTiltState = Posenalyzer.result.HeadTiltState;
                    postureData.HunchbackState = Posenalyzer.result.HunchbackState;
                    postureData.HeadPitchDirection = Posenalyzer.result.HeadPitchDirection;
                    postureData.OverallPostureStatus = Posenalyzer.result.OverallPostureStatus;
                    postureData.HeadYawDirection = Posenalyzer.result.HeadYawDirection;
                    //postureData.DetectedIssues = Posenalyzer.result.DetectedIssues;
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
                    service._logger.LogTrace("[Timer Tick] Data sent via SignalR.");
                }
                catch (Exception ex)
                {
                    // 非常重要：捕获并记录 Timer 回调中的所有异常
                    service._logger.LogError(ex, "[Timer Tick Error] Exception in posture detection timer callback.");
                    // 根据错误类型，你可能想在这里停止检测
                    // service.StopDetection(); // 例如，如果错误是不可恢复的
                }

            }, this, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000)); // 每秒推送一次
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
        public void Dispose()
        {
            _detectionTimer?.Dispose();
        }
      //  private readonly ILogger<PostureDetectionService> _logger; // 将 YourClassName 替换为你的实际类名
        public async Task<string> GetLLMSuggestionAsync(string prompt)
        {

            // 获取历史数据并发送给大模型
            // 这里还需要通过数据库获取历史数据并发送给大模型


            var queryObject = new
            {
                model = "qwen-plus",
                messages = new[]
    {
            new { role = "system", content ="\"你是一位数据驱动的体态分析师。我将为你提供包含时间戳、头部倾斜（角度和状态）、双肩倾斜（角度和状态）、驼背状态等关键指标的历史体态数据记录。\r\n你的职责是：\r\n*   **数据解读**：精确解读这些数据，识别出异常值、持续性偏差以及潜在的模式。\r\n*   **量化评估**：尽可能基于数据中的数值进行评估，例如，指出平均倾斜度、偏差频率等。\r\n*   **问题诊断**：明确指出数据反映的核心体态问题。\r\n*   **定制化方案**：基于诊断结果，提出有针对性的、可执行的调整方案，包括但不限于特定拉伸动作、力量训练、坐姿/站姿提醒等。\r\n你的分析和建议必须严格依据所给数据。\"" }, // 在这里直接使用 request.Prompt
            new { role = "user", content =  prompt }
        }
            };

            // 2. 将 C# 对象序列化为 JSON 字符串
            string jsonContent = JsonSerializer.Serialize(queryObject, new JsonSerializerOptions
            {
                WriteIndented = true // 可选：格式化输出，方便调试
            });


            string suggestion = await SendPostRequestAsync(jsonContent);

            string? assistantResponseContent = null;

            try
            {
                // 使用 System.Text.Json 反序列化
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // 如果JSON属性名和C#属性名大小写不完全匹配
                };
                LLMFullResponse? llmResponse = JsonSerializer.Deserialize<LLMFullResponse>(suggestion, options);

                if (llmResponse != null && llmResponse.Choices != null && llmResponse.Choices.Any())
                {
                    // 获取第一个 choice (通常只有一个)
                    Choice? firstChoice = llmResponse.Choices.FirstOrDefault();
                    if (firstChoice != null && firstChoice.Message != null)
                    {
                        // 确保角色是 "assistant" (可选，但好习惯)
                        if (firstChoice.Message.Role == "assistant")
                        {
                            assistantResponseContent = firstChoice.Message.Content;
                        }
                        else
                        {
                            // Log or handle cases where the role is not "assistant"
                            _logger.LogWarning($"Expected assistant role but got '{firstChoice.Message.Role}'.");
                            // You might still want to get the content if the structure is always this way:
                            // assistantResponseContent = firstChoice.Message.Content;
                        }
                    }
                }

                if (string.IsNullOrEmpty(assistantResponseContent))
                {
                    // Log or handle cases where the content could not be extracted
                    _logger.LogError("Could not extract assistant's content from LLM response.");
                    // Fallback or error message
                    assistantResponseContent = "Error: Could not parse LLM response content.";
                }
            }
            catch (JsonException ex)
            {
                // Log JSON parsing error
                _logger.LogError(ex, "Error deserializing LLM response JSON.");
                assistantResponseContent = "Error: Invalid LLM response format.";
            }
            catch (Exception ex)
            {
                // Log any other unexpected errors
                _logger.LogError(ex, "An unexpected error occurred while processing LLM response.");
                assistantResponseContent = "Error: Could not process LLM response.";
            }

            return assistantResponseContent;
        }

        private static readonly HttpClient httpClient = new HttpClient();
        public  async Task<string> SendPostRequestAsync(string jsonContent, string url = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions", string apiKey = "sk-078dcf764c624823a76b07e2131a13c0")
        {
            using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
            {
                // 设置请求头
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // 发送请求并获取响应
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                // 处理响应
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return $"请求失败: {response.StatusCode}";
                }
            }

        }

    }
}
