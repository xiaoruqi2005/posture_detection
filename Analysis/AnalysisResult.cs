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

        // 4. 综合评估
        public string OverallPostureStatus { get; set; } = "评估中..."; // 如: "姿势标准", "轻微不良", "严重不良，请调整"
        public List<string> DetectedIssues { get; private set; } // 存储所有检测到的问题描述

        // 5. 原始数据时间戳或帧号 (可选)
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
