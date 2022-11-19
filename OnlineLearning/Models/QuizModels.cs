using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class Quiz
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? CourseChannelId { get; set; }
        public string QuizTitle { get; set; }
        public string Instruction { get; set; }
        public bool? RandomizeQuestion { get; set; }
        public DateTime? DueDateTime { get; set; }
        public string CreatorId { get; set; }
        public DateTime? LastUpdate { get; set; }
        public bool? IsEnabled { get; set; }
    }

    public class QuizViewModels
    {
        public Guid Id { get; set; }
        public Guid? CourseChannelId { get; set; }
        public string CourseChannelName { get; set; }
        public StudentQuizViewModel StudentQuizViewModel { get; set; }
        [Required]
        [Display(Name = "Quiz")]
        public string QuizTitle { get; set; }
        [Required]
        public string Instruction { get; set; }
        [Display(Name = "Randomize Question")]
        public bool? RandomizeQuestion { get; set; }
        [Display(Name = "Due Date & Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? DueDateTime { get; set; }
        [Display(Name = "Due Date & Time")]
        public string strDueDateTime { get; set; }
        public string CreatorId { get; set; }
        [Display(Name = "Created By")]
        public string CreatorName { get; set; }
        [Display(Name = "Last Update")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? LastUpdate { get; set; }
        [Display(Name = "Last Update")]
        public string strLastUpdate { get; set; }
        public CourseChannelTopNavViewModels TopNavViewModels { get; set; }
        [Display(Name = "Enabled")]
        public bool? IsEnabled { get; set; }
        [Display(Name = "Time Remaining")]
        public string TimeRemaining { get; set; }
        public int? TotalQuestion { get; set; }
        public List<QuizQuestionViewModels> QuestionList { get; set; }
        public QuizQuestionViewModels AvailableQuestion { get; set; } //a question which current user haven't answered yet
        public int? CorrectCount { get; set; }
        public int? WrongCount { get; set; }
        public string CorrectWrong { get; set; }
        public bool? Answered { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? AnsweredDateTime { get; set; }
        [Display(Name ="Answered On")]
        public string strAnsweredDateTime { get; set; }
        public bool? HasQuestion { get; set; }
        [Display(Name ="All Questions have Answer")]
        public bool? AllQuestionsHaveAnswer { get; set; }
        public int? QuestionsHaveAnswers { get; set; }
        public bool? alreadyDue { get; set; }

    }
}

