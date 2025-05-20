namespace PostureChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            //InitializeComponent();
            this.Text = "��̬�������";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("����һ����ʾ������", "����", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // ��ȷ��/ȡ����ť���ɸ����û�ѡ��ִ�в�����
            DialogResult result = MessageBox.Show("ȷ��Ҫִ�д˲�����", "ȷ��", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                // �û�����ˡ�ȷ����
                Console.WriteLine("�û�ȷ�ϲ���");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
