using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class AssignmentSubmission
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? AssignmentId { get; set; }
        public Guid? AssignmentGroupId { get; set; }
        public string StudentId { get; set; }
        public string TeacherId { get; set; }
        public int? Grade { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string SubmissionFile { get; set; }
        public string Comment { get; set; }
    }

    public class AssignmentSubmissionViewModels
    {
        public Guid? Id { get; set; }
        public Guid? AssignmentId { get; set; }
        public Guid? AssignmentGroupId { get; set; }
        [Display(Name = "Group Name")]
        public string GroupName { get; set; }
        public string AssignmentType { get; set; }
        [Display(Name = "Submission Status")]
        public string SubmittedFromDueDate { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        [Display(Name = "Group Members")]
        public List<string> StudentNameList { get; set; }
        public string TeacherId { get; set; }
        public int? Grade { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd , hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? SubmissionDate { get; set; }
        [Display(Name = "Submission Date")]
        public string strSubmissionDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd , hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? LastUpdate { get; set; }
        [Display(Name = "Last Update")]
        public string strLastUpdate { get; set; }
        public string SubmissionFile { get; set; }
        public string Status { get; set; }
        [Display(Name = "Instructor's Comment")]
        public string Comment { get; set; }
        public string AssignmentTitle { get; set; }
        public string CourseChannelName { get; set; }
        public CourseChannelTopNavViewModels TopNavViewModels { get; set; }
    }
}