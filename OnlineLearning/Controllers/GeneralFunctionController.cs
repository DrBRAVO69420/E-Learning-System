using Microsoft.AspNet.Identity;
using OnlineLearning.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace OnlineLearning.Controllers
{
    public class GeneralFunctionController : Controller
    {
        private DefaultDBContext db = new DefaultDBContext();

        public string GetAppSettingsValue(string key)
        {
            switch (key)
            {
                // smtp setting
                case "smtpUserName": return ConfigurationManager.AppSettings["smtpUserName"].ToString();
                case "smtpPassword": return ConfigurationManager.AppSettings["smtpPassword"].ToString();
                case "smtpHost": return ConfigurationManager.AppSettings["smtpHost"].ToString();
                case "smtpPort": return ConfigurationManager.AppSettings["smtpPort"].ToString();
                //portal general setting
                case "portalName": return ConfigurationManager.AppSettings["portalName"].ToString();
                case "timeZone": return ConfigurationManager.AppSettings["timeZone"].ToString();
                default: break;
            }
            return "";
        }

        public DateTime? GetSystemTimeZoneDateTimeNow()
        {
            string timeZone = GetAppSettingsValue("timeZone");
            DateTime dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, timeZone);
            return dateTime;
        }

        public string GetChannelLongCoverImage(Guid? courseChannelId)
        {
            Guid? themeid = db.CourseChannels.Where(a => a.Id == courseChannelId).Select(a => a.ThemeId).FirstOrDefault();
            string img = db.Themes.Where(a => a.Id == themeid).Select(a => a.ImageSource).DefaultIfEmpty("/CourseChannelTheme/Default/1800-default-min.png").FirstOrDefault();
            return img;
        }

        public string GetUserBackgroundSetting()
        {
            string loginUserId = User.Identity.GetUserId();
            string bg = db.Settings.Where(a => a.UserId == loginUserId).Select(a => a.Background).DefaultIfEmpty("Dark").FirstOrDefault();
            return bg;
        }

        public JsonResult GetLoginUser()
        {
            string loginUserId = User.Identity.GetUserId();
            UserViewModel model = new UserViewModel();
            model.Id = loginUserId;
            model.Name = db.AspNetUsers.Where(a => a.Id == loginUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
            model.ProfilePic = db.AspNetUsers.Where(a => a.Id == loginUserId).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public string GetCourseChannelName(Guid? courseChannelId)
        {
            string courseName = db.CourseChannels.Where(a => a.Id == courseChannelId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
            return courseName;
        }

        public CourseChannelTopNavViewModels GetTopNavViewModels(Guid? courseChannelId)
        {
            CourseChannelTopNavViewModels model = new CourseChannelTopNavViewModels();
            if (courseChannelId != null)
            {
                model.CourseChannelId = courseChannelId;
                model.CourseChannelName = GetCourseChannelName(courseChannelId);
                model.ImageSource = GetChannelLongCoverImage(courseChannelId);
            }
            return model;
        }

        [AllowAnonymous]
        public ActionResult Unauthorized()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Pdf_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        [HttpPost]
        public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }
    }
}