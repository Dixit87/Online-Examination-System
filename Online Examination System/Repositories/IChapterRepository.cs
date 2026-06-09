using System.Collections.Generic;
using System.Threading.Tasks;
using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public interface IChapterRepository
    {
        Task<IEnumerable<Chapter>> GetAllAsync();
        Task<Chapter> GetByIdAsync(int id);
        Task<int> UpsertChapterAsync(Chapter chapter);
        Task ToggleStatusAsync(int id);
    }
}
