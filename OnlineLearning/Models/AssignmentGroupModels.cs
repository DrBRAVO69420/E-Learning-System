using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class AssignmentGroup
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? AssignmentId { get; set; }
        public string GroupName { get; set; }
        
    }

    public class AssignmentGroupViewModels
    {
        public Guid? Id { get; set; }
        [Display(Name ="Group Name")]
        public string GroupName { get; set; }
        public string ProfilePic { get; set; }
        public string StudentId { get; set; }

        [Display(Name = "All Groups for this Assignment")]
        public List<AspNetUsers> StudentList { get; set; }
        [Display(Name = "Group Members")]
        public List<string> StudentIdList { get; set; }
        [Display(Name = "Group Members")]
        public string StudentName { get; set; }
    }
}
