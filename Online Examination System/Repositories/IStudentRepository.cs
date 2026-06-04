using System.Collections.Generic;
using System.Threading.Tasks;
using Online_Examination_System.Models;
using Online_Examination_System.ViewModels;

namespace Online_Examination_System.Repositories
{
    public interface IStudentRepository
    {
        Task<IEnumerable<User>> GetAllStudentsAsync();
        Task<StudentDetailsViewModel> GetStudentDetailsAsync(int studentId);
        Task<StudentDashboardViewModel> GetDashboardStatsAsync(int studentId);
        Task<IEnumerable<StudentExamListViewModel>> GetStudentExamListAsync(int studentId);
        
        Task<int> InitializeExamAsync(int examId, int studentId);
        Task<IEnumerable<ExamQuestionPayload>> GetExamPayloadAsync(int studentExamId, int examId);
        Task SaveAnswerAsync(int studentExamId, int questionId, int selectedOptionId);
        Task SubmitExamAsync(int studentExamId);
        Task<ExamResultViewModel> GetExamResultAsync(int studentExamId);
    }
}
