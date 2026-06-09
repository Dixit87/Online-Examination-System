using System.Collections.Generic;
using System.Threading.Tasks;
using Online_Examination_System.Models;
using Online_Examination_System.ViewModels;

namespace Online_Examination_System.Repositories
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetAllAsync();
        Task<Question> GetByIdAsync(int id);
        Task<int> UpsertQuestionAsync(Question question);
        Task UpsertOptionsAsync(int questionId, List<Option> options);
        Task ToggleStatusAsync(int id);
        Task<IEnumerable<QuestionAnalyticsViewModel>> GetQuestionAnalyticsAsync();
    }
}
