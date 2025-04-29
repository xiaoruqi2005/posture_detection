using System.Text;

namespace Camera
{
    // --- 数据结构 (与 Python 发送的 JSON 匹配) ---
    //public class Landmark
    //{
    //    public float x { get; set; }
    //    public float y { get; set; }
    //    public float z { get; set; }
    //    public float visibility { get; set; }
    //}
    public class Landmark    //关键点信息
    {
        public float x { get; set; }  //x坐标
        public float y { get; set; }  //y坐标
        public float z { get; set; }  //z坐标
        public float? visibility { get; set; }  //可见性
        public float? presence { get; set; } //存在性
    }
    

    public class HolisticData  //姿态信息
    {
        public List<Landmark> pose {  get; set; }
        public List<Landmark> face {  get; set; }
        public List<Landmark> left_hand { get; set; }
        public List<Landmark> right_hand { get; set; }

        //判断是否含有对应的数据
        public bool HasPoseData => pose != null && pose.Count > 0;
        public bool HasFaceData => face != null && face.Count > 0;
        public bool HasLeftHandData => left_hand != null && left_hand.Count > 0;
        public bool HasRightHandData => right_hand != null && right_hand.Count > 0;

        public bool HasAnyLandmarks()
        {
            return (pose != null && pose.Count > 0) ||
                   (face != null && face.Count > 0) ||
                   (left_hand != null && left_hand.Count > 0) ||
                   (right_hand != null && right_hand.Count > 0);
        }
        //打印信息用于测试
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Pose Data:");
            foreach (var point in pose)
             sb.Append($"point({point.x},{point.y}) visible{point.visibility}  ");
            sb.AppendLine();

            sb.AppendLine("Face Data:");
            foreach  (var point in face)
                sb.Append($"point({point.x},{point.y}) visible{point.visibility}  ");
            sb.AppendLine();

            sb.AppendLine("Left_hand Data:");
            foreach (var point in left_hand)
                sb.Append($"point({point.x},{point.y}) visible{point.visibility}  ");
            sb.AppendLine();

            sb.AppendLine("Right_hand Data:");
            foreach (var point in right_hand)
                sb.Append($"point({point.x},{point.y}) visible{point.visibility}  ");
            sb.AppendLine();

            return sb.ToString();
        }
    }

}
