using Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using static Common.Constants;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace Analysis
{
    public class AnalysisResult
    {
        //图形回传的暂存对象
        public Bitmap FrameData = new Bitmap(DrawHeight, DrawWidth);

        public DateTime Timestamp { get; set; }
        // 1. 双肩水平度分析
        public float? ShoulderTiltAngle { get; set; } // 肩膀连线与水平线的夹角 (度)

        public TiltSeverity ShoulderState;
       

        // 2. 双眼水平度分析
        public float? EyeTiltAngle { get; set; }      // 双眼连线与水平线的夹角 (度)
        public Constants.TiltSeverity EyeState { get; set; }  //基于阈值的眼睛水平度状态


       //驼背状态分析 
       public HunchbackSeverity HunchbackState { get; set; } // 驼背状态

        // 3. 头部倾斜分析
        public float? HeadTiltAngle { get; set; } // 头部中轴线与垂直线的夹角 (度)
        public HeadTiltSeverity HeadTiltState { get; set; } // 头部倾斜状态


        // 头部朝向检测
        //public HeadPose HeadPoseData { get; set; } // 存储头部姿态（旋转和平移向量）
       // public Vector3 EulerAnglesDegrees { get; set; } // 存储计算出的欧拉角 (Pitch, Yaw, Roll) 单位：度
/*
        public HeadOrientationHorizontal HeadYawDirection { get; set; } = HeadOrientationHorizontal.Unknown;
        public HeadOrientationVertical HeadPitchDirection { get; set; } = HeadOrientationVertical.Unknown;


        //5 .视线检测属性
        public bool? IsGazeOnScreen {  get; set; }//视线是否在屏幕上
        public float? GazeOffsetAngle {  get; set; }//视线与屏幕偏离角度
        public Vector3 GazeDirection { get; set; }//视线的方向向量 e.g.(1,2,3)*/
                                                  // public String GazeOffsetDirection {  get; set; }//
        //6 .专注度
/*        public float FocusScore { get; set; } = 1f;//专注度得分
        public bool IsFocused { get; set; } = true;//根据专注度来判断是否专注
        public float DistractionDurationRatio { get; set; }//分心持续时间比率
        public float BlinkFrequency { get; set; }  // 眨眼频率（次/分钟）
        public float HeadMovementMagnitude { get; set; } // 头部运动幅度*/

        //7.表情的枚举类型     
   /*     public enum FacialExpression
        {
            // 基础情绪（Ekman六大基本情绪）
            Neutral = 0,         // 中性表情
            Happiness = 1 << 0,  // 快乐（嘴角上扬，眼周皱起）
            Sadness = 1 << 1,   // 悲伤（眉毛内角上扬，嘴角下垂）
            Anger = 1 << 2,     // 愤怒（眉毛下压，嘴唇紧闭）
            Surprise = 1 << 3,  // 惊讶（眉毛抬高，下颌下垂）
            Fear = 1 << 4,      // 恐惧（眉毛抬高聚拢，上眼睑提升）
            Disgust = 1 << 5,   // 厌恶（鼻梁皱起，上唇提升）

            // 复合表情（基于FACS编码系统）
            Contempt = 1 << 6,  // 轻蔑（单侧嘴角上扬）
            Confusion = Anger | Surprise,         // 困惑（愤怒+惊讶）
            Delight = Happiness | Surprise,       // 惊喜（快乐+惊讶）
            Frustration = Anger | Sadness,        // 沮丧（愤怒+悲伤）
            Anxiety = Fear | Sadness,             // 焦虑（恐惧+悲伤）

            // 特殊状态检测
            Pain = 1 << 7,       // 疼痛（眼睑紧闭，上唇提升）
            Fatigue = 1 << 8,    // 疲劳（眼睑下垂，头部倾斜）
            Focused = 1 << 9,    // 专注（眼睑微缩，头部前倾）
            Skepticism = Disgust | Surprise,     // 怀疑（厌恶+惊讶）

            // 社交表情
          *//*  PoliteSmile = Happiness & ~EyeAction, // 礼节性微笑（仅嘴角动作）
            DuchenneSmile = Happiness | EyeAction // 真诚微笑（包含眼周动作）*//*
        }

        // 微表情分析结构体
        public struct MicroExpression
        {
            public FacialExpression Expression { get; set; }
            public float Duration { get; set; }    // 持续时间（秒）
            public float Intensity { get; set; }   // 强度系数 [0-1]
            public DateTime StartTime { get; set; } // 出现时间戳
        }
*/

        //8 . 综合评估
      /*  public String OverallPostureStatus { get; set; } = "评估中..."; // 如: "姿势标准", "轻微不良", "严重不良，请调整"
        public List<String> DetectedIssues { get; private set; } // 存储所有检测到的问题描述
*/
        //9 . 原始数据时间戳或帧号 (可选)
      
        //public int? FrameId { get; set; }


        public bool IsValidForAnalysis()
        {
            // 检查是否有足够的数据进行基础分析
            //  return LeftShoulder.HasValue && RightShoulder.HasValue &&
            //   LeftEye.HasValue && RightEye.HasValue && Nose.HasValue;
            return true;
        }

        public override String ToString()
//打印分析结果
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("AnalysisResult: \n");
            sb.Append("ShoulderTiltAngle: " + ShoulderTiltAngle + "\n");
            sb.Append("ShoulderState: " + ShoulderState + "\n");
            sb.Append("EyeTiltAngle: " + EyeTiltAngle + "\n");
            sb.Append("EyeState: " + EyeState + "\n");
            sb.Append("HunchbackState: " + HunchbackState + "\n");
            sb.Append("HeadTiltAngle: " + HeadTiltAngle + "\n");
            sb.Append("HeadTiltState: " + HeadTiltState + "\n");

            sb.Append("TimeStamp" + Timestamp + "\n");
            sb.Append("Formated TimeStamp" + Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "\n");
            sb.Append('}');
            // 创建新线程避免阻塞控制台
            // 遍历图像像素并转换为ASCII字符
            /*if (FrameData != null)
            {
                try
                {
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test_output.jpg");
                    FrameData.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Console.WriteLine("✔️ 图像保存成功: " + path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ 图像保存失败: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("❌ Bitmap 为 null，无法保存");
            }


            */
            return sb.ToString();
        }

        //public void recieve()
        //{
        //    dataGridView1.Rows.Clear();//清空旧数据
        //    string sql = "insert into data_table (sa, ss, ea, es, hs, heads, times,ft) " +
        //        "values ('" + ShoulderTiltAngle + "', '" + ShoulderState + "', '" + EyeTiltAngle + "', '" + EyeState + "', '" + HunchbackState + "', '" + HeadTiltAngle + "', '" + Timestamp + "','" + Timestamp + "');";
        //    DataBase da = new DataBase();
        //    da.InsertData(sql);
        //}
    }
}
