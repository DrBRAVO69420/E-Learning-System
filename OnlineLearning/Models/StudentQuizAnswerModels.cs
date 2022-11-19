using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class StudentQuizAnswer
    {
        [Key]
        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public Guid? QuestionId { get; set; }
        public Guid? AnswerId { get; set; }
        public Guid? QuizId { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}