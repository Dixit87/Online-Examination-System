using Dapper;
using Online_Examination_System.Data;
using Online_Examination_System.Models;
using System.Data;

namespace Online_Examination_System.Repositories
{
    public class ExamTypeRepository : IExamTypeRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ExamTypeRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ExamType>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<ExamType>(
                "sp_ExamTypesGetAll", 
                commandType: CommandType.StoredProcedure);
        }

        public async Task<ExamType> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ExamTypeId", id);
            
            return await connection.QueryFirstOrDefaultAsync<ExamType>(
                "sp_ExamTypeGetById", 
                parameters, 
                commandType: CommandType.StoredProcedure);
        }

        public async Task UpsertAsync(ExamType examType, int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ExamTypeId", examType.ExamTypeId);
            parameters.Add("@Title", examType.Title);
            parameters.Add("@IsActive", examType.IsActive);
            parameters.Add("@UserId", userId);

            await connection.ExecuteAsync(
                "sp_ExamTypeUpsert", 
                parameters, 
                commandType: CommandType.StoredProcedure);
        }

        public async Task ToggleStatusAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var query = "UPDATE ExamTypes SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE ExamTypeId = @Id";
            await connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
