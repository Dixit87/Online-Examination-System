namespace Online_Examination_System.ViewModels
{
    public class StudentExamListViewModel
    {
        public int ExamId { get; set; }
        public int? StudentExamId { get; set; }
        public string ExamTitle { get; set; }
        public int TotalQuestions { get; set; }
        public int Attempted { get; set; }
        public int RightAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public string Status { get; set; }
        public decimal ResultPercentage { get; set; }
        public System.DateTime? StartDateTime { get; set; }
        public System.DateTime? EndDateTime { get; set; }
    }
}
