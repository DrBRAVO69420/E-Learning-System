using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class Chapter
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? CourseChannelId { get; set; }
        public string Title { get; set; }
        public string CreatorId { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? CreatedOn { get; set; }

    }

    public class ChapterViewModels
    {
        public Guid Id { get; set; }
        public Guid? CourseChannelId { get; set; }
        public string CourseChannelName { get; set; }
        [Display(Name = "Chapter Title")]
        public string ChapterTitle { get; set; }
        public string CreatorId { get; set; }
        [Display(Name ="Last Update")]
        public DateTime? LastUpdate { get; set; }
        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; }
        public List<FileMaterial> FileList { get; set; }
        public List<TextMaterial> TextList { get; set; }
        public CourseChannelTopNavViewModels TopNavViewModels { get; set; }
    }
}