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
      //  public Bitmap FrameData = new Bitmap(DrawHeight, DrawWidth);

        public DateTime Timestamp { get; set; }
        // 1. 双肩水平度分析
        public float? ShoulderTiltAngle { get; set; } // 肩膀连线与水平线的夹角 (度)

        public TiltSeverity ShoulderState;
       

            //双眼水平度
        public float? EyeTiltAngle { get; set; }      // 双眼连线与水平线的夹角 (度)
        public TiltSeverity EyeState { get; set; }  //基于阈值的眼睛水平度状态


       //2.驼背状态分析 -----------------------
       public HunchbackSeverity HunchbackState { get; set; } // 驼背状态

        // 3. 头部倾斜度分析---------------------
        public float? HeadTiltAngle { get; set; } // 头部中轴线与垂直线的夹角 (度)
        public HeadTiltSeverity HeadTiltState { get; set; } // 头部倾斜状态

        public HeadOrientationHorizontal HeadYawDirection { get; set; }// = HeadOrientationHorizontal.Unknown;
        public HeadOrientationVertical HeadPitchDirection { get; set; }// = HeadOrientationVertical.Unknown;

        //5 . 综合评估
        public OverallPosture OverallPostureStatus { get; set; } //0"姿势标准", "轻微不良", "严重不良，请调整"
        //public List<String> DetectedIssues { get; private set; } // 存储所有检测到的问题描述

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
            sb.Append("Head Yaw orientation:" + HeadYawDirection + "\n");
            sb.Append("Head Pitch orientation:" + HeadPitchDirection + "\n");
            sb.Append("OverallPostureStatus: " + OverallPostureStatus + "\n");
            //sb.Append("HeadTiltState: " + HeadTiltState + "\n");
            //sb.Append("HeadTiltState: " + HeadTiltState + "\n");

            sb.Append("TimeStamp" + Timestamp + "\n");
            sb.Append("Formated TimeStamp" + Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "\n");
            sb.Append('}');
        
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
