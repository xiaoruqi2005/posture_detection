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
    public partial class Form3 : Form
    {
        Form1 f1;
        public Form3(Form1 form1)
        {
            InitializeComponent();
            f1 = form1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            f1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            f1.Show();
        }
    }
}
