using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class DbConnection
    {
    }

    public class DefaultDBContext: DbContext
    {
        public DefaultDBContext()
            : base("DefaultConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        //used by asp.net identity
        public DbSet<AspNetUsers> AspNetUsers { get; set; }
        public DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public DbSet<AspNetRoles> AspNetRoles { get; set; }
        public DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        //portal's other table
        public DbSet<Setting> Settings { get; set; }
        public DbSet<CourseChannel> CourseChannels { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<ThemeCategory> ThemeCategories { get; set; }
        public DbSet<StudentCourseChannel> StudentCourseChannels { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<FileMaterial> FileMaterials { get; set; }
        public DbSet<TextMaterial> TextMaterials { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<AssignmentGroup> AssignmentGroups { get; set; }
        public DbSet<StudentAssignmentGroup> StudentAssignmentGroups { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
        public DbSet<StudentQuizAnswer> StudentQuizAnswers { get; set; }
        public DbSet<StudentQuiz> StudentQuizzes { get; set; }
        public DbSet<Thread> Threads { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<ChatChannel> ChatChannels { get; set; }
        public DbSet<Chat> Chats { get; set; }

    }
}