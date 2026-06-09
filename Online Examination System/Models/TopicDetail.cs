using System;

namespace Online_Examination_System.Models
{
    public class TopicDetail
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int ChapterId { get; set; }
        public int TopicId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? DurationMin { get; set; }
        public string FileType { get; set; }
        public string FilePath { get; set; }
        public int Position { get; set; }
        public bool Status { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
