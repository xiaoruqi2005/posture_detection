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
    public partial class Form2 : Form
    {
        Form1 f1;
        //public Form1 f2;
        public Form2(Form1 form1)
        {
            InitializeComponent();
            f1 = form1;
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
            f1.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            f1.Show();
        }
    }
}
