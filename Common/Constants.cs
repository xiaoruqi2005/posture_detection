//using Analysis;

namespace Common
{
    public class Constants  //定义一些需要的常量类型
    {
        public const String pythonScriptFileName = "pythonScripts\\Holistic_server.py";         // python 脚本路径
        public const String pythonScriptPath2 = "pythonScripts\\pose_server.py";
        //public const String pythonInterpreterPath = "D:\\python_project\\pythonProject\\.venv\\Scripts\\python.exe"; // Python 解释器路径 注意对应的python环境下需要有mediapipe包
        // public const String pythonExecutable = "pythonScripts\\Holistic_server\\Holistic_server.exe"; // python 可执行文件路径 
        public const String pythonInterpreterPath = "pythonScripts\\PythonEnv\\Scripts\\python.exe";
        public const int InterValMs = 500; // 解析间隔时间
        public const int waitTime = 5000; // 等待时间 ，单位 毫秒

        //public static readonly AnalysisResult result;//存储结果的静态对象

        //绘图尺寸
        public const int DrawWidth = 640; // 绘图宽度
        public const int DrawHeight = 480; // 绘图高度

        # region 各种姿势的阈值常量设置
        //1.两肩两眼水平阈值----------------------

        //轻微偏离
        public const float SlightShoulderHorizontalDeg = 4f;         //两肩轻微偏离允许角度 5°
        public const float SlightEyeHorizontalDeg =3.5f;             //眼睛水平度轻微偏离允许角度 5°                                                        
        public const float SlightVerticalDeg = 5f;              //头部倾斜的轻微偏离允许角度 5°
        //显著偏离
        public const float MaxShoulderHorizontalDeg = 6f;         //两肩显著偏离最大允许角度 10°
        public const float MaxEyeHorizontalDeg = 7f;             //眼睛水平度显著偏离最大允许角度 10°
        public const float MaxVerticalDeg =7.5f;              //头部倾斜的显著偏离最大允许角度 10°

        //2.驼背阈值-----------------------------

        public const float NeckForwardThresholdZ = -90f;      //轻微驼背阈值   （z轴差异）为负值
        public const float NectForwardThresholdZObvious = -120f;      //严重驼背阈值  （z轴差异） 为负值

        //3.头部倾斜阈值--------------------------
        public const float MaxHeadUprightAngle = 4f;      // 头部正直的最大允许角度偏差
        public const float SlightHeadTiltThreshold = 7f; // 轻微头部倾斜的阈值下限 (应 > MaxHeadUprightAngle)

        //public static float HunchbackAngleThreshold = 10f; // 驼背角度阈值
        //public static float SpineCurvatureThreshold = 0.2f; // 脊柱弯曲阈值

        public const int Width = 1920;  //宽度和高度的像素值 
        public const int Height = 1200;
        public const int Depth = 100;   //用于描述深度的相对值

        //4.头部朝向的阈值--------------------------
        public const float EarZDifferenceThreshold = 10; // 耳朵z轴差异阈值
        public const float SymmetryDifferenceRatioThreshold = 100; // 耳朵对称差异比率阈值
        public const float HeadYawThreshold = 30; // 头部水平朝向的阈值
        public const float HeadPitchThreshold = 30; // 头部竖直朝向的阈值
        public const float PitchNoseEyeYThresholdNormalized = 10;
        public const float PitchForeheadChinZThreshold = 18;

        #endregion


        #region 各种姿势的枚举类型

        //1.两肩两眼倾斜的枚举类型
        public enum TiltSeverity
        {
            /// <summary>
            /// 未知或无法评估（例如，缺少关键点）
            /// </summary>
            Unknown,

            /// <summary>
            /// 双侧在可接受的范围内保持水平
            /// </summary>
            Level,

            /// <summary>
            /// 左侧轻微偏高
            /// </summary>
            LeftSlightlyHigh,

            /// <summary>
            /// 右侧轻微偏高
            /// </summary>
            RightSlightlyHigh,

            /// <summary>
            /// 左侧显著偏高
            /// </summary>
            LeftObviouslyHigh,

            /// <summary>
            /// 右侧显著偏高
            /// </summary>
            RightObviouslyHigh
        }
        //2.驼背状态的枚举类型
        public enum HunchbackSeverity 
        {
            Unknown,        // 无法判断
            NoHunchback,    // 未检测到驼背
            SlightHunchback, // 轻微驼背
            ObviousHunchback // 明显驼背 

        }
        //3.头部倾斜的枚举类型
        public enum HeadTiltSeverity
        {
            /// <summary>
            /// 未知或无法评估
            /// </summary>
            Unknown,

            /// <summary>
            /// 头部保持正直（在允许范围内）
            /// </summary>
            Upright,

            /// <summary>
            /// 头部轻微左倾
            /// </summary>
            SlightlyTiltedLeft,

            /// <summary>
            /// 头部明显左倾
            /// </summary>
            SignificantlyTiltedLeft,

            /// <summary>
            /// 头部轻微右倾
            /// </summary>
            SlightlyTiltedRight,

            /// <summary>
            /// 头部明显右倾
            /// </summary>
            SignificantlyTiltedRight
        }   
      
        //4.头部朝向的枚举类型
        public enum HeadOrientationHorizontal//左右倾
        {
            Unknown,
            Forward,//前 左 右
            Left,
            Right
        }  
       
        public enum HeadOrientationVertical //上下倾
        {
            Unknown,
            Straight, //前 上 下
            Up,
            Down
        }

        //5.综合评估的枚举类型
        public enum OverallPosture 
        { 
            Unknown,
            StandardPosture,
            MinorPoor,
            SeverelyPoor//未知 姿势标准 轻微不良 严重不良 
        }
        
        #endregion 


    }
}
