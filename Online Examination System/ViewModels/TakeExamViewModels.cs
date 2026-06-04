using System.Collections.Generic;

namespace Online_Examination_System.ViewModels
{
    public class TakeExamViewModel
    {
        public int ExamId { get; set; }
        public int StudentExamId { get; set; }
        public string ExamTitle { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class SaveAnswerRequest
    {
        public int StudentExamId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }

    public class ExamQuestionPayload
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int SavedOptionId { get; set; }
        public List<ExamOptionPayload> Options { get; set; } = new List<ExamOptionPayload>();
    }

    public class ExamOptionPayload
    {
        public int OptionId { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; }
    }
}
