namespace Camera
{
    // --- 数据结构 (与 Python 发送的 JSON 匹配) ---
    public class Landmark
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float visibility { get; set; }
    }

}
