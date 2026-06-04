using System;
using System.Collections.Generic;
using Online_Examination_System.Models;

namespace Online_Examination_System.ViewModels
{
    public class StudentDetailsViewModel
    {
        public User Student { get; set; }
        public List<StudentExamHistoryViewModel> ExamHistory { get; set; } = new List<StudentExamHistoryViewModel>();
    }

    public class StudentExamHistoryViewModel
    {
        public int StudentExamId { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public string Status { get; set; }
        public decimal Score { get; set; }
        public decimal TotalMark { get; set; }
        public decimal ResultPercentage { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}
