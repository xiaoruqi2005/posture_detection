using Camera;
using Common;
using System.Threading.Tasks;
using static Analysis.AnalysisResult;
using static Common.Constants;


namespace Analysis
{
    public class Vector3
    {   //基础坐标分量
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        // 构造函数
        public Vector3(float x = 0, float y = 0, float z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class Posenalyzer : IDisposable
    {
        private PoseTcpClient poseClient;
        //发出停止信号的 TaskCompletionSource
        private TaskCompletionSource<bool> stopSignal = new TaskCompletionSource<bool>();

        private static readonly string pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.pythonScriptFileName);
        //左肩 右肩 左眼 右眼 鼻子 
        public const int LEFT_SHOULDER_INDEX = 11;  //基于33个点的各个关键点对应的索引
        public const int RIGHT_SHOULDER_INDEX = 12;
        public const int LEFT_EYE_INDEX = 2;
        public const int RIGHT_EYE_INDEX = 5;
        public const int NOSE_INDEX = 0;


        public const int NOSE_TIP = 1;
        public const int CHIN_CENTER = 152;
        public const int LEFT_EYE_OUTER_CORNER = 33;
        public const int RIGHT_EYE_OUTER_CORNER = 263;
        public const int LEFT_MOUTH_CORNER = 61;
        public const int RIGHT_MOUTH_CORNER = 291;
        public const int LEFT_EAR_TRAGION = 234; // (示例，耳屏附近点)
        public const int RIGHT_EAR_TRAGION = 454; // (示例)
        public const int BETWEEN_EYEBROWS = 10;


        //public const int LEFT_HIP = 23;
        //public const int RIGHT_HIP = 24;

        public static readonly AnalysisResult result = new AnalysisResult();

        public bool _isStandardPosture = false;//比较综合的一个评价

        public Posenalyzer()
        {
            poseClient = new PoseTcpClient(pythonScriptPath, Constants.pythonInterpreterPath);
            poseClient.PeriodicDataUPdate += PoseClient_PeriodicDataUPdate;
            poseClient.SetUpdateInterval(Constants.InterValMs);
            poseClient.ConnectionStatusChanged += PoseClient_ConnectionStatusChanged;
            poseClient.ImageFrameReceived += PoseClient_ImageFrameReceived;
            //   poseClient.ConnectionStatusChanged += PoseClient_PeriodicDataUPdate;
        }

        private void PoseClient_ImageFrameReceived(object? sender, System.Drawing.Bitmap e)
        {
            result.FrameData = e;
        }

        public async Task StartAsync()
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
            // await stopSignal.Task; //返回一个 Task 对象，表示分析的结果
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
        private void PoseClient_PeriodicDataUPdate(object? sender, HolisticData data) //这个函数中书写分析逻辑，每隔 500 ms 会触发一次
        {
            Console.WriteLine("成功进入分析逻辑");
            //  throw new NotImplementedException();

            Console.WriteLine("开始进行体态分析");
            if (data == null)
            {
                Console.WriteLine("data 为空，无法分析");
                return;
            }

            Landmark nose = data.pose[NOSE_INDEX];
            Landmark leftShouder = data.pose[LEFT_SHOULDER_INDEX];
            Landmark rightShoudler = data.pose[RIGHT_SHOULDER_INDEX];
            Landmark leftEye = data.pose[LEFT_EYE_INDEX];
            Landmark rightEye = data.pose[RIGHT_EYE_INDEX];

            CheckTimeStamp(); //记录当前帧时间戳
            CheckShoulder(leftShouder, rightShoudler);
            CheckHead(leftEye, rightEye, nose);
            CheckEye(leftEye, rightEye);
            CheckHunchback(leftShouder, rightShoudler, nose);
            AnalyzeHeadDirection_Combined(data.face, Constants.Width, Constants.Height);
            ComprehensiveAnalyse();
            //测试效果
            //    Console.WriteLine("运行到这里了1");
            Console.WriteLine(result);
            // Console.WriteLine("运行到这里了2");

        }

        #region 检测算法
        private void CheckTimeStamp()
        {
            result.Timestamp = DateTime.Now;
        }

        //1.两肩两眼-------------------
        //两肩水平检测
        private void CheckShoulder(Landmark ls, Landmark rs)
        {

            if (ls == null || rs == null)
            {
                result.ShoulderState = Constants.TiltSeverity.Unknown; // 更新为未知
                result.ShoulderTiltAngle = 0f; // 角度也设为0或特定值
                return;
            }
            // 计算双肩连线角度
            float deltaY = (rs.y - ls.y) * Constants.Height;
            float deltaX = (rs.x - ls.x) * Constants.Width;

            float calculatedAngleDegrees;       //计算结果的角度--

            // 处理两个地标点几乎重合的特殊情况
            if (Math.Abs(deltaX) < 0.0001f && Math.Abs(deltaY) < 0.0001f)
            {
                calculatedAngleDegrees = 0f;
                result.ShoulderState = Constants.TiltSeverity.Unknown;
                result.ShoulderTiltAngle = calculatedAngleDegrees;
                return;
            }

            // 使用 Math.Atan2(y, x) 计算角度 (弧度)
            // 角度是从 X 轴正方向开始，逆时针测量。
            // 对于屏幕坐标系 (Y 轴向下):
            //   - 水平 (deltaY = 0, deltaX > 0): angle = 0 rad
            //   - 右肩低于左肩 (deltaY > 0, deltaX > 0): angle > 0 rad
            //   - 右肩高于左肩 (deltaY < 0, deltaX > 0): angle < 0 rad
            double angleRadians = Math.Atan2(deltaY, deltaX);

            // 将弧度转换为角度
            calculatedAngleDegrees = (float)(angleRadians * 180.0 / Math.PI);
            result.ShoulderTiltAngle = calculatedAngleDegrees; // 存储计算出的原始角度

            // 根据角度判断倾斜程度并更新 result.ShoulderState
            float absAngle = Math.Abs(calculatedAngleDegrees);
            absAngle = Math.Abs(180 - absAngle);

            if (absAngle <= Constants.SlightShoulderHorizontalDeg)
            {
                //认为两肩水平
                result.ShoulderState = TiltSeverity.Level;
            }
            else if (calculatedAngleDegrees > 0 && ((180 - calculatedAngleDegrees) > Constants.MaxShoulderHorizontalDeg))
            {
                // 左肩显著高
                result.ShoulderState = TiltSeverity.LeftObviouslyHigh; return;
            }
            else if (calculatedAngleDegrees < 0 && ((180 + calculatedAngleDegrees) > Constants.MaxShoulderHorizontalDeg))
            {
                //右肩显著高
                result.ShoulderState = TiltSeverity.RightObviouslyHigh; return;
            }

            else if (calculatedAngleDegrees > 0 && ((180 - calculatedAngleDegrees) > Constants.SlightShoulderHorizontalDeg))
            {
                // 左肩略微高
                result.ShoulderState = TiltSeverity.LeftSlightlyHigh;
            }
            else if (calculatedAngleDegrees < 0 && ((180 + calculatedAngleDegrees) > Constants.SlightShoulderHorizontalDeg))
            {
                // 右肩略微高
                result.ShoulderState = TiltSeverity.RightSlightlyHigh;
            }

            else
            {
                //这里不会用到，故缺省
            }

        }

        //两眼水平检测
        private void CheckEye(Landmark le, Landmark re)
        {
            if (le == null || re == null)
            {
                result.EyeState = Constants.TiltSeverity.Unknown;
                result.EyeTiltAngle = 0f;
                return;
            }

            // 计算双眼连线在像素坐标系中的差值
            // 假设 le.y, le.x 等是归一化坐标 (0-1)
            float deltaY = (re.y - le.y) * Constants.Height; // 右眼Y - 左眼Y
            float deltaX = (re.x - le.x) * Constants.Width;  // 右眼X - 左眼X

            float calculatedAngleDegrees;

            // 处理两个地标点几乎重合的特殊情况 (理论上眼睛不会完全重合)
            if (Math.Abs(deltaX) < 0.0001f && Math.Abs(deltaY) < 0.0001f)
            {
                calculatedAngleDegrees = 0f;
                result.EyeState = Constants.TiltSeverity.Level;
                result.EyeTiltAngle = calculatedAngleDegrees;
                return;
            }

            // 使用 Math.Atan2(y, x) 计算角度 (弧度)
            // 角度约定与肩膀相同:
            // 正值: 右眼低于左眼 (即左眼相对偏高)
            // 负值: 右眼高于左眼 (即右眼相对偏高)
            double angleRadians = Math.Atan2(deltaY, deltaX);

            // 将弧度转换为角度
            calculatedAngleDegrees = (float)(angleRadians * 180.0 / Math.PI);
            result.EyeTiltAngle = calculatedAngleDegrees; // 存储计算出的原始角度

            // 根据角度判断倾斜程度并更新 result.EyeState
            float absAngle = Math.Abs(calculatedAngleDegrees);
            absAngle = Math.Abs(180 - absAngle);

            if (absAngle <= Constants.SlightEyeHorizontalDeg) // 使用眼睛特定的阈值
            {
                result.EyeState = Constants.TiltSeverity.Level;
            }
            else if (calculatedAngleDegrees > 0 && ((180 - calculatedAngleDegrees) > Constants.MaxEyeHorizontalDeg))
            {
                result.EyeState = Constants.TiltSeverity.LeftObviouslyHigh; return;
            }
            else if (calculatedAngleDegrees < 0 && ((180 + calculatedAngleDegrees) > Constants.MaxEyeHorizontalDeg))
            {
                result.EyeState = Constants.TiltSeverity.RightObviouslyHigh; return;
            }
            // 右眼低于左眼 => 左眼更高
            else if (calculatedAngleDegrees > 0 && ((180 - calculatedAngleDegrees) > Constants.SlightEyeHorizontalDeg))
            {
                result.EyeState = Constants.TiltSeverity.LeftSlightlyHigh;
            }
            // 右眼高于左眼 => 右眼更高
            else if (calculatedAngleDegrees < 0 && ((180 + calculatedAngleDegrees) > Constants.SlightEyeHorizontalDeg))
            {
                result.EyeState = Constants.TiltSeverity.RightSlightlyHigh;
            }

            else
            {
                //缺省    
            }
        }

        //2.驼背检测---------------------
        public void CheckHunchback(Landmark ls, Landmark rs, Landmark nos)
        {
            if (ls == null || rs == null || nos == null)
            {
                result.HunchbackState = HunchbackSeverity.Unknown;
                return;
            }
            ls.z = ls.z * Constants.Depth;
            rs.z = rs.z * Constants.Depth;
            nos.z = nos.z * Constants.Depth;

            // 1. 计算双肩的平均Z坐标
            //    注意：这里简单取平均。更复杂的模型可能会考虑双肩连线的中点。
            float shoulderAverageZ = (ls.z + rs.z) / 2.0f;

            // 2. 计算鼻子Z坐标与肩膀平均Z坐标的差异
            //    如果鼻子比肩膀更靠前（Z值更小），则 difference 为负值。
            float zDifference = nos.z - shoulderAverageZ;
            // result.NoseShoulderZDifference = zDifference;

            // 3. 根据阈值判断是否驼背 (颈部前倾)
            //    Constants.NeckForwardThresholdZ 是一个正值。
            //    我们期望 nos.z < (shoulderAverageZ - NeckForwardThresholdZ)
            //    这等价于 nos.z - shoulderAverageZ < -NeckForwardThresholdZ
            //    即 zDifference < -Constants.NeckForwardThresholdZ

            // z坐标越小说明离屏幕越近
            if (zDifference >= Constants.NeckForwardThresholdZ)
            {
                //如果鼻子在肩膀后面，说明没有驼背
                result.HunchbackState = HunchbackSeverity.NoHunchback; //没有驼背
            }

            else if (zDifference < Constants.NeckForwardThresholdZ && zDifference > Constants.NectForwardThresholdZObvious)
                result.HunchbackState = HunchbackSeverity.SlightHunchback; //轻微驼背
            else result.HunchbackState = HunchbackSeverity.ObviousHunchback; //严重驼背

        }

        //3.头部倾斜---------------------
        public void CheckHead(Landmark le, Landmark re, Landmark nos)
        {
            if (le == null || re == null)
            {
                result.HeadTiltState = HeadTiltSeverity.Unknown;
                result.HeadTiltAngle = 0f;
                return;
            }

            // 计算双眼连线在像素坐标系中的差值
            // 假设 le.y, le.x 等是归一化坐标 (0-1)
            float deltaY = (re.y - le.y) * Constants.Height; // 右眼Y - 左眼Y
            float deltaX = (re.x - le.x) * Constants.Width;  // 右眼X - 左眼X

            float calculatedAngleDegrees;

            if (Math.Abs(deltaX) < 0.0001f && Math.Abs(deltaY) < 0.0001f)
            {
                calculatedAngleDegrees = 0f;
                result.HeadTiltState = HeadTiltSeverity.Upright;
                result.HeadTiltAngle = calculatedAngleDegrees;
                return;
            }

            // 使用 Math.Atan2(y, x) 计算角度 (弧度)
            // 角度约定:
            //   Y轴向下。Atan2(deltaY, deltaX)
            //   正角度: 右眼低于左眼 (头部向观察者的左侧倾斜，即人物的左倾)
            //   负角度: 右眼高于左眼 (头部向观察者的右侧倾斜，即人物的右倾)
            double angleRadians = Math.Atan2(deltaY, deltaX);

            calculatedAngleDegrees = (float)(angleRadians * 180.0 / Math.PI);
            float tmpAngleDegrees;
            tmpAngleDegrees = (180 - Math.Abs(calculatedAngleDegrees)) * (Math.Abs(calculatedAngleDegrees) / calculatedAngleDegrees);
            result.HeadTiltAngle = tmpAngleDegrees;

            float absAngle = Math.Abs(calculatedAngleDegrees);
            absAngle = Math.Abs(180 - absAngle);


            if (absAngle <= Constants.MaxHeadUprightAngle)
            {
                result.HeadTiltState = HeadTiltSeverity.Upright;
            }
            // tmpAngleDegrees < 0: 头部向人物的左侧倾斜
            else if (tmpAngleDegrees < 0)
            {
                if (absAngle <= Constants.SlightHeadTiltThreshold)
                {
                    result.HeadTiltState = HeadTiltSeverity.SlightlyTiltedLeft;
                }
                else // absAngle > Constants.SlightHeadTiltThreshold
                {
                    result.HeadTiltState = HeadTiltSeverity.SignificantlyTiltedLeft;
                }
            }
            // calculatedAngleDegrees < 0: 头部向人物的右侧倾斜
            else if (tmpAngleDegrees > 0)
            {
                // 此时 calculatedAngleDegrees 是负数, absAngle 是其绝对值
                if (absAngle <= Constants.SlightHeadTiltThreshold)
                {
                    result.HeadTiltState = HeadTiltSeverity.SlightlyTiltedRight;
                }
                else // absAngle > Constants.SlightHeadTiltThreshold
                {
                    result.HeadTiltState = HeadTiltSeverity.SignificantlyTiltedRight;
                }
            }
        }

        //4.头部朝向检测------------------
        private float DistancePointToLine(Landmark point, Landmark lineP1, Landmark lineP2)
        {
            if (point == null || lineP1 == null || lineP2 == null) return float.MaxValue;

            // 使用向量叉积的模除以基线向量的模
            // 或者使用点到直线的距离公式: |Ax0 + By0 + C| / sqrt(A^2 + B^2)
            // 直线方程: (y1-y2)x + (x2-x1)y + (x1y2 - x2y1) = 0
            // A = y1-y2, B = x2-x1, C = x1y2 - x2y1
            // (x0, y0) 是 point 的坐标
            double A = lineP1.y - lineP2.y;
            double B = lineP2.x - lineP1.x;
            double C = (lineP1.x * lineP2.y) - (lineP2.x * lineP1.y);

            return (float)(Math.Abs(A * point.x + B * point.y + C) / Math.Sqrt(A * A + B * B));
        }

        public void AnalyzeHeadDirection_Combined(List<Landmark> landmarks, int imageWidth, int imageHeight)
        {
            if (landmarks == null || landmarks.Count < 400) // 确保有足够的Face Mesh点
            {
                result.HeadYawDirection = HeadOrientationHorizontal.Unknown;
                result.HeadPitchDirection = HeadOrientationVertical.Unknown;
                return;
            }

            // --- 获取关键点 ---
            Landmark noseTip = landmarks[NOSE_TIP];
            Landmark chinCenter = landmarks[CHIN_CENTER];
            Landmark leftEyeOuter = landmarks[LEFT_EYE_OUTER_CORNER];
            Landmark rightEyeOuter = landmarks[RIGHT_EYE_OUTER_CORNER];
            Landmark leftMouthCorner = landmarks[LEFT_MOUTH_CORNER];
            Landmark rightMouthCorner = landmarks[RIGHT_MOUTH_CORNER];
            Landmark leftEar = landmarks.Count > LEFT_EAR_TRAGION ? landmarks[LEFT_EAR_TRAGION] : null; // 检查索引是否存在
            Landmark rightEar = landmarks.Count > RIGHT_EAR_TRAGION ? landmarks[RIGHT_EAR_TRAGION] : null;
            Landmark foreheadCenter = landmarks[BETWEEN_EYEBROWS]; // 眉心

            if (noseTip == null || chinCenter == null || leftEyeOuter == null || rightEyeOuter == null ||
                leftMouthCorner == null || rightMouthCorner == null || foreheadCenter == null)
            {
                result.HeadYawDirection = HeadOrientationHorizontal.Unknown;
                result.HeadPitchDirection = HeadOrientationVertical.Unknown;
                return;
            }

            // --- 1. 水平朝向 (Yaw) ---
            int yawScore = 0; // 正值表示向右看，负值表示向左看

            // 1a. 对称性分析
            // 中轴线 (鼻尖到下巴)
            // (注意：更鲁棒的中轴线可能需要平均更多点，或者使用solvePnP得到的头部坐标系的Y轴投影)
            if (noseTip != null && chinCenter != null)
            {
                float distLeftEyeToMidline = DistancePointToLine(leftEyeOuter, noseTip, chinCenter);
                float distRightEyeToMidline = DistancePointToLine(rightEyeOuter, noseTip, chinCenter);
                float distLeftMouthToMidline = DistancePointToLine(leftMouthCorner, noseTip, chinCenter);
                float distRightMouthToMidline = DistancePointToLine(rightMouthCorner, noseTip, chinCenter);

                // 转换为像素单位再比较，或者使用归一化距离的比例
                // 这里假设距离是归一化单位，需要乘以图像宽度来得到像素感觉
                distLeftEyeToMidline *= imageWidth;
                distRightEyeToMidline *= imageWidth;
                distLeftMouthToMidline *= imageWidth;
                distRightMouthToMidline *= imageWidth;


                // 眼睛对称性
                float eyeDistDiffRatio = (distLeftEyeToMidline - distRightEyeToMidline) / ((distLeftEyeToMidline + distRightEyeToMidline) / 2f + 1e-6f);
                if (eyeDistDiffRatio > SymmetryDifferenceRatioThreshold) yawScore++; // 右眼远，左眼近 => 头向左转
                if (eyeDistDiffRatio < -SymmetryDifferenceRatioThreshold) yawScore--; // 右眼近，左眼远 => 头向右转

                // 嘴巴对称性
                float mouthDistDiffRatio = (distLeftMouthToMidline - distRightMouthToMidline) / ((distLeftMouthToMidline + distRightMouthToMidline) / 2f + 1e-6f);
                if (mouthDistDiffRatio > SymmetryDifferenceRatioThreshold) yawScore++;
                if (mouthDistDiffRatio < -SymmetryDifferenceRatioThreshold) yawScore--;
            }


            // 1b. 深度线索 (耳朵Z坐标)
            if (leftEar != null && rightEar != null)
            {
                float earZDiff = Constants.Depth * (leftEar.z - rightEar.z);
                // z越小越近。如果 leftEar.z < rightEar.z (diff < 0), 说明右耳更近，头可能向左转
                //Console.WriteLine("--earZDiff--------     " + earZDiff + "---------阈值10-------");
                if (earZDiff > EarZDifferenceThreshold) yawScore--; // 右耳远，左耳近 => 头向右转
                if (earZDiff < -EarZDifferenceThreshold) yawScore++; // 右耳近，左耳远 => 头向左转
            }

            // 综合Yaw分数判断
            if (yawScore > 0) result.HeadYawDirection = HeadOrientationHorizontal.Left;
            else if (yawScore < 0) result.HeadYawDirection = HeadOrientationHorizontal.Right;
            else result.HeadYawDirection = HeadOrientationHorizontal.Forward;


            // --- 2. 垂直朝向 (Pitch) ---
            // 使用方法一的简化逻辑：鼻子相对于眼睛的Y位置
            // 或者额头和下巴的Z坐标差异 (更依赖Z的稳定性)
            int pitchScore = 0;

            /*
            // 2a. 鼻子与眼睛的Y坐标比较 (Y轴向下为正)
            float eyeCenterY = (leftEyeOuter.y + rightEyeOuter.y) / 2f;
            float noseEyeYDiffNormalized = Constants.Height * (noseTip.y - eyeCenterY);
            Console.WriteLine("--noseEyeYDiffNormalized--------      " + noseEyeYDiffNormalized + "----阈值10------------");

            if (noseEyeYDiffNormalized > PitchNoseEyeYThresholdNormalized) pitchScore++; // 鼻子在眼下方 => 低头
            if (noseEyeYDiffNormalized < -PitchNoseEyeYThresholdNormalized) pitchScore--; // 鼻子在眼上方 => 抬头
            */

            // 2b. 额头和下巴的Z坐标比较 (可选，如果Z坐标可靠)
            if (foreheadCenter != null && chinCenter != null)
            {
                float foreheadChinZDiff = Constants.Depth * (foreheadCenter.z - chinCenter.z);
                //Console.WriteLine("--foreheadChinZDiff----     " + foreheadChinZDiff + "-----阈值18------负8------");
                // z越小越近。如果额头比下巴近 (forehead.z < chin.z, diff < 0) => 可能抬头
                // 如果额头比下巴远 (forehead.z > chin.z, diff > 0) => 可能低头
                if (foreheadChinZDiff > PitchForeheadChinZThreshold) pitchScore++; // 额头远，下巴近 => 抬头
                if (foreheadChinZDiff < -PitchForeheadChinZThreshold+15) pitchScore--; // 额头近，下巴远 => 低头
            }

            // Pitch分数判断
            if (pitchScore > 0) result.HeadPitchDirection = HeadOrientationVertical.Up;
            else if (pitchScore < 0) result.HeadPitchDirection = HeadOrientationVertical.Down;
            else result.HeadPitchDirection = HeadOrientationVertical.Straight;
          
        }

        //5.综合评价---------------------
        public void ComprehensiveAnalyse() {
            //AnalysisResult:
            /*
            ShoulderState
            EyeState
                Unknown,
                Level,
                LeftSlightlyHigh,
                RightSlightlyHigh,
                LeftObviouslyHigh,
                RightObviouslyHigh

            HunchbackState
                Unknown,        // 无法判断
                NoHunchback,    // 未检测到驼背
                SlightHunchback, // 轻微驼背
                ObviousHunchback // 明显驼背 

            HeadTiltState
                Unknown,
                Upright,
                SlightlyTiltedLeft,
                SignificantlyTiltedLeft,
                SlightlyTiltedRight,
            */
            //按照优先级：
            //如果有未能检测出来的Unknown状态
            if (result.ShoulderState== TiltSeverity.Unknown|| result.EyeState== TiltSeverity.Unknown
                || result.HunchbackState== HunchbackSeverity.Unknown|| result.HeadTiltState== HeadTiltSeverity.Unknown) {
                result.OverallPostureStatus = OverallPosture.Unknown;
            }
            //如果有检测出来的严重情况
            else if (result.ShoulderState == TiltSeverity.LeftObviouslyHigh || result.EyeState == TiltSeverity.LeftObviouslyHigh
                || result.ShoulderState == TiltSeverity.RightObviouslyHigh || result.EyeState == TiltSeverity.RightObviouslyHigh
                || result.HunchbackState == HunchbackSeverity.ObviousHunchback || result.HeadTiltState == HeadTiltSeverity.SignificantlyTiltedLeft)
            {
                result.OverallPostureStatus = OverallPosture.SeverelyPoor;
            }
            //如果有检测出来的轻微严重情况
            else if (result.ShoulderState == TiltSeverity.LeftSlightlyHigh || result.EyeState == TiltSeverity.LeftSlightlyHigh
                || result.ShoulderState == TiltSeverity.RightSlightlyHigh || result.EyeState == TiltSeverity.RightSlightlyHigh
                || result.HunchbackState == HunchbackSeverity.SlightHunchback || result.HeadTiltState == HeadTiltSeverity.SlightlyTiltedLeft
                || result.HeadTiltState == HeadTiltSeverity.SlightlyTiltedRight)
            {
                result.OverallPostureStatus = OverallPosture.MinorPoor;
            }
            //体态检测的各个指标全部正常，则该时刻的综合
            else
            {
                result.OverallPostureStatus = OverallPosture.StandardPosture;
            }
            
        }

        #endregion
    }
}