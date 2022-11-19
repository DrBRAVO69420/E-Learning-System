using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineLearning.Models
{
    public class Announcement
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? CourseChannelId { get; set; }
        public string Title { get; set; }
        [AllowHtml]
        public string Detail { get; set; }
        public string Type { get; set; } //Course or Campus (Course Announcement is made by Instructor & only can be seen by the enrolled students, Campus Announcement is made by Admin & can be seen by all users)
        public string CreatorId { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class AnnouncementViewModel
    {
        public Guid? Id { get; set; }
        [Required]
        [Display(Name ="Title")]
        public string TitleText { get; set; }
        [Required]
        [AllowHtml]
        public string Details { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string Type { get; set; }
        public string CreatorProfilePic { get; set; }
        public Guid? CourseChannelId { get; set; }
        [Display(Name = "Last Updated On")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd, hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? LastUpdate { get; set; }
        [Display(Name = "Last Updated On")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd, hh:mm tt}", ApplyFormatInEditMode = true)]
        public string strLastUpdate { get; set; }
        public string UpdatedBy { get; set; }
        [Display(Name = "Updated By")]
        public string AdminName { get; set; } //admin who update
        public CourseChannelTopNavViewModels TopNavViewModels { get; set; }
    }
}