using System.ComponentModel.DataAnnotations;

namespace Online_Examination_System.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } // Joined from Roles table
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
