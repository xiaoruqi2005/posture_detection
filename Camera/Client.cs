using System;
using System.Collections.Generic; // For List
using System.Threading.Tasks; // For Task, async/await
using System.Threading; // For CancellationTokenSource
using System.Text;
using System.Net.Sockets;
using System.Linq; // Optional: For Linq extensions like .Any()
using System.Diagnostics; // Required for Process
using System.IO;
using System.Text.Json;
using System.Reflection.Metadata; // Required for Path.Combine and File.Exists
using Common;
namespace Camera
{
    public class PoseData      //最初的简化版的PoseData
    {
        public List<Landmark> keypoints { get; set; } // 使用 List<Landmark> 对应 JSON 数组
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Landmark l in keypoints)
            {
                sb.Append(l.ToString());
            }
            return sb.ToString();
        }
    }
    // --- Socket 客户端类 ---
    public class PoseTcpClient : IDisposable
    {

        private static bool EnableInvoke = true; // 是否启用事件触发，默认启用
        private static int PrintCount = 10; // 打印次数
        private static readonly  Object PrintLock = new Object();

        private TcpClient client;
        private NetworkStream stream;
        private const string ServerIp = "127.0.0.1"; // 需要与 Python 服务器 IP 匹配
        private const int ServerPort = 65432;       // 需要与 Python 服务器端口匹配
        private const int MESSAGE_LENGTH_BYTES = 4; // 需要与 Python 端匹配

        private CancellationTokenSource cts;

        // 事件：当接收到新的姿态数据时触发
       // public event EventHandler<HolisticData> PoseDataReceived;

        // 事件：当连接状态改变时触发
        public event EventHandler<bool> ConnectionStatusChanged;
      //  public event EventHandler<HolisticData> ConnectionStatusChanged; //连接状态改变事件
        private bool isConnected = false;    //连接状态
        public bool IsConnected
        {
            get { return isConnected; }
            private set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                     ConnectionStatusChanged?.Invoke(this, isConnected);
                    //ConnectionStatusChanged?.Invoke(this, _latestHolisticData); // 触发连接状态改变事件
                }
            }
        }

        //添加python进程管理的相关成员， 方便用c#运行python脚本
        private Process pythonProcess;
        private bool isPythonRunning;
        private string pythongScriptPath; //脚本路径
        private string pythonInterpreterPath; //python解释器路径 注意对应的环境要导入mediapipe
        private string pythonExexutable; //  //python可执行文件路径

        //添加姿态属性
        private volatile HolisticData _latestHolisticData;  // volatile 关键字用于确保数据在多线程环境下的可见性   每次数据接收时就会改变 
        private System.Timers.Timer _periodicUpdateTimer;
        private int _updateIntervalMs = 38;  //帧刷新间隔为 38 ms

        public event EventHandler<HolisticData> PeriodicDataUPdate; // 当周期性事件触发时，提供最新的关键点数据
        public HolisticData LatestHolisticData => _latestHolisticData;


        private int waitTime = 10000;  //默认等待时间为 5 秒
        public PoseTcpClient(string scriptPath, string interpreterPath)//string scriptPath, string interpreterPath )
        {
            this.cts = new CancellationTokenSource();
            this.pythongScriptPath = scriptPath;
            this.pythonInterpreterPath = interpreterPath;
            //this.pythonExexutable = pythonExecutablePath;
            //初始化定时器
            _periodicUpdateTimer = new System.Timers.Timer(_updateIntervalMs);
            _periodicUpdateTimer.Elapsed += OnPeriodicTimerElapsed;  //绑定计时器触发事件
            _periodicUpdateTimer.AutoReset = true; // 设置为自动重置
        }

        public void SetUpdateInterval(int intervalMs)
        {
            if(intervalMs <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalMs), "更新间隔不能为负数");
            }
            _updateIntervalMs = intervalMs;
            if(_periodicUpdateTimer != null && _periodicUpdateTimer.Enabled)
            {
                _periodicUpdateTimer.Interval = _updateIntervalMs; // 更新定时器间隔
            }
        }

        //启动 python 脚本
        public void StartPythonScript()
        {
            if (isPythonRunning && pythonProcess != null && !pythonProcess.HasExited)
            {
                Console.WriteLine("Python 脚本已经在运行");
                return;
            }
            try
            {
                pythonProcess = new Process//创建python运行进程
                {
                    StartInfo = new ProcessStartInfo  //简便写法， 为对象进行赋值
                    {
                        FileName = pythonInterpreterPath, //python解释器路径
                        Arguments = pythongScriptPath, //python脚本路径  使用exe可执行文件时不需要解释器路径
                        UseShellExecute = false, //不使用shell执行
                        RedirectStandardOutput = true, //重定向标准输出
                        RedirectStandardError = true,
                        CreateNoWindow = true   //隐藏窗口
                    },
                    EnableRaisingEvents = true //启用事件
                };

                //python 标准输出处理
                pythonProcess.OutputDataReceived += (sender, e) => //添加事件
                {
                    if ((!string.IsNullOrEmpty(e.Data)))
                    {
                        Console.WriteLine($"python 的标准输出为：{e.Data}");
                    }
                };

                //python 报错信息输出
                pythonProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"python 输出信息：{e.Data}");
                    }
                };

                //python 脚本退出
                pythonProcess.Exited += (sender, e) =>
                {
                    Console.WriteLine($"python process exited with code {pythonProcess.ExitCode}");
                    isPythonRunning = false;
                    Disconnect();
                };
                pythonProcess.Start();
                pythonProcess.BeginOutputReadLine();  //开始异步读取标准输出
                pythonProcess.BeginErrorReadLine();
                isPythonRunning = true;
                Console.WriteLine($"启动python进程{pythongScriptPath}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"python脚本启动异常: {ex.Message}");
            }

        }

        //停止 python 脚本
        public void  StopPythonScript()
        {
            if (isPythonRunning && pythonProcess != null && !pythonProcess.HasExited)
            {
                try
                {
                    Console.WriteLine("正在尝试关闭 python 进程……");
                    if (pythonProcess.CloseMainWindow())  //向主窗口发送关闭python进程的消息
                    {
                        if (pythonProcess.WaitForExit(waitTime))  //5秒内如果退出则视为顺利退出
                        {
                            Console.WriteLine("python 进程顺利退出");
                            return;
                        }
                        Console.WriteLine("python 进程退出超时");
                    }
                    else
                    {
                        Console.WriteLine("python 退出超时，关闭python进程");
                    }
                    pythonProcess.Kill(true); // true 表示也终止子进程

                    // 在强制终止后，等待进程退出，给操作系统时间进行清理
                    // 可以用一个较短的等待时间
                    if (pythonProcess.WaitForExit(5000)) // 例如，等待最多 5 秒进行清理
                    {
                        Console.WriteLine("Python 进程已强制终止并确认退出。");
                    }
                    else
                    {
                        Console.WriteLine("Python 进程强制终止，但等待退出超时（可能有残留资源需要清理）。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"进程退出存在异常： {ex.Message}");
                }
                finally
                {
                    pythonProcess.Dispose();
                    pythonProcess = null;
                    isPythonRunning = false;
                }
                }
            else if (pythonProcess != null)   //进程存在但是没有运行
            {
                pythonProcess.Dispose ();
                pythonProcess = null; 
            }
            isPythonRunning = false;
            }
        
        //尝试异步连接并接收数据
        public async Task ConnectAndReceiveAsync()
        {
            if (IsConnected)
            {
                Console.WriteLine("已经连接");
                return;
            }
            if(!isPythonRunning)
            {
                Console.WriteLine("python脚本未运行 无法连接");
                return;
            }
            client = new TcpClient();
            cts = new CancellationTokenSource();

            //连接逻辑
            try
            {
                Console.WriteLine($"正在尝试建立连接{ServerIp}:{ServerPort}……");//父类的成员
                //使用异步连接
                await client.ConnectAsync(ServerIp, ServerPort,cts.Token).ConfigureAwait(false);
                Console.WriteLine("成功连接到服务器");
                IsConnected = true; //更新连接状态
                stream = client.GetStream(); //获取网络流
                byte[] lengthBuffer = new byte[MESSAGE_LENGTH_BYTES]; //缓冲区用于读取长度前缀

                //连接后， 启动周期性更新定时器
                _periodicUpdateTimer.Interval = _updateIntervalMs;
                _periodicUpdateTimer.Start(); // 启动定时器
                Console.WriteLine($"成功启动定时器，计时间隔为{_periodicUpdateTimer.Interval}");



                //接受循环
                while (!cts.IsCancellationRequested)  //只要不主动退出就一直读取
                {
                    int bytesRead;
                    int totalBytesRead = 0;
                    int bytesToRead = MESSAGE_LENGTH_BYTES;
                    // --- 1. 读取消息长度前缀 ---
                    // 需要确保读取到完整的长度前缀 (MESSAGE_LENGTH_BYTES 字节)
                    while (totalBytesRead < MESSAGE_LENGTH_BYTES)
                    {
                        try
                        {
                            // 使用异步读取
                            bytesRead = await stream.ReadAsync(lengthBuffer, totalBytesRead, bytesToRead - totalBytesRead, cts.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("接收操作被取消");
                            return;
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine($"Socket 错误：{ex.Message}");
                            return; // Exit receive loop on socket error
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"错误：{ex.Message}");
                            return; // Exit receive loop on other errors
                        }
                        if (bytesRead == 0)
                        {
                            Console.WriteLine("服务器断开连接");
                            return; // Exit receive loop
                        }
                        totalBytesRead += bytesRead;
                    }
                    // 解包长度前缀 (大端序无符号整数)
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(lengthBuffer); // Reverse bytes if system is little-endian
                    }
                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    // Basic check to prevent reading excessively large or negative lengths
                    if (messageLength <= 0 || messageLength > 1024 * 1024 * 10) // Example max size (1MB)
                    {
                        Console.WriteLine($"收到无效消息长度: {messageLength}. 断开连接.");
                        return; // Invalid length, disconnect
                    }
                    // --- 2. 读取实际消息数据 ---
                    byte[] messageBuffer = new byte[messageLength];
                    totalBytesRead = 0;
                    bytesToRead = messageLength;
                    // 需要确保读取到完整的消息数据 (messageLength 字节)
                    while (totalBytesRead < messageLength)
                    {
                        try
                        {
                            bytesRead = await stream.ReadAsync(messageBuffer, totalBytesRead, bytesToRead - totalBytesRead, cts.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("接收操作被取消");
                            return;
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine($"Socket 错误：{ex.Message}");
                            return; // Exit receive loop on socket error
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"错误：{ex.Message}");
                            return; // Exit receive loop on other errors
                        }
                        if (bytesRead == 0)
                        {
                            Console.WriteLine("服务器断开连接");
                            return; // Exit receive loop
                        }
                        totalBytesRead += bytesRead;
                    }

                    //将字节转换为字符串
                    string jsonString = Encoding.UTF8.GetString(messageBuffer, 0, messageLength);
                    try
                    {
                        HolisticData holisticData = JsonSerializer.Deserialize<HolisticData>(jsonString);
                        _latestHolisticData = holisticData; // 更新最新数据
                       // PoseDataReceived?.Invoke(this, holisticData); // 触发事件，将数据传递给订阅者
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"json 数据解析错误： {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"发生意外错误： {ex.Message}");
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket 连接错误： {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"其他意外错误 :{ex.Message}");
            }
            finally
            {
                Disconnect();
            }

        }

        public void Disconnect()
        {
            _periodicUpdateTimer?.Stop();
            Console.WriteLine("数据更新周期停止 ");
            cts?.Cancel(); // 发送取消信号给异步读取任务
            if (stream != null)
            {
                try { stream.Close(); } catch { } // 关闭数据流
                try { stream.Dispose(); } catch { }
                stream = null;
            }
            if (client != null)
            {
                try { client.Close(); } catch { } // 关闭客户端
                try { client.Dispose(); } catch { }
                client = null;
            }
            IsConnected = false;
            Console.WriteLine("Socket 客户端断开连接.");
        }

        //达到定时器的预设时间时会触发这个事件
        private void OnPeriodicTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)   
        {
            // 在定时器触发时，读取存储的最新数据
            HolisticData dataToProvide = _latestHolisticData;
            if (dataToProvide != null && dataToProvide.HasAnyLandmarks())
            {
                //数据不为空，可以向外传递
               // if (EnableInvoke)
                {
                    PeriodicDataUPdate?.Invoke(this, dataToProvide); // 触发事件，将数据传递给订阅者
                  //  Console.WriteLine($"周期性更新数据: {dataToProvide}");
                    PrintCount--;
                    if(PrintCount <=0)EnableInvoke = false;
                }
            }
        }

        public void Dispose()    //用于释放资源
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                cts?.Dispose();
                cts = null;
            }
            Disconnect();
            StopPythonScript(); //释放python进程
        }
    }
}

