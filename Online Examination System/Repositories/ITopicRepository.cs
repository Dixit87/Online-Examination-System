using System.Collections.Generic;
using System.Threading.Tasks;
using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public interface ITopicRepository
    {
        Task<IEnumerable<Topic>> GetAllAsync();
        Task<Topic> GetByIdAsync(int id);
        Task<int> UpsertTopicAsync(Topic topic);
        Task ToggleStatusAsync(int id);
        
        Task<IEnumerable<TopicDetail>> GetTopicDetailsByTopicIdAsync(int topicId);
        Task UpsertTopicDetailsAsync(int topicId, List<TopicDetail> details);
    }
}
