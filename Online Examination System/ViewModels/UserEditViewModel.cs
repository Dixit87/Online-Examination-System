using System.ComponentModel.DataAnnotations;

namespace Online_Examination_System.ViewModels
{
    public class UserEditViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        public string Contact { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        public string Address { get; set; }
        
        public bool IsActive { get; set; }
    }
}
