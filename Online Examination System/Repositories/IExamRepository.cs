using System.Collections.Generic;
using System.Threading.Tasks;
using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public interface IExamRepository
    {
        Task<IEnumerable<Exam>> GetAllAsync();
        Task<Exam> GetByIdAsync(int id);
        Task<int> UpsertExamAsync(Exam exam);
        Task UpsertExamQuestionsAsync(int examId, List<int> questionIds);
        Task ToggleStatusAsync(int id);
        Task<IEnumerable<Online_Examination_System.ViewModels.AdminExamResultItemViewModel>> GetResultsAsync(int examId);
    }
}
