using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Online_Examination_System.Data;
using Online_Examination_System.Models;
using Online_Examination_System.ViewModels;

namespace Online_Examination_System.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public QuestionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Question>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Question>(
                "sp_QuestionGetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Question> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@QuestionId", id);

            var question = await connection.QuerySingleOrDefaultAsync<Question>(
                "sp_QuestionGetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (question != null)
            {
                var options = await connection.QueryAsync<Option>(
                    "sp_OptionsGetByQuestionId",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                    
                question.Options = options.ToList();
            }

            return question;
        }

        public async Task<int> UpsertQuestionAsync(Question question)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@QuestionId", question.QuestionId);
            parameters.Add("@QuestionText", question.QuestionText);
            parameters.Add("@IsActive", question.IsActive);
            parameters.Add("@UserId", question.CreatedBy);

            return await connection.ExecuteScalarAsync<int>(
                "sp_QuestionUpsert",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task UpsertOptionsAsync(int questionId, List<Option> options)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Delete existing options
                await connection.ExecuteAsync(
                    "sp_OptionsDeleteByQuestionId",
                    new { QuestionId = questionId },
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);

                // Insert new options
                foreach (var option in options)
                {
                    await connection.ExecuteAsync(
                        "sp_OptionInsert",
                        new { QuestionId = questionId, OptionText = option.OptionText, IsCorrect = option.IsCorrect },
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

        public async Task ToggleStatusAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "sp_QuestionToggleStatus",
                new { QuestionId = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<QuestionAnalyticsViewModel>> GetQuestionAnalyticsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<QuestionAnalyticsViewModel>(
                "sp_AdminGetQuestionAnalytics",
                commandType: CommandType.StoredProcedure);
        }
    }
}
