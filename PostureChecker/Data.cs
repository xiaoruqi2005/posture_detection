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
    public partial class Data : Form
    {
        //Menu f1;
        public Data()
        {
            InitializeComponent();
            //f1 = form1;
        }

        private void button1_Click(object sender, EventArgs e)//按照时间段进行查询
        {
            this.Close();
            //f1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //f1.Show();
            Menu menu = new Menu();
            this.Hide();
            menu.ShowDialog();
            this.Show();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
