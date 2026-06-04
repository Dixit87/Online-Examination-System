using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Online_Examination_System.Models;

namespace Online_Examination_System.ViewModels
{
    public class QuestionUpsertViewModel
    {
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Question Text is required")]
        public string QuestionText { get; set; }

        public bool IsActive { get; set; } = true;

        // Represents the dynamically added options from the view
        public List<OptionViewModel> Options { get; set; } = new List<OptionViewModel>();
        
        // Which option is correct (the index from the view's radio buttons)
        public int CorrectOptionIndex { get; set; }
    }

    public class OptionViewModel
    {
        public int OptionId { get; set; }
        
        [Required(ErrorMessage = "Option Text is required")]
        public string OptionText { get; set; }
        
        public bool IsCorrect { get; set; }
    }
}
