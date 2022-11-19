using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class Assignment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? CourseChannelId { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Instruction { get; set; }
        public string Type { get; set; } //Group or Individual
        public int? MemberMaxCount { get; set; }
        public DateTime? DueDate { get; set; }
        public string TeacherId { get; set; }
        public DateTime? LastUpdate { get; set; }

    }

    public class AssignmentViewModels
    {
        public Guid Id { get; set; }
        public Guid? CourseChannelId { get; set; }
        [Display(Name ="Attachments")]
        public string FileName { get; set; }
        [Required]
        [Display(Name = "Title")]
        public string AssignmentTitle { get; set; }
        [Required]
        public string Instruction { get; set; }
        [Display(Name = "Due Date & Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd , hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? DueDate { get; set; }
        [Display(Name = "Due Date & Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd , hh:mm tt}", ApplyFormatInEditMode = true)]
        public string strDueDate { get; set; }
        public string TimeRemaining { get; set; }
        public string TeacherId { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? LastUpdate { get; set; }
        [Display(Name = "Course Channel")]
        public string CourseChannelName { get; set; }
        public CourseChannelTopNavViewModels TopNavViewModels { get; set; }
        [Required]
        public string Type { get; set; }
        public string TypeName { get; set; }
        public int? MemberMaxCount { get; set; }
        public List<AssignmentViewModels> AssignmentList { get; set; }
        public AssignmentGroupViewModels AssignmentGroupViewModels { get; set; }
        public AssignmentSubmissionViewModels AssignmentSubmissionViewModels { get; set; }
        public bool? AlreadyDue { get; set; }
        public string SubmissionStatus { get; set; }

    }
}