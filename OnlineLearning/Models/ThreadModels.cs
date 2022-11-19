using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class Thread
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? CourseChannelId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? LastUpdate { get; set; }
    }

    public class ThreadViewModels
    {
        public Guid Id { get; set; }
        public Guid? CourseChannelId { get; set; }
        public string CourseChannelName { get; set; }
        public string UserId { get; set; }
        public string ProfilePic { get; set; }
        public string Name { get; set; }
        [Required]
        [Display(Name = "Topic Title")]
        public string ThreadTitle { get; set; }
        [Required]
        public string Content { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? LastUpdate { get; set; }
        [Display(Name ="Last Update")]
        public string strLastUpdate { get; set; }
        public int? PostCount { get; set; }
        public CourseChannelTopNavViewModels TopNavViewModels { get; set; }
        public PostViewModels PostViewModels { get; set; }
        public List<PostViewModels> PostList { get; set; }
        [Display(Name = "Discussion Topics Created by You")]
        public int? ThreadByLoginUser { get; set; }
        [Display(Name = "Total Discussion Topics")]
        public int? TotalThread { get; set; }
        [Display(Name = "Discussion Topics Created by Students")]
        public int? ThreadByStudent { get; set; }
        [Display(Name = "Discussion Topics Created by Instructors")]
        public int? ThreadByTeacher { get; set; }
    }
}