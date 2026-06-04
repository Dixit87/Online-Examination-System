using System.ComponentModel.DataAnnotations;

namespace Online_Examination_System.Models
{
    public class ExamType
    {
        public int ExamTypeId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; }

        public bool IsActive { get; set; }
        
        public DateTime CreatedDate { get; set; }
    }
}
