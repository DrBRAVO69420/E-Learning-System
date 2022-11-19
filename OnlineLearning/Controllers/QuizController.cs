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
    public class QuizController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext();

        //id = course channel id
        public ActionResult List(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            QuizViewModels models = new QuizViewModels();
            if (Id != null && Id != Guid.Empty)
            {
                models.CourseChannelId = Id;
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(Id);
            }
            return View(models);
        }

        [CustomAuthorize(Roles ="Teacher")]
        public ActionResult StudentResult(Guid? Id)
        {
            QuizViewModels models = new QuizViewModels();
            if (Id != null && Id != Guid.Empty)
            {
                models = GetQuizViewModels(Id,"");
            }
            return View(models);
        }

        public ActionResult Detail(Guid? Id, string studentId)
        {
            QuizViewModels models = new QuizViewModels();
            if (Id!=null && Id != Guid.Empty)
            {
                models = GetQuizViewModels(Id, studentId);
            }
            return View(models);
        }

        [CustomAuthorize(Roles ="Student")]
        public ActionResult StartQuiz(Guid? Id)
        {
            QuizViewModels models = new QuizViewModels();
            string loginUserId = User.Identity.GetUserId(); //student
            if (Id != null && Id != Guid.Empty)
            {
                models = GetQuizViewModels(Id, loginUserId);
                models.QuestionList = (from t1 in db.QuizQuestions
                                       where t1.QuizId == Id
                                       select new QuizQuestionViewModels
                                       {
                                           Id = t1.Id,
                                           Question = t1.Question,
                                           AttachedImage = t1.AttachedImage,
                                           AnswerList = db.QuizAnswers.Where(a => a.QuestionId == t1.Id).ToList()
                                       }).ToList();
                if (models.RandomizeQuestion == true)
                {
                    models.QuestionList.Shuffle();
                }
                foreach (var item in models.QuestionList)
                {
                    item.AnswerList.Shuffle();
                }
            }
            return View(models);
        }

        public ActionResult AnswerQuestion(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            bool firstStartSaved = db.StudentQuizzes.Where(a => a.QuizId == Id && a.StudentId == loginUserId).Any();
            if (firstStartSaved == false)
            {
                StudentQuiz studentQuiz = new StudentQuiz();
                studentQuiz.Id = Guid.NewGuid();
                studentQuiz.QuizId = Id;
                studentQuiz.QuizStartedOn = generalFunction.GetSystemTimeZoneDateTimeNow();
                studentQuiz.StudentId = User.Identity.GetUserId();
                db.StudentQuizzes.Add(studentQuiz);
                db.SaveChanges();
            }
            QuizQuestionViewModels models = GetAvailableQuizQuestion(Id);
            if (models == null)
            {
                //assign some default value
                Guid? courseChannelid = db.Quizzes.Where(a => a.Id == Id).Select(a => a.CourseChannelId).FirstOrDefault();
                models.UserFullName = db.AspNetUsers.Where(a => a.Id == loginUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
                models.CourseChannelName = db.CourseChannels.Where(a => a.Id == courseChannelid).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
                models.QuizName = db.Quizzes.Where(a => a.Id == Id).Select(a => a.QuizTitle).DefaultIfEmpty("").FirstOrDefault();
                models.CurrentQuestionNumber = 0;
                models.TotalQuestion = 0;
            }
            return View(models);
        }

        public ActionResult QuizResult(Guid? Id)
        {
            QuizViewModels models = GetQuizViewModels(Id,"");
            return View(models);
        }

        public JsonResult SaveAnswer(Guid? QuestionId, Guid? AnswerId)
        {
            string loginUserId = User.Identity.GetUserId();
            try
            {      
                StudentQuizAnswer studentQuizAnswer = new StudentQuizAnswer();
                if (QuestionId != null && QuestionId != Guid.Empty && AnswerId != null && AnswerId != Guid.Empty)
                {
                    studentQuizAnswer.Id = Guid.NewGuid();
                    studentQuizAnswer.AnswerId = AnswerId;
                    studentQuizAnswer.QuestionId = QuestionId;
                    studentQuizAnswer.StudentId = loginUserId;
                    studentQuizAnswer.QuizId = db.QuizQuestions.Where(a => a.Id == QuestionId).Select(a => a.QuizId).FirstOrDefault(); 
                    studentQuizAnswer.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                    db.StudentQuizAnswers.Add(studentQuizAnswer);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string error = ex.InnerException.Message;
            }
            return Json(db.QuizAnswers.Where(a => a.QuestionId == QuestionId).ToList(), JsonRequestBehavior.AllowGet);
        }

        public QuizQuestionViewModels GetAvailableQuizQuestion(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            QuizQuestionViewModels models = new QuizQuestionViewModels();
            if (Id != null && Id != Guid.Empty)
            {
                List<Guid?> AnsweredQuestions = db.StudentQuizAnswers.Where(a => a.QuizId == Id && a.StudentId == loginUserId).Select(a => a.QuestionId).ToList();
                List<QuizQuestionViewModels> AllNotAnsweredQuestions = (from t1 in db.QuizQuestions
                                                                        where t1.QuizId == Id && AnsweredQuestions.Contains(t1.Id) == false
                                                                        select new QuizQuestionViewModels
                                                                        {
                                                                            Id = t1.Id,
                                                                            Question = t1.Question,
                                                                            AttachedImage = t1.AttachedImage,
                                                                            AnswerList = db.QuizAnswers.Where(a => a.QuestionId == t1.Id).ToList()
                                                                        }).Where(a => a.AnswerList.Count > 0).ToList();
                if(AllNotAnsweredQuestions.Count != 0)
                {
                    bool? randomize = db.Quizzes.Where(a => a.Id == Id).Select(a => a.RandomizeQuestion).FirstOrDefault();
                    if (randomize == true)
                    {
                        AllNotAnsweredQuestions.Shuffle();
                    }
                    models = AllNotAnsweredQuestions.FirstOrDefault(); //return one of the 'not yet answered' question, for the student to answer it
                    models.AnswerList.Shuffle(); // randomize answer choice
                    models.QuizId = Id;
                    models.QuizName = db.Quizzes.Where(a => a.Id == Id).Select(a => a.QuizTitle).DefaultIfEmpty("").FirstOrDefault();
                    var allquestion = db.QuizQuestions.Where(a => a.QuizId == Id).Select(a => a.Id).ToList();
                    int? validQuestion = 0;
                    // only questions which have answer that are counted in Total Question for a quiz, a question that don't have answer considered invalid question
                    foreach (var questionid in allquestion)
                    {
                        if (db.QuizAnswers.Where(a => a.QuestionId == questionid).Any())
                        {
                            validQuestion++;
                        }
                    }
                    models.TotalQuestion = validQuestion;
                    Guid? courseChannelid = db.Quizzes.Where(a => a.Id == Id).Select(a => a.CourseChannelId).FirstOrDefault();
                    models.CourseChannelName = db.CourseChannels.Where(a => a.Id == courseChannelid).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
                    models.UserFullName = db.AspNetUsers.Where(a => a.Id == loginUserId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
                    int? notYetAnswered = AllNotAnsweredQuestions.Count();
                    models.CurrentQuestionNumber = models.TotalQuestion - notYetAnswered + 1;
                    if (notYetAnswered == 1)
                    {
                        models.IsLastQuestion = true;
                    }
                    else
                    {
                        models.IsLastQuestion = false;
                    }
                }
            }
            return models;
        }

        public ActionResult EditQuiz(Guid? courseChannelId, Guid? quizId)
        {
            string loginUserId = User.Identity.GetUserId();
            QuizViewModels models = new QuizViewModels();
            if (quizId != null && quizId != Guid.Empty)
            {
                models = GetQuizViewModels(quizId,"");
            }
            if (courseChannelId != null && courseChannelId != Guid.Empty)
            {
                models.CourseChannelId = courseChannelId;
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(courseChannelId);
            }
            return View(models);
        }

        [HttpPost]
        public ActionResult EditQuiz(QuizViewModels models)
        {
            string loginUserId = User.Identity.GetUserId();
            if (models != null)
            {
                if (ModelState.IsValid)
                {
                    Quiz quiz = new Quiz();
                    if (models.Id == null || models.Id == Guid.Empty) //new record
                    {
                        quiz.Id = Guid.NewGuid();
                        quiz.CourseChannelId = models.CourseChannelId;
                        quiz.CreatorId = User.Identity.GetUserId();
                        quiz.IsEnabled = false;
                    }
                    else //update record
                    {
                        quiz = db.Quizzes.Find(models.Id);
                    }

                    quiz.QuizTitle = models.QuizTitle;
                    quiz.Instruction = models.Instruction;
                    quiz.DueDateTime = models.DueDateTime;
                    quiz.RandomizeQuestion = models.RandomizeQuestion;
                    quiz.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();

                    if (models.Id == null || models.Id == Guid.Empty) //new record
                    {
                        db.Quizzes.Add(quiz);
                    }
                    else //update record
                    {
                        db.Entry(quiz).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    ModelState.Clear();
                    TempData["QuizUpdated"] = "Updated Successfully!";
                    return RedirectToAction("list", new { Id = models.CourseChannelId });
                }
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(models.CourseChannelId);
            }
            return View(models);
        }
        public string Enable_Quiz(Guid? Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    Quiz quiz = db.Quizzes.Find(Id);
                    quiz.IsEnabled = true;
                    db.Entry(quiz).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message;
                }
            }
            return error;
        }

        public QuizViewModels GetQuizViewModels(Guid? Id, string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                studentId = User.Identity.GetUserId();
            }
            QuizViewModels models = new QuizViewModels();
            if (Id != null && Id != Guid.Empty)
            {
                Quiz quiz = db.Quizzes.Find(Id);
                if (quiz != null)
                {
                    models.Id = quiz.Id;
                    models.QuizTitle = quiz.QuizTitle;
                    models.Instruction = quiz.Instruction;
                    models.LastUpdate = quiz.LastUpdate;
                    models.RandomizeQuestion = quiz.RandomizeQuestion;
                    models.CreatorId = quiz.CreatorId;
                    models.CreatorName = db.AspNetUsers.Where(a => a.Id == quiz.CreatorId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
                    models.CourseChannelId = quiz.CourseChannelId;
                    models.DueDateTime = quiz.DueDateTime;
                    models.TopNavViewModels = generalFunction.GetTopNavViewModels(quiz.CourseChannelId);
                    models.TotalQuestion = db.QuizQuestions.Where(a => a.QuizId == quiz.Id).Count();
                    models.StudentQuizViewModel = new StudentQuizViewModel();
                    models.StudentQuizViewModel.StudentId = studentId;
                    models.StudentQuizViewModel.StudentName = db.AspNetUsers.Where(a => a.Id == studentId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
                    models.StudentQuizViewModel.ProfilePic = db.AspNetUsers.Where(a => a.Id == studentId).Select(a => a.ProfilePicName).DefaultIfEmpty("").FirstOrDefault();
                    DateTime now = (DateTime)generalFunction.GetSystemTimeZoneDateTimeNow();
                    models.alreadyDue = false;
                    if (models.DueDateTime.HasValue)
                    {
                        TimeSpan timeToEvent = models.DueDateTime.Value.Subtract(now);
                        string remain = string.Format("{0} Days, {1} Hours, {2} Minutes",
                                       timeToEvent.Days, timeToEvent.Hours, timeToEvent.Minutes);
                        models.TimeRemaining = remain;
                        if (models.DueDateTime < now)
                        {
                            models.alreadyDue = true;
                        }
                    }
                    models.Answered = db.StudentQuizAnswers.Where(a => a.QuizId == quiz.Id && a.StudentId == studentId).Any();
                    if (models.Answered == true)
                    {
                        models.QuestionList = (from t1 in db.QuizQuestions
                                               where t1.QuizId == quiz.Id
                                               select new QuizQuestionViewModels
                                               {
                                                   Id = t1.Id,
                                                   QuizId = t1.QuizId,
                                                   Question = t1.Question,
                                                   AttachedImage = t1.AttachedImage,
                                                   AnswerList = db.QuizAnswers.Where(a => a.QuestionId == t1.Id).ToList(),
                                                   StudentAnswerId = db.StudentQuizAnswers.Where(a => a.QuestionId == t1.Id && a.StudentId == studentId).Select(a => a.AnswerId).FirstOrDefault()
                                               }).ToList();
                        models.AnsweredDateTime = db.StudentQuizzes.Where(a => a.QuizId == quiz.Id && a.StudentId == studentId).Select(a => a.QuizStartedOn).FirstOrDefault();
                        models.CorrectCount = GetCorrectCount(studentId, models.Id);
                        models.WrongCount = GetWrongCount(studentId, models.Id);
                    }
                }
            }
            return models;
        }

        //get number of questions that the student answered correctly
        public int? GetCorrectCount(string studentId, Guid? quizId)
        {
            int correct = (from t1 in db.StudentQuizAnswers
                           join t2 in db.QuizAnswers on t1.AnswerId equals t2.Id
                           where t1.QuizId == quizId && t2.IsCorrect == true && t1.StudentId == studentId
                           select t1.Id).Count();
            return correct;
        }

        //get number of questions that the student answered wrongly
        public int? GetWrongCount(string studentId, Guid? quizId)
        {
            int wrong = (from t1 in db.StudentQuizAnswers
                         join t2 in db.QuizAnswers on t1.AnswerId equals t2.Id
                         where t1.QuizId == quizId && t2.IsCorrect == false && t1.StudentId == studentId
                         select t1.Id).Count();
            return wrong;
        }

        public ActionResult Read_Quiz([DataSourceRequest] DataSourceRequest request, Guid CourseChannelId)
        {
            List<QuizViewModels> result = new List<QuizViewModels>();
            string loginUserId = User.Identity.GetUserId();
            try
            {
                if (User.IsInRole("Teacher"))
                {
                    result = (from t1 in db.Quizzes
                              where t1.CourseChannelId == CourseChannelId
                              select new QuizViewModels
                              {
                                  Id = t1.Id,
                                  QuizTitle = t1.QuizTitle,
                                  Instruction = t1.Instruction,
                                  DueDateTime = t1.DueDateTime,
                                  RandomizeQuestion = t1.RandomizeQuestion,
                                  CreatorId = t1.CreatorId,
                                  CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                  LastUpdate = t1.LastUpdate,
                                  CourseChannelId = t1.CourseChannelId,
                                  IsEnabled = t1.IsEnabled,
                                  HasQuestion = db.QuizQuestions.Where(a => a.QuizId == t1.Id).Any(),
                                  QuestionsHaveAnswers = (from t2 in db.QuizQuestions
                                                          join t3 in db.QuizAnswers on t2.Id equals t3.QuestionId
                                                          where t2.QuizId == t1.Id
                                                          select t2.Id).Distinct().Count(),
                                  TotalQuestion = db.QuizQuestions.Where(a => a.QuizId == t1.Id).Count()
                              }).OrderByDescending(o => o.LastUpdate).ToList();
                    foreach (var item in result)
                    {
                        if(item.QuestionsHaveAnswers == item.TotalQuestion && item.TotalQuestion != 0)
                        {
                            item.AllQuestionsHaveAnswer = true;
                        }
                        else
                        {
                            item.AllQuestionsHaveAnswer = false;
                        }
                    }
                }
                if (User.IsInRole("Student"))
                {
                    result = (from t1 in db.Quizzes
                              where t1.CourseChannelId == CourseChannelId && t1.IsEnabled == true
                              select new QuizViewModels
                              {
                                  Id = t1.Id,
                                  QuizTitle = t1.QuizTitle,
                                  Instruction = t1.Instruction,
                                  DueDateTime = t1.DueDateTime,
                                  RandomizeQuestion = t1.RandomizeQuestion,
                                  CreatorId = t1.CreatorId,
                                  CreatorName = db.AspNetUsers.Where(a => a.Id == t1.CreatorId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault(),
                                  LastUpdate = t1.LastUpdate,
                                  CourseChannelId = t1.CourseChannelId,
                                  IsEnabled = t1.IsEnabled,
                                  Answered = db.StudentQuizzes.Where(a => a.QuizId == t1.Id && a.StudentId == loginUserId).Select(a => a.Id).FirstOrDefault() == Guid.Empty  ? false : true
                              }).OrderByDescending(o => o.LastUpdate).ToList();
                }
                foreach (var item in result)
                {
                   
                    DateTime lastupdate = (DateTime)item.LastUpdate;
                    item.strLastUpdate = lastupdate.ToString("yyyy-MM-dd hh:mm tt");
                    if (item.DueDateTime.HasValue)
                    {
                        DateTime due = (DateTime)item.DueDateTime;
                        TimeSpan timeToEvent = item.DueDateTime.Value.Subtract(DateTime.Now);
                        string remain = string.Format("{0} Days, {1} Hours, {2} Minutes",
                                       timeToEvent.Days, timeToEvent.Hours, timeToEvent.Minutes);
                        item.TimeRemaining = remain;
                        item.strDueDateTime = due.ToString("yyyy-MM-dd hh:mm tt");
                    }
                    if (item.AnsweredDateTime.HasValue)
                    {
                        DateTime answeredOn = (DateTime)item.AnsweredDateTime;
                        item.strAnsweredDateTime = answeredOn.ToString("yyyy-MM-dd hh:mm tt");
                    }
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        public string Delete_Quiz(Guid Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    //delete from quiz, as set in sql query, it also will delete QuizQuestion, QuizAnswer, StudentQuiz, StudentQuizAnswer
                    Quiz quiz = db.Quizzes.Find(Id);
                    db.Quizzes.Remove(quiz);
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

        public string RemoveDueDate(Guid? Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    Quiz quiz = db.Quizzes.Find(Id);
                    quiz.DueDateTime = null;
                    db.Entry(quiz).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message;
                }
            }
            else
            {
                error = "Please select a quiz";
            }
            return error;
        }

        #region ----------------------------------- Question -----------------------------------

        [CustomAuthorize(Roles ="Teacher")]
        public ActionResult QuestionList(Guid? Id)
        {
            QuizQuestionViewModels models = new QuizQuestionViewModels();
            string loginUserId = User.Identity.GetUserId();
            if (Id != null && Id != Guid.Empty)
            {
                Guid? courseChannelId = db.Quizzes.Where(a => a.Id == Id).Select(a => a.CourseChannelId).FirstOrDefault();
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(courseChannelId);
                models.QuizDetail = GetQuizViewModels(Id,"");
                models.QuizId = Id;
            }
            return View(models);
        }

        public QuizQuestionViewModels GetQuestionViewModels(Guid? Id)
        {
            QuizQuestionViewModels models = new QuizQuestionViewModels();
            if (Id != null && Id != Guid.Empty)
            {
                QuizQuestion quizQuestion = db.QuizQuestions.Find(Id);
                models.Id = quizQuestion.Id;
                models.Question = quizQuestion.Question;
                models.QuizId = quizQuestion.QuizId;
                models.QuizDetail = GetQuizViewModels(quizQuestion.QuizId,"");
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(models.QuizDetail.CourseChannelId);
                models.AttachedImage = quizQuestion.AttachedImage;
                models.LastUpdate = quizQuestion.LastUpdate;
                models.AnswerList = db.QuizAnswers.Where(a => a.QuestionId == quizQuestion.Id).ToList();
                
            }
            return models;
        }

        [CustomAuthorize(Roles ="Teacher")]
        public ActionResult EditQuestion(Guid? quizId, Guid? questionId)
        {
            string loginUserId = User.Identity.GetUserId();
            QuizQuestionViewModels models = new QuizQuestionViewModels();
            if (questionId != null && questionId != Guid.Empty)
            {
                models = GetQuestionViewModels(questionId);
                if (models.AnswerList.Count == 2)
                {
                    
                    models.AnswerList.Add(new QuizAnswer());
                }
                if (models.AnswerList.Count == 3)
                {
                    models.AnswerList.Add(new QuizAnswer());
                }
               for(var i = 0; i<models.AnswerList.Count; i++)
                {
                    if (models.AnswerList[i].IsCorrect == true)
                    {
                        models.CorrectAnswer = i;
                    }
                }
            }
            else
            {
                models.AnswerList = new List<QuizAnswer>();
                const int maxEntries = 4;
                while (models.AnswerList.Count < maxEntries)
                {
                    models.AnswerList.Add(new QuizAnswer());
                }
            }
            if (quizId != null && quizId != Guid.Empty)
            {
                models.QuizId = quizId;
                models.QuizDetail = GetQuizViewModels(quizId,"");
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(models.QuizDetail.CourseChannelId);
            }
            return View(models);
        }

        [HttpPost]
        public ActionResult EditQuestion(QuizQuestionViewModels models, IEnumerable<HttpPostedFileBase> attachments)
        {
            string loginUserId = User.Identity.GetUserId();
            string uploadedFileName = "";
            if (models != null)
            {
                //check question duplication
                bool questionExist = false;
                if (models.Id != null && models.Id != Guid.Empty)
                {
                    string oldQuestion = db.QuizQuestions.Where(a => a.Id == models.Id).Select(a => a.Question).DefaultIfEmpty("").FirstOrDefault();
                    questionExist = db.QuizQuestions.Where(a => a.Question == models.Question && a.Question != oldQuestion && a.QuizId == models.QuizId).Any();             
                }
                else
                {
                    questionExist = db.QuizQuestions.Where(a => a.Question == models.Question && a.QuizId == models.QuizId).Any();
                }
                if (questionExist)
                {
                    ModelState.AddModelError("Question", "Question already exists in this quiz.");
                }
                //check answer null
                int answerEmptyCount = models.AnswerList.Where(a => a.AnswerText == null || a.AnswerText == "").Count();
                if (answerEmptyCount >2)
                {
                    ModelState.AddModelError("Id", "At least two Answer Options are required.");
                    TempData["AnswerEmpty"] = "At least two Answer Options are required.";
                }
                else
                {
                    //check duplicate answer                
                    foreach (var item in models.AnswerList)
                    {
                        int sameAnswerText =  models.AnswerList.Where(a => a.AnswerText == item.AnswerText && item.AnswerText != null).Count();
                        if (sameAnswerText>1)
                        {
                            ModelState.AddModelError("Id", "Answer options must be different from each other.");
                            TempData["AnswerDuplicate"] = "Answer options must be different from each other.";
                        }
                    }
                    //check whether selected the correct answer
                    if (models.CorrectAnswer == null)
                    {
                        TempData["CorrectAnswerEmpty"] = "Please select a correct answer.";
                    }
                }

                if (ModelState.IsValid)
                {
                    QuizQuestion quizQuestion = new QuizQuestion();
                    if (models.Id == null || models.Id == Guid.Empty) //new record
                    {
                        quizQuestion.Id = Guid.NewGuid();
                        quizQuestion.QuizId = models.QuizId;
                    }
                    else //update record
                    {
                        quizQuestion = db.QuizQuestions.Find(models.Id);
                    }

                    quizQuestion.Question = models.Question;
                    quizQuestion.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
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
                                quizQuestion.AttachedImage = uploadedFileName;
                            }
                        }
                    }

                    if (models.Id == null || models.Id == Guid.Empty) //new record
                    {
                        db.QuizQuestions.Add(quizQuestion);
                    }
                    else //update record
                    {
                        db.Entry(quizQuestion).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    ModelState.Clear();
                    models.Id = quizQuestion.Id;

                    for (var i = 0; i< models.AnswerList.Count; i ++)
                    {
                        if (models.AnswerList[i].Id == null || models.AnswerList[i].Id == Guid.Empty)
                        {
                            if (!string.IsNullOrEmpty(models.AnswerList[i].AnswerText))
                            {
                                QuizAnswer quizAnswer = new QuizAnswer();
                                quizAnswer.Id = Guid.NewGuid();
                                quizAnswer.QuestionId = models.Id;
                                quizAnswer.AnswerText = models.AnswerList[i].AnswerText;
                                if (models.CorrectAnswer == i)
                                {
                                    quizAnswer.IsCorrect = true;
                                }
                                else
                                {
                                    quizAnswer.IsCorrect = false;
                                }
                                quizAnswer.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                                db.QuizAnswers.Add(quizAnswer);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            //update new changes
                            QuizAnswer quizAnswer = db.QuizAnswers.Find(models.AnswerList[i].Id);
                            //if answer text empty, delete from db
                            if (string.IsNullOrEmpty(models.AnswerList[i].AnswerText))
                            {
                                db.QuizAnswers.Remove(quizAnswer);
                                db.SaveChanges();
                            }
                            else
                            {
                                quizAnswer.AnswerText = models.AnswerList[i].AnswerText;
                                if (models.CorrectAnswer == i)
                                {
                                   
                                    //remove old correct answer, set old correct answer to false
                                    QuizAnswer oldCorrectAnswer = db.QuizAnswers.FirstOrDefault(a => a.QuestionId == models.Id && a.IsCorrect == true);
                                    oldCorrectAnswer.IsCorrect = false;
                                    db.Entry(oldCorrectAnswer).State = EntityState.Modified;
                                    db.SaveChanges();
                                    //set this answer to become the correct answer
                                    quizAnswer.IsCorrect = true;
                                }
                                else
                                {
                                    quizAnswer.IsCorrect = false;
                                }
                                quizAnswer.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                                db.Entry(quizAnswer).State = EntityState.Modified;
                                db.SaveChanges();
                            } 
                        }
                    }
                    TempData["QuestionUpdated"] = "Updated Successfully!";

                    return RedirectToAction("questionlist", new { Id = models.QuizId });
                }
                models.TopNavViewModels = generalFunction.GetTopNavViewModels(models.QuizDetail.CourseChannelId);           
            }
            return View(models);
        }

        public string Delete_Question(Guid? Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    //delete from question
                    QuizQuestion quizQuestion = db.QuizQuestions.Find(Id);
                    if (quizQuestion.AttachedImage != null)
                    {
                        //delete file from server
                        string fullPath = Request.MapPath("~/UploadedFiles/" + quizQuestion.AttachedImage);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                    db.QuizQuestions.Remove(quizQuestion);
                    db.SaveChanges();
                    //delete from answer
                    List<QuizAnswer> answerList = db.QuizAnswers.Where(a => a.QuestionId == Id).ToList();
                    foreach (var answer in answerList)
                    {
                        //delete from student answer
                        List<StudentQuizAnswer> studentAnswerList = db.StudentQuizAnswers.Where(a => a.AnswerId == answer.Id).ToList();
                        foreach (var item in studentAnswerList)
                        {
                            StudentQuizAnswer studentQuizAnswer = db.StudentQuizAnswers.Find(item.Id);
                            db.StudentQuizAnswers.Remove(studentQuizAnswer);
                            db.SaveChanges();
                        }
                        QuizAnswer quizAnswer = db.QuizAnswers.Find(answer.Id);
                        db.QuizQuestions.Remove(quizQuestion);
                        db.SaveChanges();
                    }
                    ModelState.Clear(); //if everthing is ok and executed successfully, clear the model state
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message;
                }
            }
            return error;
        }

        public string Delete_Image(Guid? Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    QuizQuestion quizQuestion = db.QuizQuestions.Find(Id);
                    if (quizQuestion.AttachedImage !=null )
                    {
                        //delete file from server
                        string fullPath = Request.MapPath("~/UploadedFiles/" + quizQuestion.AttachedImage);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                    quizQuestion.AttachedImage = null;
                    db.Entry(quizQuestion).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message;
                }
            }
            return error;
        }

        public ActionResult Read_QuizQuestion([DataSourceRequest] DataSourceRequest request, Guid QuizId)
        {
            List<QuizQuestionViewModels> result = new List<QuizQuestionViewModels>();
            string loginUserId = User.Identity.GetUserId();
            try
            {
                result = (from t1 in db.QuizQuestions
                          join t2 in db.Quizzes on t1.QuizId equals t2.Id
                          where t2.Id == QuizId
                          select new QuizQuestionViewModels
                          {
                              Id = t1.Id,
                              QuizDetail = new QuizViewModels
                              {
                                  QuizTitle = t2.QuizTitle,
                                  Id = t2.Id,
                                  Instruction = t2.Instruction,
                                  DueDateTime = t2.DueDateTime,
                                  CourseChannelId = t2.CourseChannelId
                              },
                              Question = t1.Question,
                              QuizId = t1.QuizId,
                              AttachedImage = t1.AttachedImage,
                              LastUpdate = t1.LastUpdate
                          }).OrderByDescending(o => o.LastUpdate).ToList();

                foreach (var item in result)
                {
                    DateTime lastupdate = (DateTime)item.LastUpdate;
                    item.strLastUpdate = lastupdate.ToString("yyyy-MM-dd hh:mm tt");
                    if (item.QuizDetail.DueDateTime != null)
                    {
                        DateTime due = (DateTime)item.QuizDetail.DueDateTime;
                        item.QuizDetail.strDueDateTime = due.ToString("yyyy-MM-dd hh:mm tt");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        public ActionResult Read_StudentResult([DataSourceRequest] DataSourceRequest request, Guid QuizId)
        {
            List<StudentQuizViewModel> result = new List<StudentQuizViewModel>();
            try
            {
                result = (from t1 in db.Quizzes
                          join t2 in db.StudentQuizzes on t1.Id equals t2.QuizId
                          join t3 in db.AspNetUsers on t2.StudentId equals t3.Id
                          where t2.QuizId == QuizId
                          select new StudentQuizViewModel
                          {
                              Id = t1.Id,
                              StudentId = t2.StudentId,
                              StudentName = t3.Name,
                              ProfilePic = t3.ProfilePicName,
                              QuizStartedOn = t2.QuizStartedOn
                          }).OrderBy(o => o.StudentName).ToList();
                foreach (var item in result)
                {
                    item.CorrectCount = GetCorrectCount(item.StudentId , item.Id);
                    item.WrongCount = GetWrongCount(item.StudentId, item.Id);
                    if (item.QuizStartedOn!=null)
                    {
                        DateTime startedon = (DateTime)item.QuizStartedOn;
                        item.strQuizStartedOn = startedon.ToString("yyyy-MM-dd hh:mm tt");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        //validate whether uploaded attachments file name already exists in the db
        public string ValidateUpload(string currentFileName)
        {
            List<string> names = new List<string>();
            names = db.QuizQuestions.Select(a => a.AttachedImage).ToList();
            string errorMsg = "";
            if (names.Contains(currentFileName))
            {
                errorMsg = "The file " + currentFileName + " already exists in the system. Please rename your file and try again.";
                return errorMsg;
            }
            return null;
        }

        #endregion

     
    }
}