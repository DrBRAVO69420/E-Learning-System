using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNet.Identity;
using OnlineLearning.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineLearning.Controllers
{
    public class HomeController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private AnnouncementController announcement = new AnnouncementController();
        private DefaultDBContext db = new DefaultDBContext();

        public ActionResult Index()
        {
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

        [CustomAuthorize(Roles = "Teacher")]
        public ActionResult Teacher()
        {
            TeacherHomeDashboardViewModels models = new TeacherHomeDashboardViewModels();
            string loginUserId = User.Identity.GetUserId();
            DateTime now = (DateTime)generalFunction.GetSystemTimeZoneDateTimeNow();
            models.TotalAssignment = db.Assignments.Where(a => a.TeacherId == loginUserId).Count();
            List<Guid> courseChannelList = db.CourseChannels.Where(a => a.CreatorUserId == loginUserId).Select(a => a.Id).ToList();
            models.TotalCourseChannels = courseChannelList.Count;
            int? threadInCourse = 0;
            foreach (var courseId in courseChannelList)
            {
                threadInCourse += db.Threads.Where(a => a.CourseChannelId == courseId).Count();
            }
            models.TotalDiscussionTopics = threadInCourse;
            models.TotalQuizzes = db.Quizzes.Where(a => a.CreatorId == loginUserId).Count();
            models.UpcomingExpiredAssignments = (from t1 in db.Assignments
                                                 join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                                 where t1.DueDate != null && t1.DueDate >= now && t2.CreatorUserId == loginUserId
                                                 select new AssignmentViewModels
                                                 {
                                                     Id = t1.Id,
                                                     AssignmentTitle = t1.Title,
                                                     DueDate = t1.DueDate,
                                                     CourseChannelName = t2.Name
                                                 }).OrderBy(o => o.DueDate).Take(5).ToList();
            foreach (var item in models.UpcomingExpiredAssignments)
            {
                    TimeSpan timeToEvent = item.DueDate.Value.Subtract(now);
                    string remaining = string.Format("{0} Days, {1} Hours, {2} Minutes Left",
                                   timeToEvent.Days, timeToEvent.Hours, timeToEvent.Minutes);
                    item.TimeRemaining = remaining;
            }
            models.UpcomingExpiredQuizzes = (from t1 in db.Quizzes
                                             join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                             where t1.DueDateTime != null && t1.DueDateTime >= now && t2.CreatorUserId == loginUserId
                                             select new QuizViewModels
                                             {
                                                 Id = t1.Id,
                                                 QuizTitle = t1.QuizTitle,
                                                 DueDateTime = t1.DueDateTime,
                                                 CourseChannelName = t2.Name
                                             }).OrderBy(o => o.DueDateTime).Take(5).ToList();
            foreach (var item in models.UpcomingExpiredQuizzes)
            {
                TimeSpan timeToEvent = item.DueDateTime.Value.Subtract(now);
                string remaining = string.Format("{0} Days, {1} Hours, {2} Minutes Left",
                               timeToEvent.Days, timeToEvent.Hours, timeToEvent.Minutes);
                item.TimeRemaining = remaining;
            }
            models.NewDiscussionTopics = (from t1 in db.Threads
                                          join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                          where t2.CreatorUserId == loginUserId
                                          select new ThreadViewModels
                                          {
                                              Id = t1.Id,
                                              ThreadTitle = t1.Title,
                                              Content = t1.Content,
                                              Name = db.AspNetUsers.Where(a => a.Id == t1.UserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                              ProfilePic = db.AspNetUsers.Where(a => a.Id == t1.UserId).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault(),
                                              LastUpdate = t1.LastUpdate,
                                              CourseChannelName = t2.Name
                                          }).OrderByDescending(o => o.LastUpdate).Take(5).ToList();
            models.ReceivedAssignments = (from t1 in db.AssignmentSubmissions
                                          join t2 in db.Assignments on t1.AssignmentId equals t2.Id
                                          join t3 in db.CourseChannels on t2.CourseChannelId equals t3.Id
                                          where t3.CreatorUserId == loginUserId
                                          select t1.Id).Count();
            models.GradedAssignments = (from t1 in db.AssignmentSubmissions
                                          join t2 in db.Assignments on t1.AssignmentId equals t2.Id
                                          join t3 in db.CourseChannels on t2.CourseChannelId equals t3.Id
                                          where t3.CreatorUserId == loginUserId && t1.Grade != null
                                          select t1.Id).Count();
            models.NonGradedAssignments = (from t1 in db.AssignmentSubmissions
                                          join t2 in db.Assignments on t1.AssignmentId equals t2.Id
                                          join t3 in db.CourseChannels on t2.CourseChannelId equals t3.Id
                                          where t3.CreatorUserId == loginUserId && t1.Grade == null
                                          select t1.Id).Count();
            models.OldestAssignmentsHaventGraded = (from t1 in db.AssignmentSubmissions
                                                    join t2 in db.Assignments on t1.AssignmentId equals t2.Id
                                                    join t3 in db.CourseChannels on t2.CourseChannelId equals t3.Id
                                                    where t3.CreatorUserId == loginUserId && t1.Grade == null
                                                    select new AssignmentSubmissionViewModels
                                                    {
                                                        Id = t1.Id,
                                                        AssignmentTitle = t2.Title,
                                                        AssignmentId = t2.Id,
                                                        GroupName = db.AssignmentGroups.Where(a => a.Id == t1.AssignmentGroupId).Select(a => a.GroupName).DefaultIfEmpty("").FirstOrDefault(),
                                                        StudentName = db.AspNetUsers.Where(a => a.Id == t1.StudentId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                                        SubmissionDate = t1.SubmissionDate
                                                    }).OrderBy(o => o.SubmissionDate).Take(5).ToList();
          
            return View(models);
        }

        [CustomAuthorize(Roles = "Student")]
        public ActionResult Student()
        {
            string loginUserId = User.Identity.GetUserId();
            StudentHomeDashboardViewModels models = new StudentHomeDashboardViewModels();
            DateTime now = (DateTime)generalFunction.GetSystemTimeZoneDateTimeNow();
            models.TotalCourseChannels = db.StudentCourseChannels.Where(a => a.StudentId == loginUserId).Count();
            models.TotalAssignment = (from t1 in db.StudentCourseChannels
                                      join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                      join t3 in db.Assignments on t2.Id equals t3.CourseChannelId
                                      where t1.StudentId == loginUserId
                                      select t3.Id).Count();
            models.TotalQuizzes = (from t1 in db.StudentCourseChannels
                                   join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                   join t3 in db.Quizzes on t2.Id equals t3.CourseChannelId
                                   where t1.StudentId == loginUserId
                                   select t3.Id).Count();
            models.HighestGradeAssignment = (from t1 in db.StudentCourseChannels
                                             join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                             join t3 in db.Assignments on t2.Id equals t3.CourseChannelId
                                             join t4 in db.AssignmentSubmissions on t3.Id equals t4.AssignmentId
                                             where t1.StudentId == loginUserId && t4.Grade != null
                                             select new AssignmentSubmissionViewModels
                                             {
                                                 Id = t4.Id,
                                                 AssignmentTitle = t3.Title,
                                                 Grade = t4.Grade,
                                                 CourseChannelName = t2.Name
                                             }).OrderByDescending(o => o.Grade).FirstOrDefault();
            if (models.HighestGradeAssignment == null)
            {
                models.HighestGradeAssignment = new AssignmentSubmissionViewModels();
                models.HighestGradeAssignment.Grade = 0;
                models.HighestGradeAssignment.AssignmentTitle = "-";
                models.HighestGradeAssignment.CourseChannelName = "-";
            }
            models.LowestGradeAssignment = (from t1 in db.StudentCourseChannels
                                             join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                             join t3 in db.Assignments on t2.Id equals t3.CourseChannelId
                                             join t4 in db.AssignmentSubmissions on t3.Id equals t4.AssignmentId
                                             where t1.StudentId == loginUserId && t4.Grade != null
                                             select new AssignmentSubmissionViewModels
                                             {
                                                 Id = t4.Id,
                                                 AssignmentTitle = t3.Title,
                                                 Grade = t4.Grade,
                                                 CourseChannelName = t2.Name
                                             }).OrderBy(o => o.Grade).FirstOrDefault();
            if (models.LowestGradeAssignment == null)
            {
                models.LowestGradeAssignment = new AssignmentSubmissionViewModels();
                models.LowestGradeAssignment.Grade = 0;
                models.LowestGradeAssignment.AssignmentTitle = "-";
                models.LowestGradeAssignment.CourseChannelName = "-";
            }
            models.UpcomingExpiredAssignments = (from t1 in db.Assignments
                                                 join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                                 join t3 in db.StudentCourseChannels on t2.Id equals t3.CourseChannelId
                                                 where t1.DueDate != null && t1.DueDate > now && t3.StudentId == loginUserId
                                                 select new AssignmentViewModels
                                                 {
                                                     Id = t1.Id,
                                                     AssignmentTitle = t1.Title,
                                                     DueDate = t1.DueDate,
                                                     CourseChannelName = t2.Name
                                                 }).OrderBy(o => o.DueDate).Take(5).ToList();
            foreach (var item in models.UpcomingExpiredAssignments)
            {
                TimeSpan timeToEvent = item.DueDate.Value.Subtract(now);
                string remaining = string.Format("{0} Days, {1} Hours, {2} Minutes Left",
                               timeToEvent.Days, timeToEvent.Hours, timeToEvent.Minutes);
                item.TimeRemaining = remaining;
            }
            models.UpcomingExpiredQuizzes = (from t1 in db.Quizzes
                                                 join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                                 join t3 in db.StudentCourseChannels on t2.Id equals t3.CourseChannelId
                                                 where t1.DueDateTime != null && t1.DueDateTime > now && t3.StudentId == loginUserId
                                             select new QuizViewModels
                                             {
                                                 Id = t1.Id,
                                                 QuizTitle = t1.QuizTitle,
                                                 DueDateTime = t1.DueDateTime,
                                                 CourseChannelName = t2.Name
                                             }).OrderBy(o => o.DueDateTime).Take(5).ToList();
            foreach (var item in models.UpcomingExpiredQuizzes)
            {
                TimeSpan timeToEvent = item.DueDateTime.Value.Subtract(now);
                string remaining = string.Format("{0} Days, {1} Hours, {2} Minutes Left",
                               timeToEvent.Days, timeToEvent.Hours, timeToEvent.Minutes);
                item.TimeRemaining = remaining;
            }
            return View(models);
        }

        public ActionResult Read_StudentEnrollment([DataSourceRequest] DataSourceRequest request)
        {
            List<CourseChannelViewModel> result = new List<CourseChannelViewModel>();
            string loginUserId = User.Identity.GetUserId();
            try
            {
                result = (from t1 in db.StudentCourseChannels
                          join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                          where t1.StudentId == loginUserId
                          select new CourseChannelViewModel
                          {
                              Id = t2.Id,
                              Name = t2.Name,
                              CreatorUserId = t2.CreatorUserId,
                              CreatorName = db.AspNetUsers.Where(a => a.Id == t2.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                              StudentCount = db.StudentCourseChannels.Where(a => a.CourseChannelId == t2.Id).Count(),
                          }).OrderBy(o => o.Name).ToList();
                foreach (var item in result)
                {

                    item.LastestAnnouncement = db.Announcements.Where(a => a.CourseChannelId == item.Id).OrderByDescending(o => o.CreatedOn).Take(1).FirstOrDefault();
                    if (item.LastestAnnouncement == null)
                    {
                        item.LastestAnnouncement = new Announcement();
                        item.LastestAnnouncement.Title = "";
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }
            return Json(result.ToDataSourceResult(request));
        }

    
        public ActionResult Read_NewThreads([DataSourceRequest] DataSourceRequest request)
        {
            List<ThreadViewModels> result = new List<ThreadViewModels>();
            string loginUserId = User.Identity.GetUserId();
            try
            {
                result = (from t1 in db.Threads
                          join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                          where t2.CreatorUserId == loginUserId
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
                              CourseChannelName = t2.Name,
                              CourseChannelId = t1.CourseChannelId
                          }).OrderByDescending(o => o.LastUpdate).ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

    }
}