using Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analysis
{
    class AnalysisResult
    {
        // 1. 双肩水平度分析
        public Landmark? LeftShoulder { get; set; }
        public Landmark? RightShoulder { get; set; }
        public float? ShoulderAngleDeg { get; set; } // 肩膀连线与水平线的夹角 (度)
        public bool? AreShouldersLevel { get; set; } // 肩膀是否水平 (基于阈值)
        public string ShoulderLevelDescription { get; set; } = "未知"; // 如: "双肩水平", "左肩偏高", "右肩偏高"

        // 2. 双眼水平度分析
        public Landmark? LeftEye { get; set; }
        public Landmark? RightEye { get; set; }
        public float? EyeAngleDeg { get; set; }      // 双眼连线与水平线的夹角 (度)
        public bool? AreEyesLevel { get ; set; }    // 双眼是否水平 (基于阈值)
        public string EyeLevelDescription { get; set; } = "未知";     // 如: "双眼水平", "左眼偏高", "右眼偏高"

        // 3. 头部倾斜分析
        public Landmark? Nose { get; set; }
        public float? HeadTiltAngleDeg { get; set; } // 头部中轴线与垂直线的夹角 (度)
        public bool? IsHeadStraight { get; set; }   // 头部是否正直 (基于阈值)
        public string HeadTiltDescription { get; set; } = "未知";     // 如: "头部正直", "向左歪头", "向右歪头"

        // 4. 颈部前倾属性
        public float?NeckForwardDistance { get; set; }//前倾距离
        public bool? IsNeckForward { get; set; }//是否前倾
        public string NeckForwardDescription { get; set; } = "未知";  //如："颈部稍前倾"，"颈部过度前倾"，"颈部正直"

        //5 .视线检测属性
        public bool? IsGazeOnScreen {  get; set; }//视线是否在屏幕上
        public float? GazeOffsetAngle {  get; set; }//视线与屏幕偏离角度
        public Vector3 GazeDirection { get; set; }//视线的方向向量 e.g.(1,2,3)
                                                  // public string GazeOffsetDirection {  get; set; }//


        //6 .专注度
        public float FocusScore { get; set; } = 1f;//专注度得分
        public bool IsFocused { get; set; } = true;//根据专注度来判断是否专注
        public float DistractionDurationRatio { get; set; }//分心持续时间比率
        public float BlinkFrequency { get; set; }  // 眨眼频率（次/分钟）
        public float HeadMovementMagnitude { get; set; } // 头部运动幅度

        //7.表情类        
        public enum FacialExpression
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
            PoliteSmile = Happiness & ~EyeAction, // 礼节性微笑（仅嘴角动作）
            DuchenneSmile = Happiness | EyeAction // 真诚微笑（包含眼周动作）
        }

        // 微表情分析结构体
        public struct MicroExpression
        {
            public FacialExpression Expression { get; set; }
            public float Duration { get; set; }    // 持续时间（秒）
            public float Intensity { get; set; }   // 强度系数 [0-1]
            public DateTime StartTime { get; set; } // 出现时间戳
        }


        //8 . 综合评估
        public string OverallPostureStatus { get; set; } = "评估中..."; // 如: "姿势标准", "轻微不良", "严重不良，请调整"
        public List<string> DetectedIssues { get; private set; } // 存储所有检测到的问题描述

        //9 . 原始数据时间戳或帧号 (可选)
        public long? TimestampMs { get; set; }
        public int? FrameId { get; set; }



        public AnalysisResult()
        {
            DetectedIssues = new List<string>();
        }

        public bool IsValidForAnalysis()
        {
            // 检查是否有足够的数据进行基础分析
            //  return LeftShoulder.HasValue && RightShoulder.HasValue &&
            //   LeftEye.HasValue && RightEye.HasValue && Nose.HasValue;
            return true;
        }

        public override string ToString()
        {
            var report = new StringBuilder();
            report.AppendLine($"姿态分析结果 (帧: {FrameId?.ToString() ?? "N/A"}, 时间戳: {TimestampMs?.ToString() ?? "N/A"}ms):");

            if (!IsValidForAnalysis() && LeftShoulder == null) // A bit more specific for incomplete data
            {
                if (DetectedIssues.Any())
                {
                    report.AppendLine($"  - {DetectedIssues.First()}");
                }
                else
                {
                    report.AppendLine("  - 关键点数据不足，无法进行完整分析。");
                }
            }
            else
            {
                report.AppendLine($"  - 双肩分析: {ShoulderLevelDescription} (角度: {ShoulderAngleDeg?.ToString("F2") ?? "N/A"}°)");
                report.AppendLine($"  - 双眼分析: {EyeLevelDescription} (角度: {EyeAngleDeg?.ToString("F2") ?? "N/A"}°)");
                report.AppendLine($"  - 头部倾斜: {HeadTiltDescription} (角度: {HeadTiltAngleDeg?.ToString("F2") ?? "N/A"}° vs 垂直)");
            }

            report.AppendLine($"  - 总体评价: {OverallPostureStatus}");
            if (DetectedIssues.Any() && IsValidForAnalysis()) // Only show detailed issues if analysis was possible
            {
                report.AppendLine($"  - 检测到的问题 ({DetectedIssues.Count}):");
                foreach (var issue in DetectedIssues)
                {
                    report.AppendLine($"    - {issue}");
                }
            }
            else if (!IsValidForAnalysis() && !DetectedIssues.Any(x => x.Contains("提取关键点失败")))
            {
                // if it's not valid but no explicit extraction error, state no issues detected due to lack of data
            }
            else if (IsValidForAnalysis() && !DetectedIssues.Any())
            {
                report.AppendLine("  - 未检测到明显姿势问题。");
            }


            return report.ToString();
        }
    }
}
