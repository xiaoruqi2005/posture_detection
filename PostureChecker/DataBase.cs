using Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;

namespace PostureChecker
{
    public class DataBase
    {
        MySqlConnection sc;
        public MySqlConnection connect()//数据库连接
        {
            string conStr = "Server=localhost;Database=MyData;User=root;Password=@Ab123456YJC;";
            sc = new MySqlConnection(conStr);
            sc.Open();
            return sc;
        }
        public MySqlCommand command(string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, connect());
            return cmd;
        }
        public int Execute(string sql)//更新
        {
            return command(sql).ExecuteNonQuery();
        }
        public MySqlDataReader read(string sql)//读取
        {
            return command(sql).ExecuteReader();
        }
        public void Dataclose()//关闭
        {
            sc.Close();
        }
        public void InsertData(AnalysisResult result)//插入
        {
            //Application.Run(new Data());
            //Console.WriteLine("--- 开始进行测试  ---");
            //Posenalyzer ana = new Posenalyzer();
            //ana.StartAsync().Wait();
            DataBase db = new DataBase();
            string sql1 = "use mydata;";
            MySqlCommand cmd = command(sql1);
            sc.Open();
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
        public List<string[]> Table()
        {
            //dataGridView1.Rows.Clear();//清空旧数据
            List<string[]> result = new List<string[]>();
            DataBase da = new DataBase();
            string sql = "select * from data_table;";
            string a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10;
            IDataReader dc = da.read(sql);
            while (dc.Read())
            {
                a0 = dc[6].ToString();
                a1 = dc[0].ToString();
                a2 = dc[1].ToString();
                a3 = dc[2].ToString();
                a4 = dc[3].ToString();
                a5 = dc[4].ToString();
                a6 = dc[7].ToString();
                a7 = dc[11].ToString();
                a8 = dc[8].ToString();
                a9 = dc[9].ToString();
                a10 = dc[10].ToString();
                //a11 = dc[12].ToString();
                string[] table = { a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10 };//, a11 };
                //dataGridView1.Rows.Add(table);
                result.Add(table);
            }
            dc.Close();
            da.Dataclose();
            return result;
        }
        /// <summary>
        /// 【方法】将datagridview中的数据写入到数据库的table中。
        /// </summary>
        /// <param name="dbSource">服务器名称，如localhost</param>
        /// <param name="dbUid">用户名，如root</param>
        /// <param name="dbPwd">密码</param>
        /// <param name="dbName">已有数据库名称</param>
        /// <param name="tbName">已有数据表名称</param>
        /// <param name="dataGrid">datagridview的名称</param>
        //static void InsertDataToTable(string dbSource, string dbUid, string dbPwd, string dbName, string tbName, DataGridView dataGrid)
        //{
        //    //创建连接字符串con
        //    MySqlConnection con = new MySqlConnection("Data Source=" + dbSource + ";Persist Security Info=yes;UserId=" + dbUid + "; PWD=" + dbPwd + ";");

        //    // 打开数据库
        //    string tablecmd = "USE " + dbName + ";";
        //    MySqlCommand cmd = new MySqlCommand(tablecmd, con);
        //    con.Open();
        //    int res = cmd.ExecuteNonQuery();

        //    for (int i = 0; i < dataGrid.RowCount; i++)
        //    {
        //        string data = "INSERT INTO " + tbName + " VALUES (" +
        //            (i + 1) + "," +
        //            "'" + dataGrid[1, i].Value + "'" + "," +
        //            "'" + dataGrid[2, i].Value + "'" + "," +
        //            dataGrid[3, i].Value + "," +
        //            dataGrid[4, i].Value + ");";
        //        MySqlCommand cmd1 = new MySqlCommand(data, con);
        //        int res1 = cmd1.ExecuteNonQuery();
        //    }
        //    con.Close();
        //}

        //public void MySqlOp()
        //{
        //    string conStr = "Server=localhost;Database=MyData;User=root;Password=@Ab123456YJC;";
        //    MySqlConnection conn = new MySqlConnection(conStr);
        //    MySqlCommand cmd = null;
        //    try
        //    {
        //        conn.Open();
        //        cmd = conn.CreateCommand();
        //        cmd.CommandText = "SELECT * FROM user_table";
        //        MySqlDataReader reader = cmd.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            Console.WriteLine(reader["id"] + " " + reader["name"]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    finally
        //    {
        //        if (cmd != null)
        //            cmd.Dispose();
        //        if (conn != null)
        //            conn.Close();
        //    }
        //}
    }
}
//}
