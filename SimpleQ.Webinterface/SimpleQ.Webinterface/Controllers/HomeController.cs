using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Session["custCode"] = "m4rku5";

            var model = new ContainerViewModel
            {
                SurveyCreationModel = new SurveyCreationModel()
            };

            using (var db = new SimpleQDBEntities())
            {
                model.SurveyCreationModel.SurveyCategories = db.SurveyCategories.Where(s => s.CustCode == CustCode).ToList();
                model.SurveyCreationModel.AnswerTypes = db.AnswerTypes.ToList();
                model.SurveyCreationModel.Departments = db.Departments.Where(g => g.CustCode == CustCode).ToList();
            }
            return View(model: model);
        }

        private string CustCode
        {
            get
            {
                return Session["custCode"] as string;
            }
        }
    }
}