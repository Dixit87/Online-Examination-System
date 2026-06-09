namespace Online_Examination_System.ViewModels
{
    public class QuestionAnalyticsViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int TotalAttempts { get; set; }
        public int TotalCorrect { get; set; }
        public int TotalWrong { get; set; }
        public decimal DifficultyIndex => TotalAttempts > 0 ? ((decimal)TotalWrong / TotalAttempts) * 100 : 0;
    }
}
