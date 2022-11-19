using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNet.Identity;
using OnlineLearning.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineLearning.Controllers
{
    public class AssignmentController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext();

        //id = course channel id
        public ActionResult List(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            AssignmentViewModels models = new AssignmentViewModels();
            if (Id != null && Id != Guid.Empty)
            {
                models.CourseChannelId = Id;
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(Id);
            }
            return View(models);
        }

        //Id = course channel id
        public List<AssignmentViewModels> GetAssignmentList(Guid? Id)
        {
            List<AssignmentViewModels> list = new List<AssignmentViewModels>();
            list = (from t1 in db.Assignments
                    join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                    where t2.Id == Id
                    select new AssignmentViewModels
                    {
                        Id = t1.Id,
                        CourseChannelId = t2.Id,
                        AssignmentTitle = t1.Title,
                        Instruction = t1.Instruction,
                        DueDate = t1.DueDate,
                        FileName = t1.FileName,
                        TeacherId = t1.TeacherId,
                        LastUpdate = t1.LastUpdate,
                        CourseChannelName = t2.Name,
                        MemberMaxCount = t1.MemberMaxCount,
                        Type = t1.Type
                    }).OrderBy(o => o.LastUpdate).ToList();
            return list;

        }

        public ActionResult EditAssignment(Guid? courseChannelId, Guid? assignmentId)
        {
            string loginUserId = User.Identity.GetUserId();
            AssignmentViewModels models = new AssignmentViewModels();
            if (assignmentId != null && assignmentId != Guid.Empty)
            {
                models = GetAssignmentViewModels(assignmentId);
            }
            if (courseChannelId != null && courseChannelId != Guid.Empty)
            {
                models.CourseChannelId = courseChannelId;
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(courseChannelId);
            }
            return View(models);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditAssignment(AssignmentViewModels models, IEnumerable<HttpPostedFileBase> attachments)
        {
            string loginUserId = User.Identity.GetUserId();
            string uploadedFileName = "";
            if (models != null)
            {
                if (models.Type == null)
                {
                    ModelState.AddModelError("Type", "Type is required.");
                }
                else if (models.Type.Equals("Group"))
                {
                    if (models.MemberMaxCount == null || models.MemberMaxCount == 0)
                    {
                        ModelState.AddModelError("MemberMaxCount", "Maximum Number of Member in a Group is required.");
                    }
                }
                if (ModelState.IsValid)
                {
                    Assignment assignment = new Assignment();
                    if (models.Id == null || models.Id == Guid.Empty) //new record
                    {
                        assignment.Id = Guid.NewGuid();
                        assignment.CourseChannelId = models.CourseChannelId;
                        assignment.TeacherId = User.Identity.GetUserId();
                    }
                    else //update record
                    {
                        assignment = db.Assignments.Find(models.Id);
                    }

                    assignment.Title = models.AssignmentTitle;
                    assignment.Instruction = models.Instruction;
                    assignment.DueDate = models.DueDate;
                    assignment.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                    assignment.Type = models.Type;
                    assignment.MemberMaxCount = models.MemberMaxCount;
                    //save attachment
                    if (attachments != null)
                    {
                        foreach (var file in attachments)
                        {
                            if (file != null)
                            {
                                // get the file name of the selected image file and save into Files folder
                                uploadedFileName = new FileInfo(file.FileName).Name;
                                string serverFilePath = HttpContext.Server.MapPath("~/UploadedFiles/") + uploadedFileName;
                                file.SaveAs(serverFilePath); //remember to set 'write' permission on MaterialFiles folder in the server 
                                assignment.FileName = uploadedFileName;
                            }
                        }
                    }

                    if (models.Id == null || models.Id == Guid.Empty) //new record
                    {
                        db.Assignments.Add(assignment);
                    }
                    else //update record
                    {
                        db.Entry(assignment).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    ModelState.Clear();
                    TempData["AssignmentUpdated"] = "Updated Successfully!";
                    return RedirectToAction("list", new { Id = models.CourseChannelId });
                }
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(models.CourseChannelId);
            }
            return View(models);
        }

        public string RemoveDueDate(Guid? Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    Assignment assignment = db.Assignments.Find(Id);
                    assignment.DueDate = null;
                    db.Entry(assignment).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message;
                }
            }
            else
            {
                error = "Please select an assignment";
            }
            return error;
        }

        public string LeaveGroup(Guid? Id)
        {
            string error = "";
            string loginUserId = User.Identity.GetUserId();
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    //if this is the last and the only one student in the group who leave the group, delete the assignment group
                    int? groupmember = db.StudentAssignmentGroups.Where(a => a.GroupId == Id).Count();
                    if (groupmember == 1)
                    {
                        //delete group (AssignmentGroup table) will also delete members in the group (StudentAssignmentGroup table)
                        AssignmentGroup assignmentGroup = db.AssignmentGroups.Where(a => a.Id == Id).FirstOrDefault();
                        db.AssignmentGroups.Remove(assignmentGroup);
                        db.SaveChanges();
                    }
                    else
                    {
                        StudentAssignmentGroup studentAssignmentGroup = db.StudentAssignmentGroups.Where(a => a.GroupId == Id && a.StudentId == loginUserId).FirstOrDefault();
                        db.StudentAssignmentGroups.Remove(studentAssignmentGroup);
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message.ToString() == null ? "Database Query Error" : ex.InnerException.Message.ToString();
                }
            }
            else
            {
                error = "You didn't join any group";
            }
            return error;
        }

        public string Delete_Assignment(Guid Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    //as set in sql query, delete Assignment will also delete AssignmentGroup, StudentAssignmentGroup, AssignmentSubmission
                    Assignment assignment = db.Assignments.Where(a => a.Id == Id).FirstOrDefault();
                    if (assignment.FileName != null)
                    {
                        //delete attached file from server
                        string fullPath = Request.MapPath("~/UploadedFiles/" + assignment.FileName);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
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
                    //delete data from database
                    db.Assignments.Remove(assignment);
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

        public AssignmentViewModels GetAssignmentViewModels(Guid? Id)
        {
            AssignmentViewModels models = new AssignmentViewModels();
            string loginUserId = User.Identity.GetUserId();
            Assignment assignment = db.Assignments.Find(Id);
            models.Id = assignment.Id;
            models.CourseChannelId = assignment.CourseChannelId;
            models.AssignmentTitle = assignment.Title;
            models.Instruction = assignment.Instruction;
            models.DueDate = assignment.DueDate;
            models.AlreadyDue = false;
            if (assignment.DueDate!=null)
            {
                if (generalFunction.GetSystemTimeZoneDateTimeNow() > assignment.DueDate )
                {
                    models.AlreadyDue = true;
                }
                else
                {
                    models.AlreadyDue = false;
                }
                TimeSpan timeToEvent = assignment.DueDate.Value.Subtract((DateTime)generalFunction.GetSystemTimeZoneDateTimeNow());
                string remaining = string.Format("{0} Days, {1} Hours, {2} Minutes",
                                   timeToEvent.Days, timeToEvent.Hours, timeToEvent.Minutes);
                models.TimeRemaining = remaining;
            }
            else
            {
                models.TimeRemaining = "-";
            }
            
            models.FileName = assignment.FileName; //only allow one file, can upload a zip file if there is many files
            models.TeacherId = assignment.TeacherId;
            models.LastUpdate = assignment.LastUpdate;
            models.MemberMaxCount = assignment.MemberMaxCount;
            models.Type = assignment.Type == null ? "Individual" : assignment.Type; //default type = individual
            models.CourseChannelName = db.CourseChannels.Where(a => a.Id == assignment.CourseChannelId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
            models.TopNavViewModels = generalFunction.GetTopNavViewModels(assignment.CourseChannelId);
            models.AssignmentGroupViewModels = new AssignmentGroupViewModels();
            if (User.IsInRole("Student") || User.IsInRole("Teacher"))
            {
                models.AssignmentGroupViewModels.Id = db.StudentAssignmentGroups.Where(a => a.StudentId == loginUserId).Select(a => a.GroupId).DefaultIfEmpty(Guid.Empty).FirstOrDefault();
                if (models.AssignmentGroupViewModels.Id != Guid.Empty)
                {
                    models.AssignmentGroupViewModels.GroupName = db.AssignmentGroups.Where(a => a.Id == models.AssignmentGroupViewModels.Id).Select(a => a.GroupName).DefaultIfEmpty("").FirstOrDefault();
                }
                models.AssignmentSubmissionViewModels = new AssignmentSubmissionViewModels();
                AssignmentSubmission assignmentSubmission = new AssignmentSubmission();
                if (models.Type.Equals("Group"))
                {
                    assignmentSubmission = db.AssignmentSubmissions.Where(a => a.AssignmentId == models.Id && a.AssignmentGroupId == models.AssignmentGroupViewModels.Id).FirstOrDefault();
                }
                else
                {
                    assignmentSubmission = db.AssignmentSubmissions.Where(a => a.AssignmentId == models.Id && a.StudentId == loginUserId).FirstOrDefault();
                }
                if (assignmentSubmission != null)
                {
                    models.AssignmentSubmissionViewModels.Id = assignmentSubmission.Id;
                    //models.AssignmentSubmissionViewModels.Status = "Submitted for Grading";
                    DateTime? duedate = db.Assignments.Where(a => a.Id == models.Id).Select(a => a.DueDate).FirstOrDefault();
                    if (duedate != null)
                    {
                        TimeSpan timeToEvent = duedate.Value.Subtract(assignmentSubmission.SubmissionDate.Value);
                        string submittedFrom = string.Format("Submitted {0} Days, {1} Hours, {2} Minutes Early",
                                       timeToEvent.Days, timeToEvent.Hours, timeToEvent.Minutes);
                        models.AssignmentSubmissionViewModels.Status = submittedFrom;
                    }
                    models.AssignmentSubmissionViewModels.Grade = assignmentSubmission.Grade;
                    models.AssignmentSubmissionViewModels.SubmissionDate = assignmentSubmission.SubmissionDate;
                    models.AssignmentSubmissionViewModels.SubmissionFile = assignmentSubmission.SubmissionFile;
                    models.AssignmentSubmissionViewModels.Comment = assignmentSubmission.Comment;
                    models.AssignmentSubmissionViewModels.LastUpdate = assignmentSubmission.LastUpdate;
                }
            }

            return models;
        }

        public ActionResult Read_Assignment([DataSourceRequest] DataSourceRequest request, Guid CourseChannelId)
        {
            List<AssignmentViewModels> result = new List<AssignmentViewModels>();
            string studentId = User.Identity.GetUserId();
            try
            {
                result = (from t1 in db.Assignments
                          join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                          where t2.Id == CourseChannelId
                          select new AssignmentViewModels
                          {
                              Id = t1.Id,
                              CourseChannelId = t2.Id,
                              AssignmentTitle = t1.Title,
                              Instruction = t1.Instruction,
                              DueDate = t1.DueDate,
                              FileName = t1.FileName,
                              TeacherId = t1.TeacherId,
                              LastUpdate = t1.LastUpdate,
                              CourseChannelName = t2.Name,
                              MemberMaxCount = t1.MemberMaxCount,
                              Type = t1.Type,
                              SubmissionStatus = db.AssignmentSubmissions.Where(a => a.AssignmentId == t1.Id && a.StudentId == studentId).Any() ? "Submitted" : "Not Yet Submit"
                          }).OrderBy(o => o.LastUpdate).ToList();
                foreach (var item in result)
                {
                    if (item.DueDate != null)
                    {
                        DateTime duedate = (DateTime)item.DueDate;
                        item.strDueDate = duedate.ToString("yyyy-MM-dd hh:mm tt");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        public ActionResult Read_AssignmentSubmission([DataSourceRequest] DataSourceRequest request, Guid AssignmentId)
        {
            List<AssignmentSubmissionViewModels> result = new List<AssignmentSubmissionViewModels>();
            try
            {
                string assignmentType = db.Assignments.Where(a => a.Id == AssignmentId).Select(a => a.Type).DefaultIfEmpty("").FirstOrDefault();
                if (assignmentType.Equals("Group"))
                {
                    result = (from t1 in db.AssignmentSubmissions
                              join t2 in db.AssignmentGroups on t1.AssignmentGroupId equals t2.Id
                              where t1.AssignmentId == AssignmentId
                              select new AssignmentSubmissionViewModels
                              {
                                  Id = t1.Id,
                                  AssignmentId = t1.AssignmentId,
                                  Comment = t1.Comment,
                                  SubmissionDate = t1.SubmissionDate,
                                  SubmissionFile = t1.SubmissionFile,
                                  TeacherId = t1.TeacherId,
                                  LastUpdate = t1.LastUpdate,
                                  GroupName = t2.GroupName,
                                  Grade = t1.Grade ,
                                  AssignmentGroupId = t2.Id
                              }).OrderBy(o => o.LastUpdate).ToList();         
                }
                else
                {
                    result = (from t1 in db.AssignmentSubmissions
                              join t2 in db.AspNetUsers on t1.StudentId equals t2.Id
                              where t1.AssignmentId == AssignmentId
                              select new AssignmentSubmissionViewModels
                              {
                                  Id = t1.Id,
                                  AssignmentId = t1.AssignmentId,
                                  Comment = t1.Comment,
                                  SubmissionDate = t1.SubmissionDate,
                                  SubmissionFile = t1.SubmissionFile,
                                  TeacherId = t1.TeacherId,
                                  LastUpdate = t1.LastUpdate,
                                  Grade = t1.Grade,
                                  StudentId = t1.StudentId,
                                  StudentName = t2.Name
                              }).OrderBy(o => o.LastUpdate).ToList();
                }
                foreach (var item in result)
                {
                    DateTime? duedate = db.Assignments.Where(a => a.Id == item.AssignmentId).Select(a => a.DueDate).FirstOrDefault();
                    if (duedate != null)
                    {
                        TimeSpan timeToEvent = duedate.Value.Subtract(item.SubmissionDate.Value);
                        string submittedFrom = string.Format("Submitted {0} Days, {1} Hours, {2} Minutes Early",
                                       timeToEvent.Days, timeToEvent.Hours, timeToEvent.Minutes);
                        item.SubmittedFromDueDate = submittedFrom;
                    }
                    if (item.LastUpdate != null)
                    {
                        DateTime lastupdate = (DateTime)item.LastUpdate;
                        item.strLastUpdate = lastupdate.ToString("yyyy-MM-dd hh:mm tt");
                    }
                    if (item.SubmissionDate != null)
                    {
                        DateTime submissiondate = (DateTime)item.SubmissionDate;
                        item.strSubmissionDate = submissiondate.ToString("yyyy-MM-dd hh:mm tt");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [CustomAuthorize(Roles = "Teacher")]
        public ActionResult Update_AssignmentGrade([DataSourceRequest] DataSourceRequest request, AssignmentSubmissionViewModels models)
        {
            if (models != null && ModelState.IsValid)
            {
                try
                {
                    AssignmentSubmission assignmentSubmission = db.AssignmentSubmissions.Find(models.Id);
                    assignmentSubmission.Grade = models.Grade;
                    assignmentSubmission.Comment = models.Comment;
                    db.Entry(assignmentSubmission).State = EntityState.Modified;
                    db.SaveChanges();
                    ModelState.Clear();
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
            return Json(new[] { models }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetGroupMembers(Guid Id)
        {
            List<string> result = new List<string>();
            result = (from t1 in db.AssignmentGroups
                      join t2 in db.StudentAssignmentGroups on t1.Id equals t2.GroupId
                      join t3 in db.AspNetUsers on t2.StudentId equals t3.Id
                      where t1.Id == Id
                      select t3.Name).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //validate whether uploaded attachments file name already exists in the db
        public string ValidateUpload(string currentFileName)
        {
            List<string> names = new List<string>();
            names = db.Assignments.Select(a => a.FileName).ToList();
            string errorMsg = "";
            if (names.Contains(currentFileName))
            {
                errorMsg = "The file " + currentFileName + " already exists in the system. Please rename your file and try again.";
                return errorMsg;
            }
            return null;
        }

        // ----------------------------------- Student -----------------------------------
        //Id = Assignment Id
        public ActionResult Detail(Guid? Id)
        {
            AssignmentViewModels models = new AssignmentViewModels();
            if (Id != null && Id != Guid.Empty)
            {
                models = GetAssignmentViewModels(Id);
            }
            return View(models);
        }

        public ActionResult ManageGroupMembers(Guid? Id)
        {
            AssignmentViewModels models = new AssignmentViewModels();
            if (Id != null && Id != Guid.Empty)
            {
                models = GetAssignmentViewModels(Id);
            }
            return View(models);
        }

        public JsonResult GetAvailableStudentInCourse(Guid? courseChannelId)
        {
            var result = (from t1 in db.StudentCourseChannels
                          join t2 in db.AspNetUsers on t1.StudentId equals t2.Id
                          where t1.CourseChannelId == courseChannelId && db.StudentAssignmentGroups.Where(a => a.StudentId == t1.StudentId).Any() == false
                          select new
                          {
                              Text = t2.Name,
                              Value = t2.Id
                          }).OrderBy(o => o.Text).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult ManageGroupMembers(AssignmentViewModels models)
        {
            if (models != null)
            {
                //check duplicate group name
                bool groupNameExist = db.AssignmentGroups.Where(a => a.GroupName == models.AssignmentGroupViewModels.GroupName).Any();
                if (groupNameExist)
                {
                    ModelState.AddModelError("AssignmentGroupViewModels.GroupName", "Group Name already exists. Please try with another name.");
                }
                if (models.AssignmentGroupViewModels.StudentIdList.Count == 1)
                {
                    ModelState.AddModelError("AssignmentGroupViewModels.StudentIdList", "Please choose a minimum of 2 members.");
                }
                if (ModelState.IsValid)
                {
                    //add assignment group in db
                    AssignmentGroup assignmentGroup = new AssignmentGroup();
                    assignmentGroup.Id = Guid.NewGuid();
                    assignmentGroup.AssignmentId = models.Id;
                    assignmentGroup.GroupName = models.AssignmentGroupViewModels.GroupName;
                    db.AssignmentGroups.Add(assignmentGroup);
                    db.SaveChanges();

                    //add students in StudentAssignmentGroup table in db
                    models.AssignmentGroupViewModels.StudentList = new List<AspNetUsers>();
                    foreach (var studentId in models.AssignmentGroupViewModels.StudentIdList)
                    {
                        StudentAssignmentGroup studentAssignmentGroup = new StudentAssignmentGroup();
                        studentAssignmentGroup.StudentId = studentId;
                        studentAssignmentGroup.GroupId = assignmentGroup.Id;
                        db.StudentAssignmentGroups.Add(studentAssignmentGroup);
                        db.SaveChanges();
                        ModelState.Clear();
                        AspNetUsers users = db.AspNetUsers.Find(studentId);
                        models.AssignmentGroupViewModels.StudentList.Add(users);
                        
                    }
                    TempData["GroupCreated"] = "Group Created Successfully!";
                    return RedirectToAction("managegroupmembers", new { Id = models.Id });
                }
            }
            models = GetAssignmentViewModels(models.Id);
            models.AssignmentGroupViewModels = models.AssignmentGroupViewModels;
            return View(models);
        }

        public ActionResult Read_AssignmentGroup([DataSourceRequest] DataSourceRequest request, Guid AssignmentId)
        {
            List<AssignmentGroupViewModels> result = new List<AssignmentGroupViewModels>();
            try
            {
                result = (from t1 in db.Assignments
                          join t2 in db.AssignmentGroups on t1.Id equals t2.AssignmentId
                          join t3 in db.StudentAssignmentGroups on t2.Id equals t3.GroupId
                          where t1.Id == AssignmentId
                          select new AssignmentGroupViewModels
                          {
                              Id = t2.Id,
                              GroupName = t2.GroupName,
                              StudentId = t3.StudentId,
                              //StudentList = db.AspNetUsers.Where(a => a.Id == t3.StudentId).ToList(),
                              StudentName = db.AspNetUsers.Where(a => a.Id == t3.StudentId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                              ProfilePic = db.AspNetUsers.Where(a => a.Id == t3.StudentId).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault()
                          }).OrderBy(o => o.GroupName).ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        public ActionResult Submission(Guid? AssignmentId, Guid? Id)
        {
            AssignmentSubmissionViewModels models = new AssignmentSubmissionViewModels();
            string loginUserId = User.Identity.GetUserId();
            if (AssignmentId!=null && AssignmentId != Guid.Empty)
            {
                models.AssignmentId = AssignmentId;
                models.AssignmentType = db.Assignments.Where(a => a.Id == AssignmentId).Select(a => a.Type).DefaultIfEmpty("").FirstOrDefault();
                models.AssignmentTitle = db.Assignments.Where(a => a.Id == AssignmentId).Select(a => a.Title).DefaultIfEmpty("").FirstOrDefault();
                Guid? courseChannelId = db.Assignments.Where(a => a.Id == AssignmentId).Select(a => a.CourseChannelId).FirstOrDefault();
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(courseChannelId);
                models.AssignmentGroupId = db.StudentAssignmentGroups.Where(a => a.StudentId == loginUserId).Select(a => a.GroupId).DefaultIfEmpty(Guid.Empty).FirstOrDefault();   
            }

            if (Id != null && Id != Guid.Empty)
            {
                AssignmentSubmission assignmentSubmission = db.AssignmentSubmissions.Find(Id);
                models.Id = assignmentSubmission.Id;
                models.SubmissionFile = assignmentSubmission.SubmissionFile;
                models.LastUpdate = assignmentSubmission.LastUpdate;
                models.AssignmentGroupId = assignmentSubmission.AssignmentGroupId;
                models.Comment = assignmentSubmission.Comment;
            }
            return View(models);
        }

        [HttpPost]
        public ActionResult Submission(AssignmentSubmissionViewModels models, IEnumerable<HttpPostedFileBase> attachments)
        {
            if (models!=null)
            {
                //if the assignment is a group assignment, check whether user have a group
                string loginUserId = User.Identity.GetUserId();
                if (models.AssignmentType.Equals("Group"))
                {
                    bool userHasGroup = db.StudentAssignmentGroups.Where(a => a.StudentId == loginUserId).Any();
                    if (!userHasGroup)
                    {
                        ModelState.AddModelError("Id", "You must have a group in order to submit this assignment.");
                        TempData["NoGroupError"] = "You must have a group in order to submit this assignment.";
                    }
                }
                if (ModelState.IsValid)
                {
                    AssignmentSubmission assignmentSubmission = new AssignmentSubmission();
                    string FileName = "";
                    
                    if (models.Id == null || models.Id == Guid.Empty) //new record
                    {
                        assignmentSubmission.Id = Guid.NewGuid();
                        assignmentSubmission.SubmissionDate = generalFunction.GetSystemTimeZoneDateTimeNow();
                        assignmentSubmission.TeacherId = db.Assignments.Where(a => a.Id == models.AssignmentId).Select(a => a.TeacherId).DefaultIfEmpty("").FirstOrDefault();
                        assignmentSubmission.AssignmentId = models.AssignmentId;
                        if (models.AssignmentType.Equals("Group"))
                        {
                            assignmentSubmission.AssignmentGroupId = models.AssignmentGroupId;
                        }
                        else
                        {
                            assignmentSubmission.StudentId = loginUserId;
                        }
                    }
                    else
                    {
                        assignmentSubmission = db.AssignmentSubmissions.Find(models.Id);
                        assignmentSubmission.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                    }
                    assignmentSubmission.Comment = models.Comment;
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
                                assignmentSubmission.SubmissionFile = FileName;
                            }
                        }
                    }
                    if (models.Id == null || models.Id == Guid.Empty)
                    {
                        db.AssignmentSubmissions.Add(assignmentSubmission);
                    }
                    else
                    {
                        db.Entry(assignmentSubmission).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    ModelState.Clear();
                    TempData["AssignmetSubmitted"] = "Assignment Submitted Successfully!";
                    return RedirectToAction("detail", new { Id = models.AssignmentId });
                }
            }
            return View(models);
        }

    }
}