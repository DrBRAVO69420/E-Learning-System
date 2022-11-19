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
    public class SettingController : Controller
    {
        private GeneralFunctionController generalFunction = new GeneralFunctionController();
        private DefaultDBContext db = new DefaultDBContext();

        // GET: Setting
        public ActionResult Index()
        {
            string loginUserId = User.Identity.GetUserId();
            SettingViewModels models = new SettingViewModels();
            Setting setting = db.Settings.Where(a => a.UserId == loginUserId).FirstOrDefault();
            if (setting!=null)
            {
                models.Id = setting.Id;
                models.Background = setting.Background;
            }
            return View(models);
        }

        [HttpPost]
        public ActionResult PostSetting(SettingViewModels models)
        {
            Setting setting = new Setting();
            if (models.Id != null && models.Id != Guid.Empty)
            {
                setting = db.Settings.Find(models.Id);
            }           
            setting.Background = models.Background;

            if (models.Id != null && models.Id != Guid.Empty)
            {
                db.Entry(setting).State = EntityState.Modified;
            }
            else
            {
                setting.Id = Guid.NewGuid();
                db.Settings.Add(setting);
            }
            db.SaveChanges();
            TempData["SettingUpdated"] = "Updated Successfully!";
            return RedirectToAction("index");
        }
        
    }
}