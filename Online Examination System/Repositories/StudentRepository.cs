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
    public class StudentRepository : IStudentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public StudentRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<User>> GetAllStudentsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<User>(
                "sp_StudentGetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<StudentDetailsViewModel> GetStudentDetailsAsync(int studentId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", studentId);

            var student = await connection.QuerySingleOrDefaultAsync<User>(
                "sp_StudentGetDetails",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (student == null) return null;

            var examHistory = await connection.QueryAsync<StudentExamHistoryViewModel>(
                "sp_StudentExamsGetByStudentId",
                new { StudentId = studentId },
                commandType: CommandType.StoredProcedure);

            return new StudentDetailsViewModel
            {
                Student = student,
                ExamHistory = examHistory.ToList()
            };
        }

        public async Task<StudentDashboardViewModel> GetDashboardStatsAsync(int studentId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);

            var result = await connection.QuerySingleOrDefaultAsync<StudentDashboardViewModel>(
                "sp_StudentGetDashboardStats",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result ?? new StudentDashboardViewModel();
        }

        public async Task<IEnumerable<StudentExamListViewModel>> GetStudentExamListAsync(int studentId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);

            return await connection.QueryAsync<StudentExamListViewModel>(
                "sp_StudentGetExamList",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> InitializeExamAsync(int examId, int studentId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ExamId", examId);
            parameters.Add("@StudentId", studentId);

            return await connection.ExecuteScalarAsync<int>(
                "sp_StudentExamInitialize",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ExamQuestionPayload>> GetExamPayloadAsync(int studentExamId, int examId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@StudentExamId", studentExamId);
            parameters.Add("@ExamId", examId);

            using var multi = await connection.QueryMultipleAsync("sp_StudentGetExamData", parameters, commandType: CommandType.StoredProcedure);

            var questions = (await multi.ReadAsync<ExamQuestionPayload>()).ToList();
            var options = (await multi.ReadAsync<ExamOptionPayload>()).ToList();

            foreach (var q in questions)
            {
                q.Options = options.Where(o => o.QuestionId == q.QuestionId).ToList();
            }

            return questions;
        }

        public async Task SaveAnswerAsync(int studentExamId, int questionId, int selectedOptionId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@StudentExamId", studentExamId);
            parameters.Add("@QuestionId", questionId);
            parameters.Add("@SelectedOptionId", selectedOptionId);

            await connection.ExecuteAsync(
                "sp_StudentSaveAnswer",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task SubmitExamAsync(int studentExamId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@StudentExamId", studentExamId);

            await connection.ExecuteAsync(
                "sp_StudentSubmitExam",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<ExamResultViewModel> GetExamResultAsync(int studentExamId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@StudentExamId", studentExamId);

            return await connection.QuerySingleOrDefaultAsync<ExamResultViewModel>(
                "sp_StudentGetExamResult",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
