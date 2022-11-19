using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class AspNetUsers
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(256)]
        public string UserName { get; set; }
        [MaxLength(256)]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        //additional variables
        public string Name { get; set; }
        public string Code { get; set; }
        public string Gender { get; set; }
        public int? IntakeYear { get; set; }
        public string ProfilePicName { get; set; }
        public string ProfilePicPath { get; set; }
        [MaxLength(128)]
        public string ApproveAdminId { get; set; } //admin who approve the user registration
        [MaxLength(128)]
        public string UpdateAdminId { get; set; } //admin who disable user login / reset password / delete user
        public string UpdateDetail { get; set; } //disable login / reset password / delete user
        public string UserStatus { get; set; } //(New/Approved/Disabled) [New] = haven't approved, [Approved] = alredy approved by admin, [Disabled] = disabled login due to leave school / quit job
        public DateTime? RegisterOn { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public DateTime? UpdateOn { get; set; }
        public string Biography { get; set; }
    }
}



    