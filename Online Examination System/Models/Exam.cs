using System;
using System.Collections.Generic;

namespace Online_Examination_System.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public int ExamTypeId { get; set; }
        public string ExamTypeName { get; set; } // Used for displaying in the list
        public string ExamTitle { get; set; }
        public int NoOfQuestions { get; set; }
        public decimal PerQuestionMark { get; set; }
        public decimal TotalMark { get; set; }
        public decimal PassingMark { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        
        // Mapped Questions
        public List<int> SelectedQuestionIds { get; set; } = new List<int>();
    }
}
