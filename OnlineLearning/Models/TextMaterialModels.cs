using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class TextMaterial
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? ChapterId { get; set; }
        public string Detail { get; set; }
        public string CreatorId { get; set; }
        public DateTime? LastUpdate { get; set; }
    }

    public class TextMaterialViewModels
    {
        public Guid Id { get; set; }
        public Guid? ChapterId { get; set; }
        public string Detail { get; set; }
        public string CreatorId { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? LastUpdate { get; set; }
        [Display(Name ="Last Update")]
        public string strLastUpdate { get; set; }
    }
}