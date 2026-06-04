using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Online_Examination_System.ViewModels
{
    public class ExamUpsertViewModel
    {
        public int ExamId { get; set; }

        [Required(ErrorMessage = "Exam Type is required")]
        [Display(Name = "Exam Type")]
        public int ExamTypeId { get; set; }
        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public IEnumerable<SelectListItem> ExamTypeList { get; set; }

        [Required(ErrorMessage = "Exam Title is required")]
        [Display(Name = "Exam Title")]
        public string ExamTitle { get; set; }

        [Required(ErrorMessage = "No of Questions is required")]
        [Range(1, 1000, ErrorMessage = "Must be between 1 and 1000")]
        [Display(Name = "No Of Questions")]
        public int NoOfQuestions { get; set; }

        [Required(ErrorMessage = "Per Question Mark is required")]
        [Range(0.1, 100, ErrorMessage = "Must be between 0.1 and 100")]
        [Display(Name = "Per Question Mark")]
        public decimal PerQuestionMark { get; set; }

        [Display(Name = "Total Mark")]
        public decimal TotalMark { get; set; }

        [Required(ErrorMessage = "Passing Mark is required")]
        [Display(Name = "Passing Mark")]
        public decimal PassingMark { get; set; }

        public bool IsActive { get; set; } = true;

        // For the checkboxes
        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public List<QuestionSelectionViewModel> AvailableQuestions { get; set; } = new List<QuestionSelectionViewModel>();
    }

    public class QuestionSelectionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public bool IsSelected { get; set; }
    }
}
