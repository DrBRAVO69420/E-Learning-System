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
    public class ChapterController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext();

        public ActionResult EditChapter(Guid? Id)
        {
            string loginUserId = User.Identity.GetUserId();
            ChapterViewModels models = GetChapterViewModels(Id);
            models.TopNavViewModels = generalFunction.GetTopNavViewModels(models.CourseChannelId);
            return View(models);
        }

        [HttpPost]
        public ActionResult EditChapter(ChapterViewModels models)
        {
            string loginUserId = User.Identity.GetUserId();
            if (models != null)
            {
                //check whether title is empty
                if (string.IsNullOrEmpty(models.ChapterTitle))
                {
                    ModelState.AddModelError("ChapterTitle", "Please enter a chapter title.");
                    TempData["ChapterError"] = "Please enter a chapter title.";
                }
                //check title duplication
                else
                {
                    string originalTitleInDB = "";
                    if (models.Id != null && models.Id != Guid.Empty)
                    {
                        originalTitleInDB = db.Chapters.Where(a => a.Id == models.Id).Select(a => a.Title).DefaultIfEmpty("").FirstOrDefault();
                    }
                    bool titleDuplicate = db.Chapters.Where(a => a.Title == models.ChapterTitle && a.Title != originalTitleInDB && a.CreatorId == loginUserId).Any();
                    if (titleDuplicate)
                    {
                        ModelState.AddModelError("ChapterTitle", "Chapter Title already exists. Please rename and try again.");
                        TempData["ChapterError"] = "Chapter Title already exists. Please rename and try again.";
                    }                  
                }

                //everything valid
                if (ModelState.IsValid)
                {
                    try
                    {
                        Chapter chapter = new Chapter();
                        if (models.Id == null || models.Id == Guid.Empty)  //create record
                        {
                            chapter.Id = Guid.NewGuid();                            
                            chapter.CreatorId = User.Identity.GetUserId();
                            chapter.CreatedOn = generalFunction.GetSystemTimeZoneDateTimeNow();
                            chapter.CourseChannelId = models.CourseChannelId;
                            chapter.Title = models.ChapterTitle;
                            db.Chapters.Add(chapter);
                            TempData["ChapterCreated"] = "Created Successfully!";
                        }
                        else //update selected record
                        {
                            chapter = db.Chapters.Find(models.Id);
                            chapter.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                            chapter.Title = models.ChapterTitle;
                            db.Entry(chapter).State = EntityState.Modified;
                            TempData["ChapterUpdated"] = "Updated Successfully!";
                        }
                        db.SaveChanges();
                        models.Id = chapter.Id;
                        ModelState.Clear();
                        if (models.Id == null || models.Id == Guid.Empty)
                        {
                            return RedirectToAction("manage", "coursechannel", new { Id = models.CourseChannelId });
                        }                      
                    }
                    catch (Exception ex)
                    {
                        TempData["ChapterError"] = ex.InnerException.Message;
                    }
                }
            }
            models = GetChapterViewModels(models.Id);
            models.ChapterTitle = db.Chapters.Where(a => a.Id == models.Id).Select(a => a.Title).DefaultIfEmpty("").FirstOrDefault();
            models.TopNavViewModels = generalFunction.GetTopNavViewModels(models.CourseChannelId);
            return View(models);
        }

        public ChapterViewModels GetChapterViewModels(Guid? Id)
        {
            ChapterViewModels models = new ChapterViewModels();
            Chapter chapter = db.Chapters.Find(Id);
            models.Id = chapter.Id;
            models.ChapterTitle = chapter.Title;
            models.CourseChannelId = chapter.CourseChannelId;
            models.CourseChannelName = db.CourseChannels.Where(a => a.Id == chapter.CourseChannelId).Select(a => a.Name).DefaultIfEmpty("").FirstOrDefault();
            models.CreatedOn = chapter.CreatedOn;
            models.CreatorId = chapter.CreatorId;
            models.FileList = db.FileMaterials.Where(a => a.ChapterId == Id).ToList();
            models.TextList = db.TextMaterials.Where(a => a.ChapterId == Id).ToList();
            models.LastUpdate = chapter.LastUpdate;
            return models;
        }

        public ActionResult Read_Chapter([DataSourceRequest] DataSourceRequest request, Guid CourseChannelId)
        {
            List<ChapterViewModels> result = new List<ChapterViewModels>();
            try
            {
                result = (from t1 in db.Chapters
                          join t2 in db.CourseChannels on t1.CourseChannelId equals t2.Id
                          where t2.Id == CourseChannelId
                          select new ChapterViewModels
                          {
                              Id = t1.Id,
                              ChapterTitle = t1.Title,
                              CourseChannelId = t1.CourseChannelId,
                              CreatorId = t1.CreatorId,
                              LastUpdate = t1.LastUpdate,
                              CreatedOn = t1.CreatedOn
                          }).OrderBy(o => o.CreatedOn).ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

        public string Delete_Chapter(Guid? Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    List<FileMaterial> fileMaterials = db.FileMaterials.Where(a => a.ChapterId == Id).ToList();
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
                    Chapter chapter = db.Chapters.Where(a => a.Id == Id).FirstOrDefault();
                    db.Chapters.Remove(chapter);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    error = ex.InnerException.Message;
                }
            }
            return error;
        }
        
    }
}