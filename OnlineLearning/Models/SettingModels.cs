using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class Setting
    {
        [Key]
        public Guid? Id { get; set; }
        public string UserId { get; set; }
        public string Background { get; set; }

    }

    public class SettingViewModels
    {
        public Guid? Id { get; set; }
        [Display(Name ="Theme")]
        public string Background { get; set; }
    }
}