using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNet.Identity;
using OnlineLearning.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineLearning.Controllers
{
    public class FileMaterialController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext();

        public ActionResult UploadFile(ChapterViewModels model, IEnumerable<HttpPostedFileBase> attachments)
        {
            string FileName = "";
            //save attachments
            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    if (file != null)
                    {
                        // get the file name of the selected image file and save into Files folder
                        FileName = new FileInfo(file.FileName).Name;
                        string serverFilePath = HttpContext.Server.MapPath("~/UploadedFiles/") + FileName;
                        file.SaveAs(serverFilePath); //remember to set 'write' permission on UploadedFiles folder in the server 
                        FileMaterial fileMaterial = new FileMaterial();
                        fileMaterial.Id = Guid.NewGuid();
                        fileMaterial.ChapterId = model.Id;
                        fileMaterial.CreatorId = User.Identity.GetUserId();
                        fileMaterial.FileName = FileName;
                        fileMaterial.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();
                        db.FileMaterials.Add(fileMaterial);
                        db.SaveChanges();
                        ModelState.Clear();
                        TempData["FileUploaded"] = "File Uploaded Successfully!";
                    }
                }
            }
            return RedirectToAction("editchapter", "chapter", new { Id = model.Id });
        }

        public ActionResult Read_FileMaterial([DataSourceRequest] DataSourceRequest request, Guid? ChapterId)
        {
            List<FileMaterialViewModels> result = new List<FileMaterialViewModels>();
            try
            {
                result = (from t1 in db.Chapters
                          join t2 in db.FileMaterials on t1.Id equals t2.ChapterId
                          where t1.Id == ChapterId
                          select new FileMaterialViewModels
                          {
                              Id = t2.Id,
                              FileName = t2.FileName,
                              ChapterId = t1.Id,
                              CreatorId = t2.CreatorId,
                              LastUpdate = t2.LastUpdate
                          }).OrderBy(o => o.LastUpdate).ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", ex.InnerException.Message);
            }

            return Json(result.ToDataSourceResult(request));
        }

      
        public ActionResult Delete_FileMaterial([DataSourceRequest] DataSourceRequest request, FileMaterialViewModels model)
        {
            if (model != null)
            {
                try
                {
                    FileMaterial fileMaterial = db.FileMaterials.Find(model.Id);
                    //delete file from server
                    string fullPath = Request.MapPath("~/UploadedFiles/" + fileMaterial.FileName);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    db.FileMaterials.Remove(fileMaterial);
                    db.SaveChanges();

                    ModelState.Clear(); //if everthing is ok and executed successfully, clear the model state
                }
                catch (Exception ex)
                {
                    //if got any sql exception when delete the record, add the exception message to model state error
                    ModelState.AddModelError("Id", ex.InnerException);
                }
            }
            
            return RedirectToAction("editchapter","chapter",new { Id = model.ChapterId});
        }


        //validate whether uploaded attachments file name already exists in the db
        public string ValidateUpload(string currentFileName)
        {
            List<string> names = new List<string>();
            names = db.FileMaterials.Select(a => a.FileName).ToList();
            string errorMsg = "";
            if (names.Contains(currentFileName))
            {
                errorMsg = "The file " + currentFileName + " already exists in the system. Please rename your file and try again.";
                return errorMsg;
            }
            return null;
        }

    }
}