using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearning.Models
{ 
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username / Email")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        public string Id { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Gender { get; set; } 

        [Required]
        [Display(Name = "Intake Year")]
        public int? IntakeYear { get; set; }

        [Required]
        [Display(Name = "Registered As")]
        public char? UserRole { get; set; }

        [Display(Name = "Instructor ID / Student ID")]
        public string Code { get; set; }
    }

    public class RegisterAdminViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Gender { get; set; } //'M' or 'F'

    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string RoleId { get; set; }
        public string Role { get; set; }
        [Required]
        public string Username { get; set; }
        [Display(Name = "Student ID")]
        public string Code { get; set; }
        [Display(Name = "Profile Picture")]
        public string ProfilePic { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Gender { get; set; }
        [Display(Name = "Intake Year")]
        public int? IntakeYear { get; set; }
        public string UserStatus { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime? RegisteredOn { get; set; }
        [Display(Name = "Registered On")]
        public string strRegisteredOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? AllUserCount { get; set; }
        public int? ApprovedUserCount { get; set; }
        public int? NewUserCount { get; set; }
        public int? DisabledLoginUserCount { get; set; }
        [Display(Name = "Who Approve")]
        public string ApprovedAdmin { get; set; }
        [Display(Name = "Who Update")]
        public string UpdateAdmin { get; set; }
        [Display(Name = "Update Detail")]
        public string UpdateDetail { get; set; }
        [Display(Name = "Updated On")]
        public DateTime? UpdateOn { get; set; }
        public List<CourseChannelViewModel> CourseChannels { get; set; }
        public string Biography { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "User Role")]
        public string UserRole { get; set; }
        [Display(Name = "Total Admin")]
        public int? TotalAdmin { get; set; }
        [Display(Name = "Total Instructor")]
        public int? TotalTeacher { get; set; }
        [Display(Name = "Total Student")]
        public int? TotalStudent { get; set; }
        public bool? OwnUserProfile { get; set; }
    }

    public class EmailTemplate
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
