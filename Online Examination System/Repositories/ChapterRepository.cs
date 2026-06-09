using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Online_Examination_System.Data;
using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ChapterRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Chapter>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Chapter>(
                "sp_ChaptersGetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Chapter> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await connection.QuerySingleOrDefaultAsync<Chapter>(
                "sp_ChapterGetById",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpsertChapterAsync(Chapter chapter)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", chapter.Id);
            parameters.Add("@SessionId", chapter.SessionId);
            parameters.Add("@Title", chapter.Title);
            parameters.Add("@Description", chapter.Description);
            parameters.Add("@Status", chapter.Status);
            parameters.Add("@UserId", chapter.CreatedBy);

            return await connection.ExecuteAsync(
                "sp_ChapterUpsert",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task ToggleStatusAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(
                "sp_ChapterToggleStatus",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
