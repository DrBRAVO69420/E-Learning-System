using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNet.Identity;
using OnlineLearning.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineLearning.Controllers
{
    public class AnnouncementController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext();

        // Admin manage general announcement that can be seen by all users from the whole campus
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult General()
        {
            AnnouncementViewModel model = new AnnouncementViewModel();
            string loginUserId = User.Identity.GetUserId();
            return View(model);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditGeneral(Guid? Id)
        {
            AnnouncementViewModel models = new AnnouncementViewModel();
            if (Id == null)
            {
                models.TitleText = "";
                models.Details = "";
            }
            else
            {
                models = GetAnnouncementViewModel(Id);
            }
            return View(models);
        }

        [HttpPost, ValidateInput(false)]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditGeneral(AnnouncementViewModel model)
        {
            string loginUserId = User.Identity.GetUserId();
            if (model != null)
            {
                if (ModelState.IsValid)
                {
                    Announcement announcement = new Announcement();
                    if (model.Id == null || model.Id == Guid.Empty) //new record
                    {
                        announcement.Id = Guid.NewGuid();
                        announcement.CreatorId = User.Identity.GetUserId();
                        announcement.Type = "Campus";
                        announcement.CreatedOn = generalFunction.GetSystemTimeZoneDateTimeNow();
                    }
                    else //update record
                    {
                        announcement = db.Announcements.Find(model.Id);
                        announcement.UpdatedBy = User.Identity.GetUserId();
                    }

                    announcement.Title = model.TitleText;
                    announcement.Detail = model.Details;
                    announcement.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();

                    if (model.Id == null || model.Id == Guid.Empty) //new record
                    {
                        db.Announcements.Add(announcement);
                    }
                    else //update record
                    {
                        db.Entry(announcement).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    ModelState.Clear();
                    TempData["AnnouncementUpdated"] = "Updated Successfully!";
                    return RedirectToAction("general");
                }
                model.TopNavViewModels = generalFunction.GetTopNavViewModels(model.CourseChannelId);
            }
            return View(model);
        }


        public ActionResult Detail(Guid? Id)
        {
            AnnouncementViewModel model = new AnnouncementViewModel();
            if (Id != null && Id != Guid.Empty)
            {
                model = GetAnnouncementViewModel(Id);
            }
            return View(model);
        }

        //id = announcement id
        public AnnouncementViewModel GetAnnouncementViewModel(Guid? Id)
        {
            AnnouncementViewModel model = new AnnouncementViewModel();
            string userId = User.Identity.GetUserId();
            if (Id != null && Id != Guid.Empty)
            {
                Announcement announcement = db.Announcements.Find(Id);
                model.Id = announcement.Id;
                model.TitleText = announcement.Title;
                model.Details = announcement.Detail;
                model.Type = announcement.Type;
                model.LastUpdate = announcement.LastUpdate;
                model.UpdatedBy = announcement.UpdatedBy;
                model.AdminName = db.AspNetUsers.Where(a => a.Id == announcement.UpdatedBy).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
                model.CreatorId = announcement.CreatorId;
                model.CreatorName = db.AspNetUsers.Where(a => a.Id == announcement.CreatorId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
                model.CreatorProfilePic = db.AspNetUsers.Where(a => a.Id == announcement.CreatorId).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault();
                model.CourseChannelId = announcement.CourseChannelId;
                if (announcement.CourseChannelId != null)
                {
                    model.TopNavViewModels = generalFunction.GetTopNavViewModels(announcement.CourseChannelId);
                }
            }
            return model;
        }

        //id = course channel id
        [CustomAuthorize(Roles = "Teacher, Student")]
        public ActionResult CourseChannel(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            AnnouncementViewModel model = new AnnouncementViewModel();
            if (Id!=null)
            {
                model.TopNavViewModels = generalFunction.GetTopNavViewModels(Id);
            }
            return View(model);
        }

        [CustomAuthorize (Roles = "Teacher")]
        public ActionResult EditAnnouncement(Guid? courseChannelId, Guid? announcementId)
        {
            string loginUserId = User.Identity.GetUserId();
            AnnouncementViewModel models = new AnnouncementViewModel();
            if (announcementId != null && announcementId != Guid.Empty)
            {
                models = GetAnnouncementViewModel(announcementId);
            }
            if (courseChannelId != null && courseChannelId != Guid.Empty)
            {
                models.CourseChannelId = courseChannelId;
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(courseChannelId);
            }
            return View(models);
        }

        [HttpPost, ValidateInput(false)]
        [CustomAuthorize(Roles = "Teacher")]
        public ActionResult EditAnnouncement(AnnouncementViewModel models)
        {
            string loginUserId = User.Identity.GetUserId();
            if (models != null)
            {
                if (ModelState.IsValid)
                {
                    Announcement announcement = new Announcement();
                    if (models.Id == null || models.Id == Guid.Empty) //new record
                    {
                        announcement.Id = Guid.NewGuid();
                        announcement.CourseChannelId = models.CourseChannelId;
                        announcement.CreatorId = User.Identity.GetUserId();
                        announcement.Type = "Course";
                        announcement.CreatedOn = generalFunction.GetSystemTimeZoneDateTimeNow();
                    }
                    else //update record
                    {
                        announcement = db.Announcements.Find(models.Id);
                        announcement.UpdatedBy = User.Identity.GetUserId();
                    }

                    announcement.Title = models.TitleText;
                    announcement.Detail = models.Details;
                    announcement.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();

                    if (models.Id == null || models.Id == Guid.Empty) //new record
                    {
                        db.Announcements.Add(announcement);
                    }
                    else //update record
                    {
                        db.Entry(announcement).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    ModelState.Clear();
                    TempData["AnnouncementUpdated"] = "Updated Successfully!";
                    return RedirectToAction("coursechannel", new { Id = models.CourseChannelId });
                }
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(models.CourseChannelId);
            }
            return View(models);
        }


        public ActionResult Read_CourseAnnouncement([DataSourceRequest] DataSourceRequest request, Guid CourseChannelId)
        {
            List<AnnouncementViewModel> result = new List<AnnouncementViewModel>();
            try
            {
                result = (from t1 in db.Announcements
                          join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                          where t2.Id == CourseChannelId && t1.Type == "Course"
                          select new AnnouncementViewModel
                          {
                              Id = t1.Id,
                              TitleText = t1.Title,
                              Details = t1.Detail,
                              CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                              LastUpdate = t1.LastUpdate,
                              UpdatedBy = t1.UpdatedBy,
                              CourseChannelId = t1.CourseChannelId                             
                          }).OrderByDescending(o => o.LastUpdate).ToList();
                foreach (var item in result)
                {
                    DateTime lastupdate = (DateTime)item.LastUpdate;
                    item.strLastUpdate = lastupdate.ToString("yyyy-MM-dd hh:mm tt");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        public string Delete_Announcement(Guid Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    Announcement announcement = db.Announcements.Find(Id);                   
                    //delete data from database
                    db.Announcements.Remove(announcement);
                    db.SaveChanges();

                    ModelState.Clear(); //if everthing is ok and executed successfully, clear the model state
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message;
                }
            }
            return error;
        }

        public ActionResult Read_CampusAnnouncement([DataSourceRequest] DataSourceRequest request)
        {
            List<AnnouncementViewModel> result = new List<AnnouncementViewModel>();
            try
            {
                result = GetCampusAnnouncementList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        public List<AnnouncementViewModel> GetCampusAnnouncementList()
        {
            List<AnnouncementViewModel> result = (from t1 in db.Announcements
                                                  where t1.Type == "Campus"
                                                  select new AnnouncementViewModel
                                                  {
                                                      Id = t1.Id,
                                                      TitleText = t1.Title,
                                                      Details = t1.Detail,
                                                      CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                                      LastUpdate = t1.LastUpdate,
                                                      UpdatedBy = t1.UpdatedBy,
                                                      CourseChannelId = t1.CourseChannelId,
                                                      AdminName = t1.UpdatedBy == null ? db.AspNetUsers.Where(a => a.Id == t1.CreatorId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                                                      : db.AspNetUsers.Where(a => a.Id == t1.UpdatedBy).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                                                  }).OrderByDescending(o => o.LastUpdate).ToList();
            foreach (var item in result)
            {
                DateTime lastupdate = (DateTime)item.LastUpdate;
                item.strLastUpdate = lastupdate.ToString("yyyy-MM-dd hh:mm tt");
            }
            return result;
        }

        public AnnouncementViewModel GetCampusAnnouncement(Guid? Id)
        {
            AnnouncementViewModel result = (from t1 in db.Announcements
                                                  where t1.Type == "Campus" && t1.Id == Id
                                                  select new AnnouncementViewModel
                                                  {
                                                      Id = t1.Id,
                                                      TitleText = t1.Title,
                                                      Details = t1.Detail,
                                                      CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                                      LastUpdate = t1.LastUpdate,
                                                      UpdatedBy = t1.UpdatedBy,
                                                      CourseChannelId = t1.CourseChannelId,
                                                      AdminName = t1.UpdatedBy == null ? db.AspNetUsers.Where(a => a.Id == t1.CreatorId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                                                      : db.AspNetUsers.Where(a => a.Id == t1.UpdatedBy).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                                                  }).FirstOrDefault();
            
            DateTime lastupdate = (DateTime)result.LastUpdate;
            result.strLastUpdate = lastupdate.ToString("yyyy-MM-dd hh:mm tt");
            
            return result;
        }

        public JsonResult GetCampusAnnouncement_Json(Guid? Id)
        {
            var result = GetCampusAnnouncement(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}