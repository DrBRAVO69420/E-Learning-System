using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class TeacherHomeDashboardViewModels
    {
        [Display(Name = "Total Course Channels")]
        public int? TotalCourseChannels { get; set; }
        [Display(Name = "Total Assignments")]
        public int? TotalAssignment { get; set; }
        [Display(Name = "Total Quizzes")]
        public int? TotalQuizzes { get; set; }
        [Display(Name = "Total Discussion Topics")]
        public int? TotalDiscussionTopics { get; set; }
        [Display(Name = "Assignments Submitted by Students")]
        public int? ReceivedAssignments { get; set; }
        [Display(Name = "Graded Assignments")]
        public int? GradedAssignments { get; set; }
        [Display(Name = "Non-Graded Assignments")]
        public int? NonGradedAssignments { get; set; }
        public List<QuizViewModels> UpcomingExpiredQuizzes { get; set; }
        public List<AssignmentViewModels> UpcomingExpiredAssignments { get; set; }
        public List<AssignmentSubmissionViewModels> OldestAssignmentsHaventGraded { get; set; }
        public List<ThreadViewModels> NewDiscussionTopics { get; set; }
    }

    public class StudentHomeDashboardViewModels
    {
        [Display(Name = "My Enrollment")]
        public int? TotalCourseChannels { get; set; }
        [Display(Name = "Assignments in My Enrollment")]
        public int? TotalAssignment { get; set; }
        [Display(Name = "Quizzes in My Enrollment")]
        public int? TotalQuizzes { get; set; }
        [Display(Name = "My Enrollment")]
        public List<QuizViewModels> UpcomingExpiredQuizzes { get; set; }
        public List<AssignmentViewModels> UpcomingExpiredAssignments { get; set; }
        [Display(Name = "Highest Grade Assignment")]
        public AssignmentSubmissionViewModels HighestGradeAssignment { get; set; }
        [Display(Name = "Lowest Grade Assignment")]
        public AssignmentSubmissionViewModels LowestGradeAssignment { get; set; }
    }
}