using System.Text.Json.Serialization;
using Common;
using static Common.Constants;
namespace WebAccess.DTO
{
    public class PostureData
    {

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


        }

        public enum HunchbackSeverity  //驼背状态的 枚举类型
        {
            Unknown,        // 无法判断
            NoHunchback,    // 未检测到驼背
            SlightHunchback, // 轻微驼背
            ObviousHunchback // 明显驼背 

        }
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
        }    // 头部左右倾的枚举类型

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
      
        public float? ShoulderTiltAngle { get; set; }
        
        // 肩膀连线与水平线的夹角 (度)
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Constants.TiltSeverity ShoulderState { get; set; }

        //驼背状态分析 
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Constants.HunchbackSeverity HunchbackState { get; set; } // 驼背状态

        // 3. 头部倾斜分析
        public float? HeadTiltAngle { get; set; } // 头部中轴线与垂直线的夹角 (度)
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Constants.HeadTiltSeverity HeadTiltState { get; set; } // 头部倾斜状态

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HeadOrientationHorizontal HeadYawDirection { get; set; }// = HeadOrientationHorizontal.Unknown;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HeadOrientationVertical HeadPitchDirection { get; set; }// = HeadOrientationVertical.Unknown;

        //5 . 综合评估
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OverallPosture OverallPostureStatus { get; set; } //0"姿势标准", "轻微不良", "严重不良，请调整"

    }

    public class LLMQueryRequest
    {
        public string Prompt { get; set; }
    }

}
