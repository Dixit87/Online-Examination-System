using System;
using System.Collections.Generic;

namespace Online_Examination_System.ViewModels
{
    public class AdminExamResultViewModel
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public List<AdminExamResultItemViewModel> Results { get; set; } = new List<AdminExamResultItemViewModel>();
    }

    public class AdminExamResultItemViewModel
    {
        public int StudentExamId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public int TotalAttempted { get; set; }
        public int TotalRight { get; set; }
        public int TotalWrong { get; set; }
        public decimal Score { get; set; }
        public decimal ResultPercentage { get; set; }
        public DateTime CompletionDate { get; set; }
    }
}
