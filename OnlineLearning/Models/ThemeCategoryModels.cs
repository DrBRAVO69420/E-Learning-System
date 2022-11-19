using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class ThemeCategory
    {
        [Key]
        public Guid CateogyId { get; set; }
        public string CategoryName { get; set; }
    }
}