using Analysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        //public void recieve()
        //{
        //    dataGridView1.Rows.Clear();//清空旧数据
        //    var result = Analysis.Posenalyzer.result;
        //    string sql = "insert into data_table (sa, ss, ea, es, hs, heads, times,ft) " +
        //        "values ('" + result.ShoulderTiltAngle + "', '" + result.ShoulderState + "', '" + result.EyeTiltAngle + "', '" + result.EyeState + "', '" + result.HunchbackState + "', '" + result.HeadTiltAngle + "', '" + result.Timestamp + "','" +result.Timestamp + "');";
        //    DataBase da = new DataBase();
        //    da.InsertData(sql);
        //}
        public void Table()
        {
            dataGridView1.Rows.Clear();//清空旧数据
            DataBase da = new DataBase();
            string sql = "select * from data_table;";
            string a0, a1, a2, a3, a4, a5, a6;
            IDataReader dc = da.read(sql);
            while (dc.Read())
            {
                a0 = dc[6].ToString();
                a1 = dc[0].ToString();
                a2 = dc[1].ToString();
                a3 = dc[2].ToString();
                a4 = dc[3].ToString();
                a5 = dc[4].ToString();
                a6 = dc[5].ToString();
                string[] table = { a0, a1, a2, a3, a4, a5, a6 };
                dataGridView1.Rows.Add(table);
            }
            dc.Close();
            da.Dataclose();
        }

        private void Data_Load(object sender, EventArgs e)
        {
            Table();
        }
    }
}
