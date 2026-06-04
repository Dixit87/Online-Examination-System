using System;
using System.Collections.Generic;

namespace Online_Examination_System.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Custom field for the list view
        public int OptionsCount { get; set; }
        
        public List<Option> Options { get; set; } = new List<Option>();
    }
}
