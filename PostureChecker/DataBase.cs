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
        public void MySqlOp()
        {
            string conStr = "Server=localhost;Database=MyData;User=root;Password=@Ab123456YJC;";
            MySqlConnection conn = new MySqlConnection(conStr);
            MySqlCommand cmd = null;
            try
            {
                conn.Open();
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM user_table";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine(reader["id"] + " " + reader["name"]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                if (conn != null)
                    conn.Close();
            }
        }
    }

}
