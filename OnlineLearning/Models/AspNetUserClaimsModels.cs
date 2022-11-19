using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class AspNetUserClaims
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }

    }
}
