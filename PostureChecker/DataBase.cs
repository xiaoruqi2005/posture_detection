using Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;
using static Common.Constants;

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
        public void InsertAnalysisResult(AnalysisResult result)
        {
            try
            {

                if (result.ShoulderState == TiltSeverity.Unknown ||
                    result.EyeState == TiltSeverity.Unknown ||
                    result.HunchbackState == HunchbackSeverity.Unknown ||
                    result.HeadTiltState == HeadTiltSeverity.Unknown ||
                    result.HeadYawDirection == HeadOrientationHorizontal.Unknown ||
                    result.HeadPitchDirection == HeadOrientationVertical.Unknown ||
                    result.OverallPostureStatus == OverallPosture.Unknown)
                {
                    throw new InvalidOperationException("存在未检测到数据的字段(Unknown状态)，拒绝写入数据库");
                }

                using (MySqlConnection connection = connect())
                {
                    string query = @"
                INSERT INTO data_table (
                    sa, ss, ea, es,
                    hs, heada, heads,
                    hyo, hpo, overall
                ) VALUES (
                    @ShoulderTiltAngle, @ShoulderState, @EyeTiltAngle, '@EyeState',
                    '@HunchbackState', @HeadTiltAngle, '@HeadTiltState',
                    '@HeadYawDirection', '@HeadPitchDirection', '@OverallPostureStatus'
                );";

                    MySqlCommand command = new MySqlCommand(query, connection);

                    // 添加参数
                    command.Parameters.AddWithValue("@ShoulderTiltAngle",result.ShoulderTiltAngle.HasValue ? (object)result.ShoulderTiltAngle.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@EyeTiltAngle",result.EyeTiltAngle.HasValue ? (object)result.EyeTiltAngle.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@HeadTiltAngle",result.HeadTiltAngle.HasValue ? (object)result.HeadTiltAngle.Value : DBNull.Value);

                    // 枚举类型转VARCHAR
                    command.Parameters.AddWithValue("@ShoulderState", result.ShoulderState.ToString());
                    command.Parameters.AddWithValue("@EyeState", result.EyeState.ToString());
                    command.Parameters.AddWithValue("@HunchbackState", result.HunchbackState.ToString());
                    command.Parameters.AddWithValue("@HeadTiltState", result.HeadTiltState.ToString());
                    command.Parameters.AddWithValue("@HeadYawDirection", result.HeadYawDirection.ToString());
                    command.Parameters.AddWithValue("@HeadPitchDirection", result.HeadPitchDirection.ToString());
                    command.Parameters.AddWithValue("@OverallPostureStatus", result.OverallPostureStatus.ToString());

                    // DateTime转TIMESTAMP
                    //command.Parameters.AddWithValue("@Timestamp", result.Timestamp);
                    //command.Parameters.AddWithValue("@Timestamp", result.Timestamp);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new Exception("数据库写入失败，影响行数为0");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                // 专门处理数据校验异常
                MessageBox.Show($"数据校验失败：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                throw; // 向上抛出异常供调用方处理
            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据库操作异常：{ex.Message}\n\n堆栈跟踪：{ex.StackTrace}",
                    "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // 向上抛出异常
            }
            finally
            {
                Dataclose(); // 确保连接关闭
            }
        }
    }
}
//}
