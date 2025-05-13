using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

//namespace posturechecker
//{
//    public class database
//    {
//        public void mysqlop()
//        {
//            string constr = "server=localhost;database=mydata;user=root;password=@ab123456yjc;";
//            mysqlconnection conn = new mysqlconnection(constr);
//            mysqlcommand cmd = null;
//            try
//            {
//                conn.open();
//                cmd = conn.createcommand();
//                cmd.commandtext = "select * from user_table";
//                mysqldatareader reader = cmd.executereader();
//                while (reader.read())
//                {
//                    console.writeline(reader["id"] + " " + reader["name"]);
//                }
//            }
//            catch (exception ex)
//            {
//                console.writeline(ex.message);
//            }
//            finally
//            {
//                if (cmd != null)
//                    cmd.dispose();
//                if (conn != null)
//                    conn.close();
//            }
//        }
//    }

//}
