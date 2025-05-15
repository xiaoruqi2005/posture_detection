namespace PostureChecker
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
            this.Text = "Menu";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            cameraVideo f2 = null;
            if (f2 == null || f2.IsDisposed)
            {
                f2 = new cameraVideo();
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
            Data f3 = null;
            if (f3 == null || f3.IsDisposed)
            {
                f3 = new Data();
                f3.ShowDialog();
            }
            else
            {
                f3.Activate();
            }

        }

        private void Menu_Load(object sender, EventArgs e)
        {

        }
    }
}
