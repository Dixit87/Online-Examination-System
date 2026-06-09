using System;

namespace Online_Examination_System.ViewModels
{
    public class ExamResultViewModel
    {
        public string ExamTypeName { get; set; }
        public string ExamName { get; set; }
        public int TotalQuestions { get; set; }
        public decimal PerQuestionMark { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal PassingMarks { get; set; }
        public int TotalAttemptedQuestions { get; set; }
        public int TotalCorrectAnswers { get; set; }
        public int TotalWrongAnswers { get; set; }
        public int Rank { get; set; }
        public int TotalParticipants { get; set; }
        public decimal ResultPercentage { get; set; }
        public decimal Score { get; set; }
        public DateTime ExamStartDate { get; set; }
        public DateTime? ExamFinishDate { get; set; }

        public string PassStatus => Score >= PassingMarks ? "PASS" : "FAIL";
    }
}
