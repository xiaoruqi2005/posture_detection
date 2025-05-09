namespace PostureChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "ÃÂÃ¨ºÏ≤‚÷˙ ÷";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form2 f2 = null;
            if(f2 == null || f2.IsDisposed)
            {
                f2 = new Form2(this);
                f2.ShowDialog();
            }
            else
            {
                f2.Activate();
            }
            
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form3 f3 = null;
            if (f3 == null || f3.IsDisposed)
            {
                f3 = new Form3(this);
                f3.ShowDialog();
            }
            else
            {
                f3.Activate();
            }

        }
    }
}
