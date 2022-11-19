using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class QuizQuestion
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? QuizId { get; set; }
        public string Question { get; set; }
        public string AttachedImage { get; set; }
        public DateTime? LastUpdate { get; set; }
    }

    public class QuizQuestionViewModels
    {
        public Guid Id { get; set; }
        public Guid? QuizId { get; set; }
        public string QuizName { get; set; }
        public int? TotalQuestion { get; set; }
        public int? CurrentQuestionNumber { get; set; }
        public string CourseChannelName { get; set; }
        public string UserFullName { get; set; }
        public bool? IsLastQuestion { get; set; }
        public QuizViewModels QuizDetail { get; set; }
        [Required]
        public string Question { get; set; }
        [Display(Name = "Image")]
        public string AttachedImage { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? LastUpdate { get; set; }
        [Display(Name ="Last Update")]
        public string strLastUpdate { get; set; }
        public List<QuizAnswer> AnswerList { get; set; }
        public Guid? StudentAnswerId { get; set; }
        public int? CorrectAnswer { get; set; }
        public CourseChannelTopNavViewModels TopNavViewModels { get; set; }

    }
}