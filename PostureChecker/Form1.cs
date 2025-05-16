namespace PostureChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "体态检测助手";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("这是一个提示弹窗！", "标题", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 带确认/取消按钮（可根据用户选择执行操作）
            DialogResult result = MessageBox.Show("确定要执行此操作吗？", "确认", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                // 用户点击了“确定”
                Console.WriteLine("用户确认操作");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
