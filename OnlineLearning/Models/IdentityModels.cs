using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OnlineLearning.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        //additional variables
        public string Name { get; set; }
        public string Code { get; set; }
        public string Gender { get; set; } 
        public int? IntakeYear { get; set; }
        public string ProfilePicName { get; set; }
        public string ProfilePicPath { get; set; }
        public string ApproveAdminId { get; set; } //admin who approve the user registration
        public string UpdateAdminId { get; set; } //admin who disable user login / reset password / delete user
        public string UpdateDetail { get; set; } //disable login / reset password / delete user
        public string UserStatus { get; set; } //(New/Approved/Disabled) [New] = haven't approved, [Approved] = alredy approved by admin, [Disabled] = disabled login due to leave school / quit job
        public DateTime? RegisterOn { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public DateTime? UpdateOn { get; set; }
        public string Biography { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

    }


}