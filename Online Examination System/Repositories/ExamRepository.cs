using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Online_Examination_System.Data;
using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public class ExamRepository : IExamRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ExamRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Exam>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Exam>(
                "sp_ExamGetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Exam> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ExamId", id);

            using var multi = await connection.QueryMultipleAsync(
                "sp_ExamGetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            var exam = await multi.ReadSingleOrDefaultAsync<Exam>();
            if (exam != null)
            {
                var questionIds = await multi.ReadAsync<int>();
                exam.SelectedQuestionIds = questionIds.ToList();
            }

            return exam;
        }

        public async Task<int> UpsertExamAsync(Exam exam)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ExamId", exam.ExamId);
            parameters.Add("@ExamTypeId", exam.ExamTypeId);
            parameters.Add("@ExamTitle", exam.ExamTitle);
            parameters.Add("@NoOfQuestions", exam.NoOfQuestions);
            parameters.Add("@PerQuestionMark", exam.PerQuestionMark);
            parameters.Add("@TotalMark", exam.TotalMark);
            parameters.Add("@PassingMark", exam.PassingMark);
            parameters.Add("@NegativeMark", exam.NegativeMark);
            parameters.Add("@StartDateTime", exam.StartDateTime);
            parameters.Add("@EndDateTime", exam.EndDateTime);
            parameters.Add("@IsActive", exam.IsActive);
            parameters.Add("@UserId", exam.CreatedBy);

            return await connection.ExecuteScalarAsync<int>(
                "sp_ExamUpsert",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task UpsertExamQuestionsAsync(int examId, List<int> questionIds)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Delete old mappings
                await connection.ExecuteAsync(
                    "DELETE FROM ExamQuestions WHERE ExamId = @ExamId",
                    new { ExamId = examId },
                    transaction: transaction);

                // Insert new mappings
                if (questionIds != null && questionIds.Any())
                {
                    foreach (var qId in questionIds)
                    {
                        await connection.ExecuteAsync(
                            "sp_ExamQuestionsUpsert",
                            new { ExamId = examId, QuestionId = qId },
                            transaction: transaction,
                            commandType: CommandType.StoredProcedure);
                    }
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
                "sp_ExamToggleStatus",
                new { ExamId = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Online_Examination_System.ViewModels.AdminExamResultItemViewModel>> GetResultsAsync(int examId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ExamId", examId);

            return await connection.QueryAsync<Online_Examination_System.ViewModels.AdminExamResultItemViewModel>(
                "sp_ExamGetResults",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
