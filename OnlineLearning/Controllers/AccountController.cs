using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security;
using OnlineLearning.Models;

namespace OnlineLearning.Controllers
{
    [System.Web.Mvc.Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [CustomAuthorize(Roles = "Teacher, Student, Admin")]
        public new ActionResult Profile(string Id)
        {
            UserViewModel model = GetUserViewModel(Id);
            string loginUserId = User.Identity.GetUserId();
            return View(model);
        }

        [CustomAuthorize(Roles = "Teacher, Student, Admin")]
        public ActionResult EditProfile()
        {
            string loginUserId = User.Identity.GetUserId();
            UserViewModel model = GetUserViewModel(loginUserId);
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        [CustomAuthorize(Roles = "Teacher, Student, Admin")]
        public ActionResult EditProfile(UserViewModel model, IEnumerable<HttpPostedFileBase> attachments)
        {
            if (model != null)
            {
                if (ModelState.IsValid)
                {
                    string FileName = "";
                    AspNetUsers user = db.AspNetUsers.Where(a => a.Id == model.Id).FirstOrDefault();
                    user.Biography = model.Biography;
                    if (attachments != null)
                    {
                        foreach (var file in attachments)
                        {
                            if (file != null)
                            {
                                // get the file name
                                FileName = new FileInfo(file.FileName).Name;
                                string serverFilePath = HttpContext.Server.MapPath("~/UploadedFiles/") + FileName;
                                file.SaveAs(serverFilePath); //remember to set 'write' permission on UploadedFiles folder in the server 
                                user.ProfilePicName = FileName;
                                user.ProfilePicPath = serverFilePath;
                            }
                        }
                    }

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["UpdatedSuccess"] = "Updated Successfully";
                    return RedirectToAction("profile",new { Id = User.Identity.GetUserId()});
                }
            }
            return View(model);
        }

        public UserViewModel GetUserViewModel(string Id)
        {
            UserViewModel model = new UserViewModel();
            string loginUserId = User.Identity.GetUserId();
            AspNetUsers user = db.AspNetUsers.Where(a => a.Id == Id).FirstOrDefault();
            model.Id = user.Id;
            model.OwnUserProfile = user.Id == loginUserId ? true : false;
            model.Email = user.Email;
            model.Name = user.Name;
            model.ProfilePic = user.ProfilePicName;
            model.Gender = user.Gender;
            model.IntakeYear = user.IntakeYear;
            model.Username = user.UserName;
            model.Biography = user.Biography == null ? "" : user.Biography;
            model.IntakeYear = user.IntakeYear;
            model.Code = user.Code == null ? "" : user.Code;
            string roleId = db.AspNetUserRoles.Where(a => a.UserId == Id).Select(a => a.RoleId).DefaultIfEmpty("").FirstOrDefault();
            string roleName = db.AspNetRoles.Where(a => a.Id == roleId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
            model.Role = roleName;
            model.UserRole = roleName.Remove(1, roleName.Length-1);
            if (roleName.Equals("Student"))
            {
                model.CourseChannels = (from t1 in db.CourseChannels
                                        join t2 in db.StudentCourseChannels on t1.Id equals t2.CourseChannelId
                                        where t2.StudentId == Id
                                        select new CourseChannelViewModel
                                        {
                                            Id = t1.Id,
                                            Name = t1.Name,
                                            CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                                        }).OrderBy(o => o.Name).ToList();
            }
            if (roleName.Equals("Teacher"))
            {
                model.CourseChannels = (from t1 in db.CourseChannels
                                        where t1.CreatorUserId == Id
                                        select new CourseChannelViewModel
                                        {
                                            Id = t1.Id,
                                            Name = t1.Name,
                                            CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                                        }).OrderBy(o => o.Name).ToList();
            }
            return model;
        }

        public JsonResult GetUserStatusList()
        {
            List<string> result = new List<string>();
            result.Add("New");
            result.Add("Approved");
            result.Add("Disabled");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserRoleList()
        {
            List<string> result = new List<string>();
            result = db.AspNetRoles.Select(a => a.Name).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region --------------------------------------- Role ---------------------------------------       
        public JsonResult GetRoleList()
        {
            try
            {
                var result = (from t1 in db.AspNetRoles
                              select new
                              {
                                  Text = t1.Name,
                                  Value = t1.Id
                              }).OrderBy(o => o.Text).ToList(); //order by category title in asc order
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region --------------------------------------- Login ---------------------------------------
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (Request.IsAuthenticated && User.IsInRole("Admin"))
            {
                return RedirectToAction("manageuser", "account");
            }
            if (Request.IsAuthenticated && User.IsInRole("Teacher"))
            {
                //return RedirectToAction("list", "coursechannel");
                return RedirectToAction("teacher", "home");
            }
            if (Request.IsAuthenticated && User.IsInRole("Student"))
            {
                //return RedirectToAction("browseall", "coursechannel");
                return RedirectToAction("student", "home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            string userid = "";
            string userStatus = "";
            // if any of the user's input invalid, return the model to display error message
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // if username input contains @ sign, means that user use email to login
            if (model.UserName.Contains("@"))
            {
                // select the UserName of the user from AspNetUsers table and assign to model.UserName because instead of email, SignInManager use username to sign in 
                model.UserName = db.AspNetUsers.Where(a => a.Email == model.UserName).Select(a => a.UserName).DefaultIfEmpty("").FirstOrDefault();
                userid = db.AspNetUsers.Where(a => a.Email == model.UserName).Select(a => a.Id).DefaultIfEmpty("").FirstOrDefault();
                userStatus = db.AspNetUsers.Where(a => a.Email == model.UserName).Select(a => a.UserStatus).DefaultIfEmpty("").FirstOrDefault();
            }
            else
            {
                userid = db.AspNetUsers.Where(a => a.UserName == model.UserName).Select(a => a.Id).DefaultIfEmpty("").FirstOrDefault();
                userStatus = db.AspNetUsers.Where(a => a.UserName == model.UserName).Select(a => a.UserStatus).DefaultIfEmpty("").FirstOrDefault();
            }
            //if the user is not approved, show invalid login attempt
            if (!userid.Equals("") && !userStatus.Equals("Approved"))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
            else
            {
                var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

                switch (result)
                {
                    case SignInStatus.Success:
                        ApplicationUser user = UserManager.FindByName(model.UserName);
                        var userRole = await UserManager.GetRolesAsync(user.Id);

                        if (userRole.Contains("Admin"))
                        {
                            return RedirectToAction("manageuser", "account");
                        }
                        if (userRole.Contains("Teacher"))
                        {
                            return RedirectToAction("teacher", "home");
                        }
                        if (userRole.Contains("Student"))
                        {
                            return RedirectToAction("student", "home");
                        }
                        else
                        {
                            return View(model);
                        }
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
        }
        #endregion

        #region --------------------------------------- Register Admin ---------------------------------------    
        [AllowAnonymous]
        public ActionResult RegisterAdmin()
        {
            return View();
        }

        public bool InsertRole()
        {
            string roleid = db.AspNetRoles.Where(a => a.Name == "Admin").Select(a => a.Id).FirstOrDefault();
            if (string.IsNullOrEmpty(roleid))
            {
                AspNetRoles adminrole = new AspNetRoles();
                adminrole.Id = Guid.NewGuid().ToString();
                adminrole.Name = "Admin";

                AspNetRoles teacherrole = new AspNetRoles();
                teacherrole.Id = Guid.NewGuid().ToString();
                teacherrole.Name = "Teacher";

                AspNetRoles studentrole = new AspNetRoles();
                studentrole.Id = Guid.NewGuid().ToString();
                studentrole.Name = "Student";

                db.AspNetRoles.Add(adminrole);
                db.AspNetRoles.Add(teacherrole);
                db.AspNetRoles.Add(studentrole);

                db.SaveChanges();
            }
            return true;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterAdmin(RegisterAdminViewModel model)
        {
            if (model != null)
            {
                bool usernameExist = db.AspNetUsers.Where(a => a.UserName == model.Username).Any();
                bool emailExist = db.AspNetUsers.Where(a => a.Email == model.Email).Any();
                if (usernameExist)
                {
                    ModelState.AddModelError("Username", "Username already taken. Please try again with other username.");
                }
                if (emailExist)
                {
                    ModelState.AddModelError("Email", "Email Address already taken. Please try again with other Email Address.");
                }
            }
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Name = model.Name,
                    RegisterOn = generalFunction.GetSystemTimeZoneDateTimeNow(),
                    Gender = model.Gender,
                    UserStatus = "Approved" ,//admin registration is automatically approved
                    ProfilePicName = "defaultProfilePic.png"
                };
                var result = await UserManager.CreateAsync(user, model.Password); //create user and save in db
                if (result.Succeeded)
                {
                    //set default setting
                    Setting setting = new Setting();
                    setting.Id = Guid.NewGuid();
                    setting.Background = "Light";
                    setting.UserId = user.Id;
                    db.Settings.Add(setting);
                    db.SaveChanges();
                    InsertRole();
                    //if create user succeeded, assign role to the user
                    var assignRoleResult = UserManager.AddToRole(user.Id, "Admin");
                    if (assignRoleResult.Succeeded)
                    {
                        ModelState.Clear(); //clear the model state if everthing succeeded
                        TempData["RegisterAdminSuccess"] = "Admin User is created successfully. Login Now!";
                    }

                    if (User.Identity.GetUserId() != null && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("manageuser");
                    }
                    else
                    {
                        return RedirectToAction("login");
                    }
                }
                AddErrors(result);
            }
            string loginUserId = User.Identity.GetUserId();
            if (User.Identity.GetUserId() != null && User.IsInRole("Admin"))
            {
                return View("addadmin",model);
            }
            else
            {
                return View(model);
            }
            
        }
        #endregion

        #region --------------------------------------- Register Teacher / Student ---------------------------------------
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
    
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (model != null)
            {
                if (model.UserRole == null)
                {
                    ModelState.AddModelError("UserRole", "Registered As is required.");
                }
                bool usernameExist = db.AspNetUsers.Where(a => a.UserName == model.Username).Any();
                bool emailExist = db.AspNetUsers.Where(a => a.Email == model.Email).Any();                
                if (usernameExist)
                {
                    ModelState.AddModelError("Username", "Username already taken. Please try again with other username.");
                }
                if (emailExist)
                {
                    ModelState.AddModelError("Email", "Email Address already taken. Please try again with other Email Address.");
                }               
            }
            if (ModelState.IsValid)
            {
                string userRole = "";
                if (model.UserRole == 'T')
                {
                    userRole = "Teacher";
                }
                if (model.UserRole == 'S')
                {
                    userRole = "Student";
                }
                var user = new ApplicationUser { 
                    UserName = model.Username, 
                    Email = model.Email, 
                    Name = model.Name,
                    Gender = model.Gender,
                    IntakeYear = model.IntakeYear,
                    RegisterOn = generalFunction.GetSystemTimeZoneDateTimeNow(),
                    UserStatus = "New",
                    ProfilePicName = "defaultProfilePic.png",
                    Code = model.Code
                };
                var result = await UserManager.CreateAsync(user, model.Password); //create user and save in db
                if (result.Succeeded)
                {                   
                    //set default setting
                    Setting setting = new Setting();
                    setting.Id = Guid.NewGuid();
                    setting.Background = "Light";
                    setting.UserId = user.Id;
                    db.Settings.Add(setting);
                    db.SaveChanges();

                    //if create user succeeded, assign role to the user
                    var assignRoleResult = UserManager.AddToRole(user.Id, userRole);
                    if (assignRoleResult.Succeeded)
                    {
                        ModelState.Clear(); //clear the model state if everthing succeeded
                        TempData["RegisterSuccess"] = "Your registration is success. Please wait for approval email notification.";
                    }

                    if (User.Identity.GetUserId() != null && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("manageuser");
                    }
                    else
                    {
                        return RedirectToAction("register");
                    }
                }
                AddErrors(result);
            }

            string loginUserId = User.Identity.GetUserId();

            if (loginUserId != null && User.IsInRole("Admin"))
            {
                return View("adduser", model);
            }
            else
            {
                // If we got this far, something failed, redisplay form
                return View(model);
            }
            
        }
        #endregion

        #region --------------------------------------- Change Password ---------------------------------------
        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            ChangePasswordViewModel model = new ChangePasswordViewModel();
            string loginUserId = User.Identity.GetUserId();
            return View(model);
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                TempData["ChangePasswordSuccess"] = "Password Changed Successfully";
                return RedirectToAction("changepassword");
            }
            AddErrors(result);
            return View(model);
        }

       

        #endregion

        #region --------------------------------------- Forgot Password ---------------------------------------
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var username = db.AspNetUsers.Where(a => a.Email == model.Email).Select(a => a.UserName).DefaultIfEmpty("").FirstOrDefault();
                var user = await UserManager.FindByNameAsync(username);
                //var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                //send email to notify the user
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("resetpassword", "account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                EmailTemplate emailTemplate = GetResetPasswordTemplate(user.UserName, callbackUrl);
                SendEmail(user.Email, emailTemplate.Subject, emailTemplate.Body);
                TempData["ResetPasswordEmailSent"] = "Reset Password Email Sent.";
                return RedirectToAction("forgotpasswordconfirmation", "account");

            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
 
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        #endregion

        #region --------------------------------------- Reset Password ---------------------------------------
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var username = db.AspNetUsers.Where(a => a.Email == model.Email).Select(a => a.UserName).DefaultIfEmpty("").FirstOrDefault();
            var user = await UserManager.FindByNameAsync(username);

            //var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        #endregion

        #region --------------------------------------- Email ---------------------------------------
        [AllowAnonymous]
        public EmailTemplate GetRegistrationApprovedTemplate(string Username, string callbackUrl)
        {
            string websiteName = generalFunction.GetAppSettingsValue("portalName");
            var link = "<a href='" + callbackUrl + "'>here</a>";
            string body = "<p>Hi " + Username + ",<br /><br /> Thanks for signing up an account in " + websiteName + ".</p>" +
                "<p>Your registration is approved. You can now click <a href='" + callbackUrl + "'> here</a> to login to the portal. Thank You.</p>" +
                        "<p>If you did not register an account on " + websiteName + ", please ignore this email.</p>" +
                        "<p><i>Do not reply to this email.</i></p><br /><br /><p>Regards,<br>" + websiteName + "</p>";
            string subject = "Account Registration on " + websiteName + " is Approved";

            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = subject;
            emailTemplate.Body = body;
            return emailTemplate;
        }

        [AllowAnonymous]
        public EmailTemplate GetRegistrationRejectedTemplate(string Username, string callbackUrl)
        {
            string websiteName = generalFunction.GetAppSettingsValue("portalName");
            var link = "<a href='" + callbackUrl + "'>here</a>";
            string body = "<p>Hi " + Username + ",<br /><br /> Thanks for signing up an account in " + websiteName + ".</p>" +
                "<p>Sorry to inform you that your registration is rejected. We have found that your registration detail is invalid." +
                " You can click <a href='" + callbackUrl + "'> here</a> to register again and make sure your detail is valid. Thank You.</p>" +
                        "<p>If you did not register an account on " + websiteName + ", please ignore this email.</p>" +
                        "<p><i>Do not reply to this email.</i></p><br /><br /><p>Regards,<br>" + websiteName + "</p>";
            string subject = "Account Registration on " + websiteName + " is Rejected";

            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = subject;
            emailTemplate.Body = body;
            return emailTemplate;
        }

        [AllowAnonymous]
        public EmailTemplate GetResetPasswordTemplate(string Username, string callbackUrl)
        {
            string websiteName = generalFunction.GetAppSettingsValue("portalName");
            var link = "<a href='" + callbackUrl + "'>here</a>";
            string body = "<p>Hi " + Username + ",<br /><br /> You just requested to reset password for " + websiteName + " account.</p>" +
                "<p>" +
                "Click <a href='" + callbackUrl + "'> here</a> to reset your password. Thank You.</p>" +
                        "<p>If you did not request a password reset on " + websiteName + ", please ignore this email.</p>" +
                        "<p><i>Do not reply to this email.</i></p><br /><br /><p>Regards,<br>" + websiteName + "</p>";
            string subject = "Reset Password on " + websiteName;

            EmailTemplate emailTemplate = new EmailTemplate();
            emailTemplate.Subject = subject;
            emailTemplate.Body = body;
            return emailTemplate;
        }

        [AllowAnonymous]
        public void SendEmail(string email, string subject, string body)
        {
            string host = generalFunction.GetAppSettingsValue("smtpHost");
            string strPort = generalFunction.GetAppSettingsValue("smtpPort");
            int port = Int32.Parse(strPort);
            string userName = generalFunction.GetAppSettingsValue("smtpUserName");
            string password = generalFunction.GetAppSettingsValue("smtpPassword");
            var client = new SmtpClient(host, port);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(userName, password);
            client.EnableSsl = true;

            MailMessage mail = new MailMessage(userName, email, subject, body);
            mail.IsBodyHtml = true;
            client.Send(mail);
        }
        #endregion

        #region --------------------------------------- Admin Manage Users ---------------------------------------
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddUser()
        {
            string loginUserId = User.Identity.GetUserId();
            RegisterViewModel model = new RegisterViewModel();
            return View(model);
        }
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddAdmin()
        {
            string loginUserId = User.Identity.GetUserId();
            RegisterAdminViewModel model = new RegisterAdminViewModel();
            return View(model);
        }
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditUser(string Id)
        {
            string loginUserId = User.Identity.GetUserId();
            UserViewModel model = new UserViewModel();
            if (Id != null)
            {
                model = GetUserViewModel(Id);
            }
            return View(model);
        }

        [HttpPost]
        [CustomAuthorize(Roles ="Admin")]
        public ActionResult EditUser(UserViewModel model)
        {
            if (model != null)
            {
                AspNetUsers users = db.AspNetUsers.Where(a => a.Id == model.Id).FirstOrDefault();
                if (model.Role != "Admin")
                {
                    if (model.UserRole == null)
                    {
                        ModelState.AddModelError("UserRole", "User Role is required.");
                    }
                    if (model.IntakeYear == null)
                    {
                        ModelState.AddModelError("IntakeYear", "Intake Year is required.");
                    }
                }

                bool usernameExist = db.AspNetUsers.Where(a => a.UserName == model.Username && a.UserName != users.UserName).Any();
                bool emailExist = db.AspNetUsers.Where(a => a.Email == model.Email && a.Email != users.Email).Any();
                if (usernameExist)
                {
                    ModelState.AddModelError("Username", "Username already taken. Please try again with other username.");
                }
                if (emailExist)
                {
                    ModelState.AddModelError("Email", "Email Address already taken. Please try again with other Email Address.");
                }

                if (ModelState.IsValid)
                {
                    //edit and save user data
                    users.UserName = model.Username;
                    users.Email = model.Email;
                    users.Name = model.Name;
                    users.Gender = model.Gender;
                    users.UpdateAdminId = User.Identity.GetUserId();
                    users.UpdateOn = generalFunction.GetSystemTimeZoneDateTimeNow();
                    users.IntakeYear = model.IntakeYear;
                    users.UpdateDetail = "Edit User";
                    users.Code = model.Code;
                    db.Entry(users).State = EntityState.Modified;
                    db.SaveChanges();

                    //edit and save role
                    string userRole = "";
                    if (model.UserRole == "T")
                    {
                        userRole = "Teacher";
                    }
                    if (model.UserRole == "S")
                    {
                        userRole = "Student";
                    }
                    string roleId = db.AspNetRoles.Where(a => a.Name == userRole).Select(a => a.Id).DefaultIfEmpty("").FirstOrDefault();
                    AspNetUserRoles aspNetUserRoles = db.AspNetUserRoles.Where(a => a.UserId == users.Id).FirstOrDefault();
                    aspNetUserRoles.RoleId = roleId;
                    db.Entry(aspNetUserRoles).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["UserUpdated"] = "Updated Successfully!";
                    return RedirectToAction("manageuser");
                }
            }
            ViewBag.ResetPasswordActive = false;
            ViewBag.EditUserData = true;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]
        public async Task<ActionResult> ResetPasswordForUser(UserViewModel model)
        {
            var user = await UserManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                ModelState.AddModelError("NewPassword", "User does not exists.");
            }

            if (ModelState.IsValid)
            {
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var result = await UserManager.ResetPasswordAsync(user.Id, code, model.NewPassword);
                if (result.Succeeded)
                {
                    AspNetUsers aspNetUsers = db.AspNetUsers.Where(a => a.Id == user.Id).FirstOrDefault();
                    aspNetUsers.UpdateOn = generalFunction.GetSystemTimeZoneDateTimeNow();
                    aspNetUsers.UpdateAdminId = User.Identity.GetUserId();
                    aspNetUsers.UpdateDetail = "Reset Password";
                    db.Entry(aspNetUsers).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["ResetPasswordSuccess"] = "Password Changed Successfully";
                    return RedirectToAction("manageuser");
                }
            }
            ViewBag.ResetPasswordActive = true;
            ViewBag.EditUserData = false;
            return View("edituser",model);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ManageUser()
        {
            string loginUserId = User.Identity.GetUserId();
            UserViewModel model = new UserViewModel();
            model.AllUserCount = db.AspNetUsers.Count();
            model.NewUserCount = db.AspNetUsers.Where(a => a.UserStatus == "New").Count();
            model.ApprovedUserCount = db.AspNetUsers.Where(a => a.UserStatus == "Approved").Count();
            model.DisabledLoginUserCount = db.AspNetUsers.Where(a => a.UserStatus == "Disabled").Count(); //6.8.2020 solve bug
            string adminRoleId = db.AspNetRoles.Where(a => a.Name == "Admin").Select(a => a.Id).FirstOrDefault();
            string teacherRoleId = db.AspNetRoles.Where(a => a.Name == "Teacher").Select(a => a.Id).FirstOrDefault();
            string studentRoleId = db.AspNetRoles.Where(a => a.Name == "Student").Select(a => a.Id).FirstOrDefault();
            model.TotalAdmin = db.AspNetUserRoles.Where(a => a.RoleId == adminRoleId).Count();
            model.TotalTeacher = db.AspNetUserRoles.Where(a => a.RoleId == teacherRoleId).Count();
            model.TotalStudent = db.AspNetUserRoles.Where(a => a.RoleId == studentRoleId).Count();
            return View(model);
        }

        public ActionResult Read_User([DataSourceRequest] DataSourceRequest request)
        {
            string currenUserId = User.Identity.GetUserId();
            List<UserViewModel> result = new List<UserViewModel>();
            try
            {
                result = (from t1 in db.AspNetUsers
                          join t2 in db.AspNetUserRoles on t1.Id equals t2.UserId
                          join t3 in db.AspNetRoles on t2.RoleId equals t3.Id
                          where t1.Id != currenUserId
                          select new UserViewModel
                          {
                              Id = t1.Id,
                              RoleId = t2.RoleId,
                              Role = t3.Name,
                              Username = t1.UserName,
                              Name = t1.Name,
                              Email = t1.Email,
                              UserStatus = t1.UserStatus,
                              ProfilePic = t1.ProfilePicName,
                              RegisteredOn = t1.RegisterOn,
                              UpdatedOn = t1.UpdateOn,
                              IntakeYear = t1.IntakeYear,
                              Gender = t1.Gender,
                              Code = t1.Code,
                              ApprovedAdmin = db.AspNetUsers.Where(a => a.Id == t1.ApproveAdminId.ToString()).Select(a => a.Name).DefaultIfEmpty("N/A").FirstOrDefault(),
                              UpdateAdmin = db.AspNetUsers.Where(a => a.Id == t1.UpdateAdminId.ToString()).Select(a => a.Name).DefaultIfEmpty("N/A").FirstOrDefault(),
                              UpdateOn = t1.UpdateOn,
                              UpdateDetail = t1.UpdateDetail == null ? "N/A" : t1.UpdateDetail
                          }).OrderBy(o => o.Username).ToList();
                foreach (var item in result)
                {
                    DateTime registeredOn = (DateTime)item.RegisteredOn;
                    item.strRegisteredOn = registeredOn.ToString("yyyy-MM-dd hh:mm tt");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        [CustomAuthorize(Roles = "Admin")]
        public async Task<string> ApproveUser(string Id)
        {
            string errorMessage = "";
            if (Id != null && Id != "")
            {
                try
                {
                    //set user status to approved
                    AspNetUsers user = db.AspNetUsers.Where(a => a.Id == Id).FirstOrDefault();
                    user.UserStatus = "Approved";
                    user.ApproveAdminId = User.Identity.GetUserId();
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();                  
                    //send email to notify the user
                    string code = await UserManager.GenerateUserTokenAsync("login",user.Id);
                    var callbackUrl = Url.Action("login", "account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    EmailTemplate emailTemplate = GetRegistrationApprovedTemplate(user.UserName, callbackUrl);
                    SendEmail(user.Email, emailTemplate.Subject, emailTemplate.Body);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.InnerException.Message;
                }
            }
            return errorMessage;
        }

        [CustomAuthorize(Roles = "Admin")]
        public string RejectUser(string Id)
        {
            string errorMessage = "";
            if (Id != null && Id != "")
            {
                //string userRole = "";
                try
                {
                    //delete user from aspnetusers table    
                    AspNetUsers user = db.AspNetUsers.Where(a => a.Id == Id).FirstOrDefault();
                    db.AspNetUsers.Remove(user);
                    db.SaveChanges();

                    var callbackUrl = Url.Action("register", "account", null, protocol: Request.Url.Scheme);
                    EmailTemplate emailTemplate = GetRegistrationRejectedTemplate(user.UserName, "");
                    SendEmail(user.Email, emailTemplate.Subject, emailTemplate.Body);
                    
                }
                catch (Exception ex)
                {
                    errorMessage = ex.InnerException.Message;
                }
            }
            return errorMessage;
        }

        [CustomAuthorize(Roles = "Admin")]
        public string DisableLogin(string Id)
        {
            string errorMessage = "";
            if (Id != null && Id != "")
            {
                try
                {
                    //set user status to disabled
                    AspNetUsers user = db.AspNetUsers.Find(Id);
                    user.UserStatus = "Disabled";
                    user.UpdateAdminId = User.Identity.GetUserId();
                    user.UpdateDetail = "Disable login";
                    user.UpdateOn = generalFunction.GetSystemTimeZoneDateTimeNow();
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    errorMessage = ex.InnerException.Message;
                }
            }
            return errorMessage;
        }

        [CustomAuthorize(Roles = "Admin")]
        public string DeleteUser(string Id)
        {
            string errorMessage = "";
            if (Id != null && Id != "")
            {
                //string userRole = "";
                try
                {
                    AspNetUserRoles aspNetUserRoles = db.AspNetUserRoles.Where(a => a.UserId == Id).FirstOrDefault();
                    string userrole = db.AspNetRoles.Where(a => a.Id == aspNetUserRoles.RoleId).Select(a => a.Name).FirstOrDefault();
                   
                    if (userrole.Equals("Teacher"))
                    {
                        List<CourseChannel> courseChannels = db.CourseChannels.Where(a => a.CreatorUserId == Id).ToList();
                        foreach (var item in courseChannels)
                        {
                            //delete assignment file from server
                            List<Assignment> assignments = db.Assignments.Where(a => a.CourseChannelId == item.Id).ToList();
                            foreach (var assignment in assignments)
                            {
                                if (assignment.FileName != null)
                                {
                                    try
                                    {
                                        //delete attached file from server
                                        string fullPath = Request.MapPath("~/UploadedFiles/" + assignment.FileName);
                                        if (System.IO.File.Exists(fullPath))
                                        {
                                            System.IO.File.Delete(fullPath);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        errorMessage = e.InnerException.Message;
                                    }
                                   
                                }
                                List<AssignmentSubmission> assignmentSubmissions = db.AssignmentSubmissions.Where(a => a.AssignmentId == assignment.Id).ToList();
                                foreach (var assignmentSubmission in assignmentSubmissions)
                                {
                                    if (assignmentSubmission.SubmissionFile != null)
                                    {
                                        //delete attached file from server
                                        string fullPath = Request.MapPath("~/UploadedFiles/" + assignmentSubmission.SubmissionFile);
                                        if (System.IO.File.Exists(fullPath))
                                        {
                                            System.IO.File.Delete(fullPath);
                                        }
                                    }
                                }
                            }
                            List<Chapter> chapters = db.Chapters.Where(a => a.CourseChannelId == item.Id).ToList();
                            foreach (var chapter in chapters)
                            {
                                List<FileMaterial> fileMaterials = db.FileMaterials.Where(a => a.ChapterId == chapter.Id).ToList();
                                foreach (var fileMaterial in fileMaterials)
                                {
                                    if (fileMaterial.FileName != null)
                                    {
                                        //delete attached file from server
                                        string fullPath = Request.MapPath("~/UploadedFiles/" + fileMaterial.FileName);
                                        if (System.IO.File.Exists(fullPath))
                                        {
                                            System.IO.File.Delete(fullPath);
                                        }
                                    }
                                }
                            }
                            List<Quiz> quizzes = db.Quizzes.Where(a => a.CourseChannelId == item.Id).ToList();
                            foreach (var quiz in quizzes)
                            {
                                List<QuizQuestion> quizQuestions = db.QuizQuestions.Where(a => a.QuizId == quiz.Id).ToList();
                                foreach (var quizQuestion in quizQuestions)
                                {
                                    if (quizQuestion.AttachedImage != null)
                                    {
                                        //delete attached file from server
                                        string fullPath = Request.MapPath("~/UploadedFiles/" + quizQuestion.AttachedImage);
                                        if (System.IO.File.Exists(fullPath))
                                        {
                                            System.IO.File.Delete(fullPath);
                                        }
                                    }
                                }
                            }
                            CourseChannel courseChannel = db.CourseChannels.Where(a => a.Id == item.Id).FirstOrDefault();
                            db.CourseChannels.Remove(courseChannel);
                            db.SaveChanges();
                        }
                    }
                    if (userrole.Equals("Student"))
                    {
                        List<AssignmentSubmission> assignmentSubmissions = db.AssignmentSubmissions.Where(a => a.StudentId == Id).ToList();
                        foreach (var assignmentSubmission in assignmentSubmissions)
                        {
                            if (assignmentSubmission.SubmissionFile != null)
                            {
                                //delete attached file from server
                                string fullPath = Request.MapPath("~/UploadedFiles/" + assignmentSubmission.SubmissionFile);
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }
                        }
                        List<Post> posts = db.Posts.Where(a => a.UserId == Id).ToList();
                        if (posts.Count > 0)
                        {
                            foreach (var item in posts)
                            {
                                db.Posts.Remove(item);
                                db.SaveChanges();
                            }
                        }
                    }
                    var chatchannellist = db.ChatChannels.Where(a => a.ChatUserOne == Id || a.ChatUserTwo == Id).ToList();
                    foreach (var item in chatchannellist)
                    {
                        db.ChatChannels.Remove(item);
                        db.SaveChanges();
                    }
                    Setting setting = db.Settings.Where(a => a.UserId == Id).FirstOrDefault();
                    if (setting != null)
                    {
                        db.Settings.Remove(setting);
                        db.SaveChanges();
                    }
                    AspNetUsers user = db.AspNetUsers.Where(a => a.Id == Id).FirstOrDefault();
                    if (user != null)
                    {
                        db.AspNetUsers.Remove(user);
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.InnerException.Message;
                }
            }
            return errorMessage;
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            var AuthenticationManager = HttpContext.GetOwinContext().Authentication;
            AuthenticationManager.SignOut();
            Session.Abandon();
            return RedirectToAction("index", "home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}