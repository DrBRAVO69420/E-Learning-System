using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class CourseChannel
    {
        [Key]
        public Guid Id { get; set; }
        public string CreatorUserId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ThemeId { get; set; }
    }

    public class CourseChannelViewModel
    {
        public Guid Id { get; set; }
        public string CreatorUserId { get; set; }
        [Display(Name = "Instructor Name")]
        public string CreatorName { get; set; }
        public DateTime? CreatedOn { get; set; }
        [Required]
        [Display(Name="Course Channel")]
        public string Name { get; set; }
        public string FormattedName { get; set; }
        [Required]
        public string Description { get; set; }
        public string FormattedDescription { get; set; }
        public Guid? ThemeId { get; set; }
        public string ImageSource { get; set; }
        public int? StudentCount { get; set; }
        public List<CourseChannelViewModel> CoursesList { get; set; }
        public List<ChapterViewModels> ChapterList { get; set; }               
        public string ChapterName { get; set; }
        public Guid? SelectedChapterId { get; set; }
        public Guid CourseChannelId { get; set; } //used in /coursechannel/manage
        public CourseChannelTopNavViewModels TopNavViewModels { get; set; }
        public int? AssignmentCount { get; set; }
        public int? QuizCount { get; set; }
        public int? ChapterCount { get; set; }
        public int? MyEnrollCount { get; set; }
        public int? TotalCount { get; set; }
        public List<string> StudentList { get; set; }
        public Announcement LastestAnnouncement { get; set; }

    }

    public class EnrollLinkViewModel
    {
        public Guid? CourseChannelId { get; set; }
        public string CourseChannelName { get; set; }
    }


}