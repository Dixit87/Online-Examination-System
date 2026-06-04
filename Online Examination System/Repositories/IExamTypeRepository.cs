using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public interface IExamTypeRepository
    {
        Task<IEnumerable<ExamType>> GetAllAsync();
        Task<ExamType> GetByIdAsync(int id);
        Task UpsertAsync(ExamType examType, int userId);
        Task ToggleStatusAsync(int id);
    }
}
