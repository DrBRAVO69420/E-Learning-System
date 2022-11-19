using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class AspNetUserRoles
    {        
        [Key, Column(Order = 1)]
        [MaxLength(128)]
        public string UserId { get; set; }
        [Key, Column(Order = 2)]
        [MaxLength(128)]
        public string RoleId { get; set; }      
    }
}