using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Online_Examination_System.ViewModels
{
    public class ChapterUpsertViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Session is required")]
        [Display(Name = "Session")]
        public int SessionId { get; set; }

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public IEnumerable<SelectListItem> SessionList { get; set; }

        [Required(ErrorMessage = "Chapter Title is required")]
        [Display(Name = "Chapter Title")]
        public string Title { get; set; }

        public string Description { get; set; }

        public bool Status { get; set; } = true;
    }
}
