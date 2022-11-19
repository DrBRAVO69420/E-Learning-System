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
    public class TextMaterialController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext();

        [HttpPost, ValidateInput(false)]
        public ActionResult AddTextMaterial(ChapterViewModels model, string textLinkEditor,Guid? textMaterialId)
        {
            if (string.IsNullOrEmpty(textLinkEditor))
            {
                TempData["TextMaterialEmptyError"] = "Failed. The value cannot be empty.";
            }
            else if (model != null)
            {
                TextMaterial textMaterial = new TextMaterial();
                if (textMaterialId == null || textMaterialId == Guid.Empty)//new record
                {
                    textMaterial.Id = Guid.NewGuid();
                    textMaterial.ChapterId = model.Id;
                    textMaterial.CreatorId = User.Identity.GetUserId();
                }
                else //update existing
                {
                    textMaterial = db.TextMaterials.Find(textMaterialId);
                }
                textMaterial.Detail = textLinkEditor;
                textMaterial.LastUpdate = generalFunction.GetSystemTimeZoneDateTimeNow();

                if (textMaterialId == null || textMaterialId == Guid.Empty)
                {
                    db.TextMaterials.Add(textMaterial);
                }
                else
                {
                    db.Entry(textMaterial).State = EntityState.Modified;
                }    
                db.SaveChanges();
                ModelState.Clear();
                TempData["TextMaterialAdded"] = "Updated Successfully!";
            }

            return RedirectToAction("editchapter", "chapter", new { Id = model.Id });
        }

        public ActionResult Read_TextMaterial([DataSourceRequest] DataSourceRequest request, Guid? ChapterId)
        {
            List<TextMaterialViewModels> result = new List<TextMaterialViewModels>();
            try
            {
                result = (from t1 in db.Chapters
                          join t2 in db.TextMaterials on t1.Id equals t2.ChapterId
                          where t1.Id == ChapterId
                          select new TextMaterialViewModels
                          {
                              Id = t2.Id,
                              Detail = t2.Detail,
                              ChapterId = t1.Id,
                              CreatorId = t2.CreatorId,
                              LastUpdate = t2.LastUpdate
                          }).OrderBy(o => o.LastUpdate).ToList();
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

        public string Delete_TextMaterial(Guid Id)
        {
            string error = "";
            if (Id != null && Id != Guid.Empty)
            {
                try
                {
                    TextMaterial textMaterial = db.TextMaterials.Find(Id);
                    db.TextMaterials.Remove(textMaterial);
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
    }
}