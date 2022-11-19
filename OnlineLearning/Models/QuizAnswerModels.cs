using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class QuizAnswer
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? QuestionId { get; set; }
        public string AnswerText { get; set; }
        public bool? IsCorrect { get; set; }
        public DateTime? LastUpdate { get; set; }
    }

    public class QuizAnswerViewModels
    {
        public Guid Id { get; set; }
        public Guid? QuestionId { get; set; }
        public string AnswerText { get; set; }
        public bool? IsCorrect { get; set; }
        public DateTime? LastUpdate { get; set; }
    }


}
