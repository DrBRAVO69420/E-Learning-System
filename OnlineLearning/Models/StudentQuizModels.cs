using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class StudentQuiz
    {
        [Key]
        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public Guid? QuizId { get; set; }
        public DateTime? QuizStartedOn { get; set; }
    }

    public class StudentQuizViewModel
    {
        public Guid? Id { get; set; }
        public string StudentId { get; set; }
        public Guid? QuizId { get; set; }
        [Display(Name ="Started On")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? QuizStartedOn { get; set; }
        [Display(Name = "Started On")]
        public string strQuizStartedOn { get; set; }
        [Display(Name = "Student Name")]
        public string StudentName { get; set; }
        public string ProfilePic { get; set; }
        [Display(Name = "Answered Correctly")]
        public int? CorrectCount { get; set; }
        [Display(Name = "Answered Wrongly")]
        public int? WrongCount { get; set; }
    }
}