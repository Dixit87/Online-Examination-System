using System.Collections.Generic;
using System.Threading.Tasks;
using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public interface ISessionRepository
    {
        Task<IEnumerable<Session>> GetAllAsync();
        Task<Session> GetByIdAsync(int id);
        Task<int> UpsertSessionAsync(Session session);
        Task ToggleStatusAsync(int id);
    }
}
