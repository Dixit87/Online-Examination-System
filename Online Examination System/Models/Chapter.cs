using System;

namespace Online_Examination_System.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        // Extended field for list views
        public string SessionTitle { get; set; }
    }
}
