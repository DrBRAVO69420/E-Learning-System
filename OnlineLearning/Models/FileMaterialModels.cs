using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class FileMaterial
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? ChapterId { get; set; }
        public string FileName { get; set; }
        public string CreatorId { get; set; }
        public DateTime? LastUpdate { get; set; }
    }

    public class FileMaterialViewModels
    {
        public Guid Id { get; set; }
        public Guid? ChapterId { get; set; }
        public string FileName { get; set; }
        public string CreatorId { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}