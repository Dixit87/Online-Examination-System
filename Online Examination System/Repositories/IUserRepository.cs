using Online_Examination_System.Models;

namespace Online_Examination_System.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByLoginAsync(string usernameOrEmail);
        Task<int> RegisterStudentAsync(User student);
        Task<User> GetUserByEmailAsync(string email);
        Task UpdatePasswordAsync(int userId, string newPasswordHash);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetByIdAsync(int userId);
        Task UpdateUserAsync(User user, int updatedBy);
        Task ToggleStatusAsync(int userId);
        Task<int> AddUserAsync(User user, string roleName, int createdBy);
        Task<ViewModels.DashboardViewModel> GetDashboardStatsAsync();
        Task UpdateStudentProfileAsync(int userId, ViewModels.StudentProfileViewModel model, string passwordHash);
    }
}
