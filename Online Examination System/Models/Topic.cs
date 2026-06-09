using System;
using System.Collections.Generic;

namespace Online_Examination_System.Models
{
    public class Topic
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int ChapterId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? DurationMin { get; set; }
        public string TopicType { get; set; } // General, Accordion, Multi Slider
        public string TopicFileType { get; set; }
        public string TopicFilePath { get; set; }
        public int TopicPosition { get; set; }
        public bool Status { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Extended fields for list views
        public string SessionTitle { get; set; }
        public string ChapterTitle { get; set; }

        // Navigation Property
        public List<TopicDetail> TopicDetails { get; set; } = new List<TopicDetail>();
    }
}
