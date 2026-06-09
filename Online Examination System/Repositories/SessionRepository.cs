using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Online_Examination_System.Data;
using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SessionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Session>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Session>(
                "sp_SessionsGetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Session> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await connection.QuerySingleOrDefaultAsync<Session>(
                "sp_SessionGetById",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpsertSessionAsync(Session session)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", session.Id);
            parameters.Add("@Title", session.Title);
            parameters.Add("@Description", session.Description);
            parameters.Add("@Status", session.Status);
            parameters.Add("@UserId", session.CreatedBy);

            return await connection.ExecuteAsync(
                "sp_SessionUpsert",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task ToggleStatusAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(
                "sp_SessionToggleStatus",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
