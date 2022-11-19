using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class Theme
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; }
        //public string ThemeFileName { get; set; }
        public string ImageSource { get; set; }
        public string ImageName { get; set; }
    }

    public class ThemeViewModel
    {
        public Guid? Id { get; set; }
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CourseChannelName { get; set; }
        public Guid? CourseChannelId { get; set; }
        public string ImageSource { get; set; }
        public string ImageName { get; set; }
        public List <ThemeViewModel> ThemeList { get; set; }
        public List <ThemeCategory> CategoryList { get; set; }
    }
}