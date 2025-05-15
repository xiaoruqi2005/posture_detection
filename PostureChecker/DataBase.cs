using Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

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
        public void InsertData(string sql)//插入
        {
            //Application.Run(new Data());
            //Console.WriteLine("--- 开始进行测试  ---");
            //Posenalyzer ana = new Posenalyzer();
            //ana.StartAsync().Wait();

            MySqlCommand cmd = command(sql);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
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

//}
