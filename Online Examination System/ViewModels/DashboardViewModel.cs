namespace Online_Examination_System.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalExamTypes { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalExams { get; set; }
        public int CompletedExams { get; set; }
        public int PassedStudents { get; set; }
        public int FailedStudents { get; set; }

        public decimal PassPercentage => CompletedExams == 0 ? 0 : Math.Round(((decimal)PassedStudents / CompletedExams) * 100, 2);
        public decimal FailPercentage => CompletedExams == 0 ? 0 : Math.Round(((decimal)FailedStudents / CompletedExams) * 100, 2);
    }
}
