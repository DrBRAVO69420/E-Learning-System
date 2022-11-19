using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNet.Identity;
using OnlineLearning.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace OnlineLearning.Controllers
{
    public class ThreadController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext();

        //id = course channel id
        public ActionResult List(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            ThreadViewModels models = new ThreadViewModels();
            if (Id != null && Id != Guid.Empty)
            {
                models.CourseChannelId = Id;
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(Id);
                models.TotalThread = db.Threads.Where(a => a.CourseChannelId == Id).Count();
                models.ThreadByLoginUser = db.Threads.Where(a => a.UserId == loginUserId).Count();
                models.ThreadByStudent = (from t1 in db.Threads
                                          join t2 in db.AspNetUsers on t1.UserId equals t2.Id
                                          join t3 in db.AspNetUserRoles on t2.Id equals t3.UserId
                                          join t4 in db.AspNetRoles on t3.RoleId equals t4.Id
                                          where t4.Name == "Student" && t1.CourseChannelId == Id
                                          select t1.Id).Count();
                models.ThreadByTeacher = (from t1 in db.Threads
                                          join t2 in db.AspNetUsers on t1.UserId equals t2.Id
                                          join t3 in db.AspNetUserRoles on t2.Id equals t3.UserId
                                          join t4 in db.AspNetRoles on t3.RoleId equals t4.Id
                                          where t4.Name == "Teacher" && t1.CourseChannelId == Id
                                          select t1.Id).Count();
            }
            return View(models);
        }

        public ActionResult EditThread(Guid? courseChannelId, Guid? threadId)
        {
            string loginUserId = User.Identity.GetUserId();
            ThreadViewModels models = new ThreadViewModels();
            if (threadId != null && threadId != Guid.Empty)
            {
                models = GetThreadViewModels(threadId);
            }
            if (courseChannelId != null && courseChannelId != Guid.Empty)
            {
                models.CourseChannelId = courseChannelId;
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(courseChannelId);
            }
            return View(models);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditThread(ThreadViewModels models)
        {
            string loginUserId = User.Identity.GetUserId();
            if (models!=null)
            {
                //check duplicate title
                string originalTitle = "";
                bool titleExists = false;
                Thread thread = new Thread();
                if (models.Id !=  null && models.Id != Guid.Empty)
                {
                    thread = db.Threads.Find(models.Id);
                    originalTitle = thread.Title;
                    titleExists = db.Threads.Where(a => a.Title == models.ThreadTitle && a.Title != originalTitle).Any();
                }
                else
                {
                    titleExists = db.Threads.Where(a => a.Title == models.ThreadTitle).Any();
                }
                if (ModelState.IsValid)
                {
                    if (models.Id == null || models.Id == Guid.Empty)//new record
                    {
                        thread.Id = Guid.NewGuid();
                        thread.UserId = loginUserId;
                        thread.CourseChannelId = models.CourseChannelId;
                    }       
                    thread.Title = models.ThreadTitle;
                    thread.Content = models.Content;
                    thread.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                    if (models.Id == null || models.Id == Guid.Empty)
                    {
                        db.Threads.Add(thread);
                        TempData["ThreadUpdated"] = "Created Successfully!";
                    }
                    else
                    {
                        db.Entry(thread).State = EntityState.Modified;
                        TempData["ThreadUpdated"] = "Updated Successfully!";
                    }
                    db.SaveChanges();
                    ModelState.Clear();
                    models.Id = thread.Id;
                    return RedirectToAction("list", new { Id = models.CourseChannelId });
                }
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(models.CourseChannelId);
            }
            return View(models);
        }

        public ThreadViewModels GetThreadViewModels(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            ThreadViewModels models = new ThreadViewModels();
            Thread thread = db.Threads.Find(Id);
            models.Id = thread.Id;
            models.CourseChannelId = thread.CourseChannelId;
            models.ThreadTitle = thread.Title;
            models.Content = thread.Content;
            models.UserId = thread.UserId;
            models.ProfilePic = db.AspNetUsers.Where(a => a.Id == thread.UserId).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault();
            models.Name = db.AspNetUsers.Where(a => a.Id == thread.UserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
            models.LastUpdate = thread.LastUpdate;
            models.TopNavViewModels = generalFunction.GetTopNavViewModels(thread.CourseChannelId);
            models.TotalThread = db.Threads.Where(a => a.CourseChannelId == thread.CourseChannelId).Count();
            models.ThreadByLoginUser = db.Threads.Where(a => a.UserId == loginUserId).Count();
            models.ThreadByStudent = (from t1 in db.Threads
                                      join t2 in db.AspNetUsers on t1.UserId equals t2.Id
                                      join t3 in db.AspNetUserRoles on t2.Id equals t3.UserId
                                      join t4 in db.AspNetRoles on t3.RoleId equals t4.Id
                                      where t4.Name == "Student"
                                      select t1.Id).Count();
            models.ThreadByTeacher = (from t1 in db.Threads
                                      join t2 in db.AspNetUsers on t1.UserId equals t2.Id
                                      join t3 in db.AspNetUserRoles on t2.Id equals t3.UserId
                                      join t4 in db.AspNetRoles on t3.RoleId equals t4.Id
                                      where t4.Name == "Teacher"
                                      select t1.Id).Count();
            models.PostList = (from t1 in db.Posts
                               join t2 in db.Threads on t1.ThreadId equals t2.Id
                               where t2.Id == Id
                               select new PostViewModels
                               {
                                   Id = t1.Id,
                                   ThreadId = t2.Id,
                                   PostId = t1.PostId,
                                   ReplyToContent = db.Posts.Where(a => a.Id == t1.PostId).Select(a => a.Content).DefaultIfEmpty().FirstOrDefault(),
                                   Title = t1.Title,
                                   Content = t1.Content,
                                   LastUpdate = t1.LastUpdate,
                                   ProfilePic = db.AspNetUsers.Where(a => a.Id == t1.UserId).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault(),
                                   UserId = t1.UserId,
                                   UserFullName = db.AspNetUsers.Where(a => a.Id == t1.UserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                   PostReplies = db.Posts.Where(a => a.PostId == t1.Id).Select(a => a.Id).ToList()
                               }).OrderBy(o => o.LastUpdate).ToList();
            foreach (var item in models.PostList)
            {
                if (item.ReplyToContent !=null)
                {
                    item.ReplyToContent = StripHTML(item.ReplyToContent);
                }
                
            }
            return models;
        }

        public string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public JsonResult GetPostReplies(Guid? Id)
        {
            var result = db.Posts.Where(a => a.PostId == Id).Select(a => a.Id).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_Thread([DataSourceRequest] DataSourceRequest request, Guid CourseChannelId)
        {
            List<ThreadViewModels> result = new List<ThreadViewModels>();
            string loginUserId = User.Identity.GetUserId();
            try
            {
                result = (from t1 in db.Threads
                          where t1.CourseChannelId == CourseChannelId
                          select new ThreadViewModels
                          {
                              Id = t1.Id,
                              ThreadTitle = t1.Title,
                              Content = t1.Content,
                              PostCount = db.Posts.Where(a => a.ThreadId == t1.Id).Count(),
                              UserId = t1.UserId,
                              Name = db.AspNetUsers.Where(a => a.Id == t1.UserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                              ProfilePic = db.AspNetUsers.Where(a => a.Id == t1.UserId).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault(),
                              LastUpdate = t1.LastUpdate,
                              CourseChannelId = t1.CourseChannelId
                          }).OrderByDescending(o => o.LastUpdate).ToList(); //newest first
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

        public string Delete_Thread(Guid? Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    //delete thread, it will also delete all its replies in Post table, which is set from sql query (on delete cascade)
                    Thread thread = db.Threads.Find(Id);
                    db.Threads.Remove(thread);
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

        public string Delete_Reply(Guid? Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    //delete from thread
                    Post post = db.Posts.Where(a => a.Id == Id).FirstOrDefault();
                    db.Posts.Remove(post);
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

        //Id = thread id
        public ActionResult Detail(Guid? Id)
        {
            ThreadViewModels models = GetThreadViewModels(Id);
            return View(models);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Detail(ThreadViewModels models)
        {
            if (models != null)
            {
                if (ModelState.IsValid)
                {
                    Post post = new Post();
                    post.Id = Guid.NewGuid();
                    post.ThreadId = models.Id;
                    post.PostId = models.PostViewModels.PostId;
                    post.Title = "Re: " + models.ThreadTitle;
                    post.UserId = User.Identity.GetUserId();
                    post.Content = models.PostViewModels.Content;
                    post.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                    db.Posts.Add(post);
                    db.SaveChanges();
                    TempData["ReplyPosted"] = "Reply Posted Successfully!";
                    return RedirectToAction("detail", new { Id = models.Id });
                }
            }
            return View(models);
        }

    }
}