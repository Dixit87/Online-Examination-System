using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Online_Examination_System.Data;
using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public class TopicRepository : ITopicRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TopicRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Topic>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Topic>(
                "sp_TopicsGetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Topic> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await connection.QuerySingleOrDefaultAsync<Topic>(
                "sp_TopicGetById",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpsertTopicAsync(Topic topic)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", topic.Id);
            parameters.Add("@SessionId", topic.SessionId);
            parameters.Add("@ChapterId", topic.ChapterId);
            parameters.Add("@Title", topic.Title);
            parameters.Add("@Description", topic.Description);
            parameters.Add("@DurationMin", topic.DurationMin);
            parameters.Add("@TopicType", topic.TopicType);
            parameters.Add("@TopicFileType", topic.TopicFileType);
            parameters.Add("@TopicFilePath", topic.TopicFilePath);
            parameters.Add("@TopicPosition", topic.TopicPosition);
            parameters.Add("@Status", topic.Status);
            parameters.Add("@UserId", topic.CreatedBy);
            
            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_TopicUpsert",
                parameters,
                commandType: CommandType.StoredProcedure);
                
            return parameters.Get<int>("@NewId");
        }

        public async Task ToggleStatusAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(
                "sp_TopicToggleStatus",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
        
        public async Task<IEnumerable<TopicDetail>> GetTopicDetailsByTopicIdAsync(int topicId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@TopicId", topicId);

            return await connection.QueryAsync<TopicDetail>(
                "sp_TopicDetailsGetByTopicId",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task UpsertTopicDetailsAsync(int topicId, List<TopicDetail> details)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Delete existing details for this topic to replace them
                await connection.ExecuteAsync(
                    "sp_TopicDetailsDeleteByTopicId",
                    new { TopicId = topicId },
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);

                // Insert new details
                foreach (var detail in details)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", 0);
                    parameters.Add("@SessionId", detail.SessionId);
                    parameters.Add("@ChapterId", detail.ChapterId);
                    parameters.Add("@TopicId", topicId);
                    parameters.Add("@Title", detail.Title);
                    parameters.Add("@Description", detail.Description);
                    parameters.Add("@DurationMin", detail.DurationMin);
                    parameters.Add("@FileType", detail.FileType);
                    parameters.Add("@FilePath", detail.FilePath);
                    parameters.Add("@Position", detail.Position);
                    parameters.Add("@Status", detail.Status);
                    parameters.Add("@UserId", detail.CreatedBy);
                    
                    await connection.ExecuteAsync(
                        "sp_TopicDetailsUpsert",
                        parameters,
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
