using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using Analysis;
using WebAccess.DTO;
using  Common;
using System.Data.Common;
namespace WebAccess.Respoository
{
    public class PostureAnalysisRepository
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PostureAnalysisRepository> _logger;

        public PostureAnalysisRepository(IConfiguration configuration,ILogger<PostureAnalysisRepository> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connectionString = configuration.GetConnectionString("MyDatabaseConnection") // "MyDatabaseConnection" 是 appsettings.json 中的键名
                                ?? throw new InvalidOperationException("Connection string 'MyDatabaseConnection' not found.");
            _logger = logger;
        }

        public async Task AddAnalysisdataAsync(PostureData data)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"
                INSERT INTO posture_results
                (Timestamp, ShoulderTiltAngle, ShoulderState, EyeTiltAngle, EyeState,
                 HunchbackState, HeadTiltAngle, HeadTiltState, HeadYawDirection,
                 HeadPitchDirection, OverallPostureStatus)
                VALUES
                (@Timestamp, @ShoulderTiltAngle, @ShoulderState, @EyeTiltAngle, @EyeState,
                 @HunchbackState, @HeadTiltAngle, @HeadTiltState, @HeadYawDirection,
                 @HeadPitchDirection, @OverallPostureStatus);";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Timestamp", data.Timestamp);
                    command.Parameters.AddWithValue("@ShoulderTiltAngle", (object)data.ShoulderTiltAngle ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ShoulderState", data.ShoulderState.ToString()); // 存储字符串
                    command.Parameters.AddWithValue("@EyeTiltAngle", (object)data.EyeTiltAngle ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EyeState", data.EyeState.ToString()); // 存储字符串
                    command.Parameters.AddWithValue("@HunchbackState", data.HunchbackState.ToString()); // 存储字符串
                    command.Parameters.AddWithValue("@HeadTiltAngle", (object)data.HeadTiltAngle ?? DBNull.Value);
                    command.Parameters.AddWithValue("@HeadTiltState", data.HeadTiltState.ToString()); // 存储字符串
                    command.Parameters.AddWithValue("@HeadYawDirection", data.HeadYawDirection.ToString()); // 存储字符串
                    command.Parameters.AddWithValue("@HeadPitchDirection", data.HeadPitchDirection.ToString()); // 存储字符串
                    command.Parameters.AddWithValue("@OverallPostureStatus", data.OverallPostureStatus.ToString()); // 存储字符串

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
            public async Task<List<PostureData>> GetAllValidAnalysisdataAsync() // 方法名修改以反映其功能
            {
                List<PostureData> dataList = new List<PostureData>(); // 用于存储多条记录

                // 构建 SQL 查询语句
                string sql = @"
            SELECT *
            FROM posture_results
            WHERE
                ShoulderTiltAngle IS NOT NULL AND
                EyeTiltAngle IS NOT NULL AND
                HeadTiltAngle IS NOT NULL AND
                EyeState <> 'Unknown' AND
                ShoulderState <> 'Unknown' AND
                HunchbackState <> 'Unknown' AND
                HeadTiltState <> 'Unknown' AND
                HeadYawDirection <> 'Unknown' AND
                HeadPitchDirection <> 'Unknown' AND
                OverallPostureStatus <> 'Unknown' 
            ORDER BY Timestamp DESC; -- 可选：按时间戳降序排序，获取最新的有效数据在前
        ";
                // 注意：如果 OverallPostureStatus 枚举没有 'Unknown' 成员，或者你不希望基于它过滤，
                // 请移除 OverallPostureStatus <> 'Unknown' 这一行。

                using (var connection = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        await connection.OpenAsync();
                        _logger.LogInformation("Successfully connected to MySQL database.");

                        using (var command = new MySqlCommand(sql, connection))
                        {
                            _logger.LogInformation("Executing SQL query: {SqlQuery}", sql);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                // 循环读取所有符合条件的行
                                while (await reader.ReadAsync())
                                {
                                    // 将当前行映射到 PostureData 对象并添加到列表
                                    dataList.Add(MapRowToPostureData(reader));
                                }
                                _logger.LogInformation("Query executed successfully. Found {Count} valid records.", dataList.Count);
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError(ex, "A MySQL error occurred while fetching valid analysis data.");
                        // 可以选择抛出异常，或者返回一个空列表/null，取决于你的错误处理策略
                        throw; // 或者 return new List<PostureData>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An unexpected error occurred while fetching valid analysis data.");
                        throw; // 或者 return new List<PostureData>();
                    }
                    // finally 块中不需要显式关闭连接，因为 using 语句会自动处理
                }
                return dataList;
            }

            // 辅助方法：将 DbDataReader 的当前行映射到 PostureData 对象
            // 你需要根据你的 PostureData 实体类和数据库列来具体实现这个方法
            private PostureData MapRowToPostureData(DbDataReader reader)
            {
                var postureData = new PostureData();

                postureData.Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")); // 假设列名为 "Timestamp"

                // 处理可空浮点数
                int shoulderTiltAngleOrdinal = reader.GetOrdinal("ShoulderTiltAngle");
                postureData.ShoulderTiltAngle = reader.IsDBNull(shoulderTiltAngleOrdinal) ? (float?)null : reader.GetFloat(shoulderTiltAngleOrdinal);

                int eyeTiltAngleOrdinal = reader.GetOrdinal("EyeTiltAngle");
                postureData.EyeTiltAngle = reader.IsDBNull(eyeTiltAngleOrdinal) ? (float?)null : reader.GetFloat(eyeTiltAngleOrdinal);

                int headTiltAngleOrdinal = reader.GetOrdinal("HeadTiltAngle");
                postureData.HeadTiltAngle = reader.IsDBNull(headTiltAngleOrdinal) ? (float?)null : reader.GetFloat(headTiltAngleOrdinal);

                // 处理枚举类型 (假设存储为字符串)
                // 你需要确保你的枚举类型和 Constants.cs 中的枚举类型是正确的
                // 并且 Enum.TryParse 对于数据库中存储的字符串是有效的
                Enum.TryParse<PostureData.TiltSeverity>(reader.GetString(reader.GetOrdinal("EyeState")), true, out var eyeState); // true for case-insensitive
                postureData.EyeState = eyeState;

                Enum.TryParse<Constants.TiltSeverity>(reader.GetString(reader.GetOrdinal("ShoulderState")), true, out var shoulderState);
                postureData.ShoulderState = shoulderState;

                Enum.TryParse<Constants.HunchbackSeverity>(reader.GetString(reader.GetOrdinal("HunchbackState")), true, out var hunchbackState);
                postureData.HunchbackState = hunchbackState;

                Enum.TryParse<Constants.HeadTiltSeverity>(reader.GetString(reader.GetOrdinal("HeadTiltState")), true, out var headTiltState);
                postureData.HeadTiltState = headTiltState;

                Enum.TryParse<Constants.HeadOrientationHorizontal>(reader.GetString(reader.GetOrdinal("HeadYawDirection")), true, out var headYawDirection);
                postureData.HeadYawDirection = headYawDirection;

                Enum.TryParse<Constants.HeadOrientationVertical>(reader.GetString(reader.GetOrdinal("HeadPitchDirection")), true, out var headPitchDirection);
                postureData.HeadPitchDirection = headPitchDirection;

                Enum.TryParse<Constants.OverallPosture>(reader.GetString(reader.GetOrdinal("OverallPostureStatus")), true, out var overallPostureStatus);
                postureData.OverallPostureStatus = overallPostureStatus;

                // 如果还有其他字段，也在这里映射

                return postureData;
            }
        }
    }
        