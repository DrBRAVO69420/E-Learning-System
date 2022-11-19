using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNet.Identity;
using OnlineLearning.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace OnlineLearning.Controllers
{
    public class CourseChannelController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext(); 

        // GET: CourseChannel
        public ActionResult List(string search,string view)
        {
            string loginUserId = User.Identity.GetUserId();
            CourseChannelViewModel model = new CourseChannelViewModel();
            model.CoursesList = GetTeacherCourseChannelsList(search);
            if (!string.IsNullOrEmpty(view))
            {
                if (view.Equals("grid"))
                {
                    ViewBag.Grid = true;
                }
                if (view.Equals("list"))
                {
                    ViewBag.List = true;
                }
            }
            else
            {
                ViewBag.Grid = true; //default
            }
            return View(model);
        }

        [CustomAuthorize(Roles ="Student")]
        public ActionResult Enroll(Guid? Id)
        {
            EnrollLinkViewModel model = new EnrollLinkViewModel();
            if (Id != null && Id != Guid.Empty)
            {
                model.CourseChannelId = Id;
                model.CourseChannelName = db.CourseChannels.Where(a => a.Id == Id).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
            }
            return View(model);
        }

        //Id = course channel id
        public ActionResult StudentList(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            CourseChannelViewModel model = new CourseChannelViewModel();
            model.TopNavViewModels = generalFunction.GetTopNavViewModels(Id);
            return View(model);
        }

        public List<CourseChannelViewModel> GetTeacherCourseChannelsList(string search)
        {
            string loginUserId = User.Identity.GetUserId();
            List<CourseChannelViewModel> fullList = (from t1 in db.CourseChannels
                                                     where t1.CreatorUserId == loginUserId
                                                     select new CourseChannelViewModel
                                                     {
                                                         Id = t1.Id,
                                                         Description = t1.Description,
                                                         FormattedDescription = t1.Description,
                                                         ThemeId = t1.ThemeId,
                                                         Name = t1.Name,
                                                         FormattedName = t1.Name,
                                                         StudentCount = db.StudentCourseChannels.Where(a => a.CourseChannelId == t1.Id).Count(),
                                                         ImageSource = db.Themes.Where(a => a.Id == t1.ThemeId).Select(a => a.ImageSource).DefaultIfEmpty("").FirstOrDefault(),
                                                         CreatedOn = t1.CreatedOn,
                                                         CreatorUserId = t1.CreatorUserId,
                                                         CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                                         StudentList = db.StudentCourseChannels.Where(a => a.CourseChannelId == t1.Id).Select(a => a.StudentId).ToList(),
                                                         QuizCount = db.Quizzes.Where(a => a.CourseChannelId == t1.Id).Count(),
                                                         ChapterCount = db.Chapters.Where(a => a.CourseChannelId == t1.Id).Count(),
                                                         AssignmentCount = db.Assignments.Where(a => a.CourseChannelId == t1.Id).Count()
                                                         // CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                                                     }).OrderBy(o => o.Name).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                List<string> queryWords = new List<string>();
                foreach (string word in search.ToLower().Split(' '))
                {
                    queryWords.Add(word);
                }
                List<CourseChannelViewModel> filteredList = new List<CourseChannelViewModel>();
                foreach (var item in fullList)
                {
                    bool nameContainQueryWord = queryWords.Any(item.Name.ToLower().Contains);
                    bool descriptionContainQueryWord = queryWords.Any(item.Description.ToLower().Contains);
                    if (nameContainQueryWord || descriptionContainQueryWord)
                    {
                        filteredList.Add(item);
                    }
                }
                string formattedName = "";
                string formattedDescription = "";
                //bold and italic the matched words
                foreach (string word in search.ToLower().Split(' '))
                {
                    if (filteredList.Count > 0)
                    {
                        foreach (var item in filteredList)
                        {
                            formattedName = Regex.Replace(
                                item.FormattedName, // input
                                @word, // word to match
                                @"<b style='background-color:#ebe7c3'>$0</b>", // "wrap match in bold and italic tags"
                                RegexOptions.IgnoreCase); // ignore case when matching

                            formattedDescription = Regex.Replace(
                                item.FormattedDescription, // input
                                @word, // word to match
                                @"<b style='background-color:#ebe7c3'>$0</b>", // "wrap match in bold and italic tags"
                                RegexOptions.IgnoreCase); // ignore case when matching


                            item.FormattedName = formattedName;
                            item.FormattedDescription = formattedDescription;
                            formattedName = "";
                            formattedDescription = "";
                        }
                    }
                }
                return filteredList;
            }
            else
            {
                return fullList;
            }       
        }

        public CourseChannelViewModel GetCourseChannelViewModel(Guid? Id)
        {
            CourseChannel courseChannel = db.CourseChannels.Where(a => a.Id == Id).FirstOrDefault();
            CourseChannelViewModel model = new CourseChannelViewModel();
            model.Id = courseChannel.Id;
            model.Name = courseChannel.Name;
            model.Description = courseChannel.Description;
            model.CreatorName = db.AspNetUsers.Where(a => a.Id == courseChannel.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("N/A").FirstOrDefault();
            model.ImageSource = db.Themes.Where(a => a.Id == courseChannel.ThemeId).Select(a => a.ImageSource).DefaultIfEmpty("N/A").FirstOrDefault();
            model.StudentCount = db.StudentCourseChannels.Where(a => a.CourseChannelId == courseChannel.Id).Count();
            model.ThemeId = courseChannel.ThemeId;
            model.CreatorUserId = courseChannel.CreatorUserId;
            model.CreatedOn = courseChannel.CreatedOn;
            model.ChapterList = (from t1 in db.Chapters
                                 join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                                 where t2.Id == courseChannel.Id
                                 select new ChapterViewModels
                                 {
                                     Id = t1.Id,
                                     ChapterTitle = t1.Title,
                                     CourseChannelId = t1.CourseChannelId,
                                     CreatorId = t1.CreatorId,
                                     LastUpdate = t1.LastUpdate,
                                     CreatedOn = t1.CreatedOn,
                                     FileList = db.FileMaterials.Where(a => a.ChapterId == t1.Id).ToList(),
                                     TextList = db.TextMaterials.Where(a => a.ChapterId == t1.Id).ToList()
                                 }).OrderBy(o => o.CreatedOn).ToList();
            model.TopNavViewModels = generalFunction.GetTopNavViewModels(model.Id);
            
            model.LastestAnnouncement = db.Announcements.Where(a => a.CourseChannelId == Id).OrderByDescending(o => o.CreatedOn).FirstOrDefault();
            if (model.LastestAnnouncement == null)
            {
                model.LastestAnnouncement = new Announcement();
            }
            return model;
        }

        public ActionResult Manage(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            CourseChannelViewModel model = GetCourseChannelViewModel(Id);
            model.TopNavViewModels = generalFunction.GetTopNavViewModels(model.Id);
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditCourseChannel(CourseChannelViewModel model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                TempData["NameEmpty"] = "Name field is required.";
                ModelState.AddModelError("Name", "The Name field is required.");
            }
            if (string.IsNullOrEmpty(model.Description))
            {
                TempData["DescriptionEmpty"] = "Description field is required.";
                ModelState.AddModelError("Description", "The Description field is required.");
            }
            if (string.IsNullOrEmpty(model.Name) && string.IsNullOrEmpty(model.Description))
            {
                TempData["AllEmpty"] = "Name and Description field are required.";
            }
            if (model != null && ModelState.IsValid)
            {
                try
                {
                    CourseChannel courseChannel = new CourseChannel();
                    if (model.Id == null || model.Id == Guid.Empty)
                    {
                        courseChannel.Id = Guid.NewGuid();
                        courseChannel.CreatorUserId = User.Identity.GetUserId();
                        courseChannel.CreatedOn = generalFunction.GetSystemTimeZoneDateTimeNow();
                        Guid? defaultThemeCategoryId = db.ThemeCategories.Where(a => a.CategoryName == "Default").Select(a => a.CateogyId).FirstOrDefault();
                        courseChannel.ThemeId = db.Themes.Where(a => a.CategoryId == defaultThemeCategoryId).Select(a => a.Id).FirstOrDefault();//set a default theme when teacher first create the course channel, he or she can change the theme after that
                    }
                    else
                    {
                        courseChannel = db.CourseChannels.Where(a => a.Id == model.Id).FirstOrDefault();
                    }

                    courseChannel.Name = model.Name;
                    courseChannel.Description = model.Description;
                    if (model.Id == null || model.Id == Guid.Empty)
                    {
                        db.CourseChannels.Add(courseChannel);
                    }
                    else
                    {
                        db.Entry(courseChannel).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                   
                    if (model.Id == null || model.Id == Guid.Empty)
                    {
                        TempData["CourseChannelCreated"] = "Record updated successfully.";
                    }
                    else
                    {
                        TempData["CourseChannelUpdated"] = "Updated successfully.";
                    }
                    model.Id = courseChannel.Id;
                    ModelState.Clear();
                    return RedirectToAction("manage",new { Id= courseChannel.Id.ToString() });
                }
                catch (Exception ex)
                {
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        ModelState.AddModelError("", ex.Message);
                    }
                }
            }         
            model.CoursesList = GetTeacherCourseChannelsList("");
            if (model.Id == null || model.Id == Guid.Empty)
            {
                return View("list",model);
            }
            else
            {
                model = GetCourseChannelViewModel(model.Id);
                return View("manage",model);
            }
           
        }

        public ActionResult Read_CourseChannel([DataSourceRequest] DataSourceRequest request)
        {
            List<CourseChannelViewModel> result = new List<CourseChannelViewModel>();
            try
            {
                result = (from t1 in db.CourseChannels         
                          select new CourseChannelViewModel
                          {
                              Id = t1.Id,
                              Description = t1.Description,
                              ThemeId = t1.ThemeId,
                              Name = t1.Name,
                              StudentCount = db.StudentCourseChannels.Where(a => a.CourseChannelId == t1.Id).Count(),
                              ImageSource = db.Themes.Where(a => a.Id == t1.ThemeId).Select(a => a.ImageSource).DefaultIfEmpty("").FirstOrDefault(),
                              CreatedOn = t1.CreatedOn,
                              CreatorUserId = t1.CreatorUserId,
                             // CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                          }).OrderBy(o => o.Name).ToList();               
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        public ActionResult Read_Students([DataSourceRequest] DataSourceRequest request, Guid? Id)
        {
            List<UserViewModel> result = new List<UserViewModel>();
            try
            {
                result = (from t1 in db.CourseChannels
                          join t2 in db.StudentCourseChannels on t1.Id equals t2.CourseChannelId
                          join t3 in db.AspNetUsers on t2.StudentId equals t3.Id
                          where t1.Id == Id
                          select new UserViewModel
                          {
                              Id = t2.StudentId,
                              Name = t3.Name,
                              ProfilePic = t3.ProfilePicName,
                              Code = t3.Code
                          }).OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }


        [HttpPost]
        public ActionResult CreateChapter(CourseChannelViewModel model)
        {
            if (model !=null)
            {
                if (model.ChapterName == null || string.IsNullOrEmpty(model.ChapterName))
                {
                    TempData["ModalStateError"] = "Please enter a chapter title";
                }
                else
                {
                    try
                    {
                        Chapter chapter = new Chapter();
                        chapter.Id = Guid.NewGuid();
                        chapter.Title = model.ChapterName;
                        chapter.CreatorId = User.Identity.GetUserId();
                        chapter.CreatedOn = generalFunction.GetSystemTimeZoneDateTimeNow();
                        chapter.CourseChannelId = model.Id;
                        db.Chapters.Add(chapter);
                        db.SaveChanges();
                        ModelState.Clear();
                        TempData["ChapterCreated"] = "Created Successfully!";
                    }
                    catch (Exception ex)
                    {
                        while (ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                            ModelState.AddModelError("", ex.Message);
                        }
                    }
                }
            }
            
            return RedirectToAction("manage","coursechannel",new { Id = model.Id});
        }

        public ActionResult EditChapterTitle(Guid? ChosenChapterId, string ChapterTitleEdit, Guid? CourseChannelId)
        {
            if (ChosenChapterId != Guid.Empty && ChosenChapterId != null && !string.IsNullOrEmpty(ChapterTitleEdit))
            {
                try
                {
                    Chapter chapter = db.Chapters.Find(ChosenChapterId);
                    chapter.Title = ChapterTitleEdit;
                    chapter.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                    db.Entry(chapter).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["EditChapterSuccess"] = "Update Successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ModalStateError"] = ex.InnerException.Message;
                }
            }
            return RedirectToAction("manage", "coursechannel", new { Id = CourseChannelId });
        }

        // ---------------------------------------- Student ----------------------------------------

        [CustomAuthorize (Roles ="Student")]
        public ActionResult BrowseAll(string search, string view, bool? myenroll)
        {
            string loginUserId = User.Identity.GetUserId();
            CourseChannelViewModel model = new CourseChannelViewModel();
            if (myenroll == null)
            {
                myenroll = false;
            }
            model.CoursesList = GetAllCourseChannelsList(search, myenroll);
            model.MyEnrollCount = db.StudentCourseChannels.Where(a => a.StudentId == loginUserId).Count();
            model.TotalCount = db.CourseChannels.Count();
            if (!string.IsNullOrEmpty(view))
            {
                if (view.Equals("grid"))
                {
                    ViewBag.Grid = true;
                }
                if (view.Equals("list"))
                {
                    ViewBag.List = true;
                }
            }
            else
            {
                ViewBag.Grid = true; //default
            }
            return View(model);
        }

        public List<CourseChannelViewModel> GetFilteredList(string search, List<CourseChannelViewModel> fullList)
        {
            if (!string.IsNullOrEmpty(search))
            {
                List<string> queryWords = new List<string>();
                foreach (string word in search.ToLower().Split(' '))
                {
                    queryWords.Add(word);
                }
                List<CourseChannelViewModel> filteredList = new List<CourseChannelViewModel>();
                foreach (var item in fullList)
                {
                    bool nameContainQueryWord = queryWords.Any(item.Name.ToLower().Contains);
                    bool descriptionContainQueryWord = queryWords.Any(item.Description.ToLower().Contains);
                    if (nameContainQueryWord || descriptionContainQueryWord)
                    {
                        filteredList.Add(item);
                    }
                }
                string formattedName = "";
                string formattedDescription = "";
                //bold and italic the matched words
                foreach (string word in search.ToLower().Split(' '))
                {
                    if (filteredList.Count > 0)
                    {
                        foreach (var item in filteredList)
                        {
                            formattedName = Regex.Replace(
                                item.FormattedName, // input
                                @word, // word to match
                                @"<b style='background-color:#ebe7c3'>$0</b>", // "wrap match in bold and italic tags"
                                RegexOptions.IgnoreCase); // ignore case when matching

                            formattedDescription = Regex.Replace(
                                item.FormattedDescription, // input
                                @word, // word to match
                                @"<b style='background-color:#ebe7c3'>$0</b>", // "wrap match in bold and italic tags"
                                RegexOptions.IgnoreCase); // ignore case when matching


                            item.FormattedName = formattedName;
                            item.FormattedDescription = formattedDescription;
                            formattedName = "";
                            formattedDescription = "";
                        }
                    }
                }
                return filteredList;
            }
            else
            {
                return fullList;
            }
        }

        public List<CourseChannelViewModel> GetAllCourseChannelsList(string search, bool? myenroll)
        {
            string loginUserId = User.Identity.GetUserId();
            List<CourseChannelViewModel> fullList = new List<CourseChannelViewModel>();
            if (myenroll == true)
            {
                fullList = (from t1 in db.CourseChannels
                            join t2 in db.StudentCourseChannels on t1.Id equals t2.CourseChannelId
                            where t2.StudentId == loginUserId
                            select new CourseChannelViewModel
                            {
                                Id = t1.Id,
                                Description = t1.Description,
                                FormattedDescription = t1.Description,
                                ThemeId = t1.ThemeId,
                                Name = t1.Name,
                                FormattedName = t1.Name,
                                StudentCount = db.StudentCourseChannels.Where(a => a.CourseChannelId == t1.Id).Count(),
                                StudentList = db.StudentCourseChannels.Where(a => a.CourseChannelId == t1.Id).Select(a => a.StudentId).ToList(),
                                ImageSource = db.Themes.Where(a => a.Id == t1.ThemeId).Select(a => a.ImageSource).DefaultIfEmpty("").FirstOrDefault(),
                                CreatedOn = t1.CreatedOn,
                                CreatorUserId = t1.CreatorUserId,
                                CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                QuizCount = db.Quizzes.Where(a => a.CourseChannelId == t1.Id).Count(),
                                ChapterCount = db.Chapters.Where(a => a.CourseChannelId == t1.Id).Count(),
                                AssignmentCount = db.Assignments.Where(a => a.CourseChannelId == t1.Id).Count()
                                // CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                            }).OrderBy(o => o.Name).ToList();
            }
            else
            {
                fullList = (from t1 in db.CourseChannels
                            select new CourseChannelViewModel
                            {
                                Id = t1.Id,
                                Description = t1.Description,
                                FormattedDescription = t1.Description,
                                ThemeId = t1.ThemeId,
                                Name = t1.Name,
                                FormattedName = t1.Name,
                                StudentCount = db.StudentCourseChannels.Where(a => a.CourseChannelId == t1.Id).Count(),
                                StudentList = db.StudentCourseChannels.Where(a => a.CourseChannelId == t1.Id).Select(a => a.StudentId).ToList(),
                                ImageSource = db.Themes.Where(a => a.Id == t1.ThemeId).Select(a => a.ImageSource).DefaultIfEmpty("").FirstOrDefault(),
                                CreatedOn = t1.CreatedOn,
                                CreatorUserId = t1.CreatorUserId,
                                CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                QuizCount = db.Quizzes.Where(a => a.CourseChannelId == t1.Id).Count(),
                                ChapterCount = db.Chapters.Where(a => a.CourseChannelId == t1.Id).Count(),
                                AssignmentCount = db.Assignments.Where(a => a.CourseChannelId == t1.Id).Count()
                                // CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault()
                            }).OrderBy(o => o.Name).ToList();
            }
            
            return GetFilteredList(search,fullList);
        }

        [CustomAuthorize(Roles = "Student")]
        public string EnrollInCourse(Guid Id)
        {
            string error = "";
            string loginUserId = User.Identity.GetUserId();
            if (Id != null && Id != Guid.Empty)
            {
                StudentCourseChannel studentCourse = db.StudentCourseChannels.Where(a => a.CourseChannelId == Id && a.StudentId == loginUserId).FirstOrDefault();
                if (studentCourse == null)
                {
                    StudentCourseChannel studentCourseChannel = new StudentCourseChannel();
                    studentCourseChannel.CourseChannelId = Id;
                    studentCourseChannel.StudentId = User.Identity.GetUserId();
                    db.StudentCourseChannels.Add(studentCourseChannel);
                    db.SaveChanges();
                }
            }
            return error;
        }

        [CustomAuthorize(Roles = "Teacher, Student")]
        public string UnEnroll(string studentId, Guid? courseChannelId)
        {
            string error = "";
            if (courseChannelId != null && courseChannelId != Guid.Empty)
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    studentId = User.Identity.GetUserId();
                }
                try
                {
                    List<StudentQuiz> studentQuizzes = (from t1 in db.StudentQuizzes
                                                        join t2 in db.Quizzes on t1.QuizId equals t2.Id
                                                        where t1.StudentId == studentId && t2.CourseChannelId == courseChannelId
                                                        select t1).ToList();
                    foreach (var quiz in studentQuizzes)
                    {
                        List<QuizQuestion> quizQuestions = db.QuizQuestions.Where(a => a.QuizId == quiz.QuizId).ToList();
                        foreach (var question in quizQuestions)
                        {
                            StudentQuizAnswer studentQuizAnswer = db.StudentQuizAnswers.Where(a => a.QuestionId == question.Id && a.StudentId == quiz.StudentId).FirstOrDefault();
                            db.StudentQuizAnswers.Remove(studentQuizAnswer);
                            db.SaveChanges();
                        }

                        StudentQuiz studentQuiz = db.StudentQuizzes.Where(a => a.Id == quiz.Id).FirstOrDefault();
                        db.StudentQuizzes.Remove(studentQuiz);
                        db.SaveChanges();
                    }
                    List<StudentAssignmentGroup> studentAssignmentGroups = db.StudentAssignmentGroups.Where(a => a.StudentId == studentId).ToList();
                    foreach (var group in studentAssignmentGroups)
                    {
                        StudentAssignmentGroup studentAssignmentGroup = db.StudentAssignmentGroups.Where(a => a.GroupId == group.GroupId).FirstOrDefault();
                        db.StudentAssignmentGroups.Remove(studentAssignmentGroup);
                        db.SaveChanges();
                    }
                    List<AssignmentSubmission> assignmentSubmissions = db.AssignmentSubmissions.Where(a => a.StudentId == studentId).ToList();
                    foreach (var submission in assignmentSubmissions)
                    {
                        AssignmentSubmission assignmentSubmission = db.AssignmentSubmissions.Where(a => a.Id == submission.Id).FirstOrDefault();
                        db.AssignmentSubmissions.Remove(assignmentSubmission);
                        db.SaveChanges();
                    }
                    List<Thread> threads = db.Threads.Where(a => a.UserId == studentId).ToList();
                    foreach (var item in threads)
                    {
                        Thread thread = db.Threads.Where(a => a.Id == item.Id).FirstOrDefault();
                        db.Threads.Remove(thread);
                        db.SaveChanges();
                    }
                    List<Post> posts = db.Posts.Where(a => a.UserId == studentId).ToList();
                    foreach (var item in posts)
                    {
                        Post post = db.Posts.Where(a => a.Id == item.Id).FirstOrDefault();
                        db.Posts.Remove(post);
                        db.SaveChanges();
                    }
                    //delete from student course channel
                    StudentCourseChannel studentCourseChannel = db.StudentCourseChannels.Where(a => a.StudentId == studentId && a.CourseChannelId == courseChannelId).FirstOrDefault();
                    db.StudentCourseChannels.Remove(studentCourseChannel);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message;
                }
            }
            return error;
        }

        public string Delete_CourseChannel(Guid? Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    //delete assignment file from server
                    List<Assignment> assignments = db.Assignments.Where(a => a.CourseChannelId == Id).ToList();
                    foreach (var item in assignments)
                    {
                        if (item.FileName != null)
                        {
                            //delete attached file from server
                            string fullPath = Request.MapPath("~/UploadedFiles/" + item.FileName);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                        }
                        List<AssignmentSubmission> assignmentSubmissions = db.AssignmentSubmissions.Where(a => a.AssignmentId == item.Id).ToList();
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
                    List<Chapter> chapters = db.Chapters.Where(a => a.CourseChannelId == Id).ToList();
                    foreach (var item in chapters)
                    {
                        List<FileMaterial> fileMaterials = db.FileMaterials.Where(a => a.ChapterId == item.Id).ToList();
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
                    List<Quiz> quizzes = db.Quizzes.Where(a => a.CourseChannelId == Id).ToList();
                    foreach (var item in quizzes)
                    {
                        List<QuizQuestion> quizQuestions = db.QuizQuestions.Where(a => a.QuizId == item.Id).ToList();
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
                    //as set in sql query, delete CourseChannel will also delete Chapter, Announcement, StudentCourseChannel, Assignment, Quiz, Thread
                    CourseChannel courseChannel = db.CourseChannels.Where(a => a.Id == Id).FirstOrDefault();
                    db.CourseChannels.Remove(courseChannel);
                    db.SaveChanges();
                    
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message;
                }
            }
            return error;
        }

        public JsonResult GetStudentInCourse(Guid? courseChannelId)
        {
            var result = (from t1 in db.StudentCourseChannels
                          join t2 in db.AspNetUsers on t1.StudentId equals t2.Id
                          where t1.CourseChannelId == courseChannelId
                          select new
                          {
                              Text = t2.Name,
                              Value = t2.Id
                          }).OrderBy(o => o.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SelectCoverImage(string Id)
        {
            ThemeViewModel model = new ThemeViewModel();
            Guid? courseId = Guid.Parse(Id);
            model.CourseChannelId = courseId;
            model.CourseChannelName = db.CourseChannels.Where(a => a.Id == courseId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
            model.CategoryList = db.ThemeCategories.Where(a => a.CategoryName != "Default").OrderBy(o => o.CategoryName).ToList();
            model.ThemeList = (from t1 in db.Themes
                               join t2 in db.ThemeCategories on t1.CategoryId equals t2.CateogyId
                               where t2.CategoryName != "Default"
                               select new ThemeViewModel
                               {
                                   Id = t1.Id,
                                   CategoryName = t2.CategoryName,
                                   ImageSource = t1.ImageSource
                               }).ToList();
            return View(model);
        }

        public string ChangeCoverImage(Guid? courseChannelId, Guid? themeId)
        {
            string error = "";
            try
            {
                CourseChannel courseChannel = db.CourseChannels.Where(a => a.Id == courseChannelId).FirstOrDefault();
                courseChannel.ThemeId = themeId;
                db.Entry(courseChannel).State = EntityState.Modified;
                db.SaveChanges();
            }catch(Exception ex)
            {
                error = ex.InnerException.Message;
            }
            return error;

        }
    }
}