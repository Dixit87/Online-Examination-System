using System.ComponentModel.DataAnnotations;

namespace Online_Examination_System.ViewModels
{
    public class StudentProfileViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [StringLength(15)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid phone number")]
        public string Contact { get; set; }

        public string? Address { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
