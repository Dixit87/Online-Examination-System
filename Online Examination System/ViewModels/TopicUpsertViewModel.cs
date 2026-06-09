using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Online_Examination_System.ViewModels
{
    public class TopicUpsertViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Session is required")]
        [Display(Name = "Session")]
        public int SessionId { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public IEnumerable<SelectListItem> SessionList { get; set; }

        [Required(ErrorMessage = "Chapter is required")]
        [Display(Name = "Chapter")]
        public int ChapterId { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public IEnumerable<SelectListItem> ChapterList { get; set; }

        [Required(ErrorMessage = "Topic Title is required")]
        [Display(Name = "Topic Title")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Display(Name = "Duration (Minutes)")]
        public int? DurationMin { get; set; }

        [Required(ErrorMessage = "Topic Type is required")]
        [Display(Name = "Topic Type")]
        public string TopicType { get; set; } // General, Accordion, Multi Slider

        [Display(Name = "Topic File Type")]
        public string TopicFileType { get; set; } // E.g., PDF, Image, YouTube

        [Display(Name = "Topic File Path / URL")]
        public string TopicFilePath { get; set; }

        [Display(Name = "Position")]
        public int TopicPosition { get; set; } = 1;

        public bool Status { get; set; } = true;

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public List<TopicDetailViewModel> TopicDetails { get; set; } = new List<TopicDetailViewModel>();
    }

    public class TopicDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? DurationMin { get; set; }
        public string FileType { get; set; }
        public string FilePath { get; set; }
        public int Position { get; set; }
        public bool Status { get; set; } = true;
        public bool IsDeleted { get; set; } = false; // Used for soft deletion in the UI
    }
}
