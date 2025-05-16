using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PostureChecker
{
    public partial class cameraVideo : Form
    {
      //Menu f1;
        //public Form1 f2;
        public cameraVideo()
        {
            InitializeComponent();
            //f1 = form1;
            //f1 = new Form1();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            //f1.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //f1.Show();
        }

        private void 主菜单ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Menu menu = new Menu();
            this.Hide();
            menu.ShowDialog();
            this.Show();
        }

        private void 查询数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Data data = new Data();
            this.Hide();
            data.ShowDialog();
            this.Show();
        }
    }
}
