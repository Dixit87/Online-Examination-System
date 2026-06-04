using System.ComponentModel.DataAnnotations;

namespace Online_Examination_System.ViewModels
{
    public class UserAddViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Contact is required")]
        public string Contact { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string RoleName { get; set; }

        public string Address { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
