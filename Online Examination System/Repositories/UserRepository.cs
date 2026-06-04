using Dapper;
using Online_Examination_System.Data;
using Online_Examination_System.Models;
using System.Data;

namespace Online_Examination_System.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User> GetUserByLoginAsync(string usernameOrEmail)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UsernameOrEmail", usernameOrEmail);

            return await connection.QueryFirstOrDefaultAsync<User>(
                "sp_UserLogin",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> RegisterStudentAsync(User student)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Name", student.Name);
            parameters.Add("@Email", student.Email);
            parameters.Add("@Contact", student.Contact);
            parameters.Add("@Username", student.Username);
            parameters.Add("@PasswordHash", student.PasswordHash);
            parameters.Add("@Address", student.Address);
            parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_StudentRegister",
                parameters,
                commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@UserId");
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<User>(
                "sp_UsersGetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await connection.QueryFirstOrDefaultAsync<User>(
                "sp_UserGetById",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateUserAsync(User user, int updatedBy)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", user.UserId);
            parameters.Add("@Name", user.Name);
            parameters.Add("@Email", user.Email);
            parameters.Add("@Contact", user.Contact);
            parameters.Add("@Username", user.Username);
            parameters.Add("@Address", user.Address);
            parameters.Add("@IsActive", user.IsActive);
            parameters.Add("@UpdatedBy", updatedBy);

            await connection.ExecuteAsync(
                "sp_UserUpdate",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task ToggleStatusAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            await connection.ExecuteAsync(
                "sp_UserToggleStatus",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> AddUserAsync(User user, string roleName, int createdBy)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Get RoleId
            var roleId = await connection.ExecuteScalarAsync<int>("SELECT RoleId FROM Roles WHERE RoleName = @RoleName", new { RoleName = roleName });

            // Check uniqueness
            var existing = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Users WHERE Username = @Username OR Email = @Email", new { Username = user.Username, Email = user.Email });
            if (existing > 0)
            {
                throw new Exception("Username or Email already exists.");
            }

            var query = @"INSERT INTO Users (RoleId, Name, Email, Contact, Username, PasswordHash, Address, IsActive, CreatedBy) 
                          VALUES (@RoleId, @Name, @Email, @Contact, @Username, @PasswordHash, @Address, @IsActive, @CreatedBy);
                          SELECT CAST(SCOPE_IDENTITY() as int);";
                          
            var newId = await connection.QuerySingleAsync<int>(query, new 
            { 
                RoleId = roleId, 
                Name = user.Name, 
                Email = user.Email, 
                Contact = user.Contact, 
                Username = user.Username, 
                PasswordHash = user.PasswordHash, 
                Address = user.Address, 
                IsActive = user.IsActive,
                CreatedBy = createdBy
            });
            
            return newId;
        }

        public async Task<ViewModels.DashboardViewModel> GetDashboardStatsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var stats = await connection.QuerySingleOrDefaultAsync<ViewModels.DashboardViewModel>(
                "sp_GetDashboardStats",
                commandType: CommandType.StoredProcedure);
                
            return stats ?? new ViewModels.DashboardViewModel();
        }

        public async Task UpdateStudentProfileAsync(int userId, ViewModels.StudentProfileViewModel model, string passwordHash)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@Name", model.Name);
            parameters.Add("@Email", model.Email);
            parameters.Add("@Contact", model.Contact);
            parameters.Add("@Address", model.Address);
            parameters.Add("@PasswordHash", passwordHash);

            await connection.ExecuteAsync(
                "sp_StudentProfileUpdate",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
