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
                SurveyCreationModel = new SurveyCreationModel(),
                SurveyResultsModel = new SurveyResultsModel(),
                GroupAdministrationModel = new GroupAdministrationModel()
            };

            using (var db = new SimpleQDBEntities())
            {
                model.SurveyCreationModel.SurveyCategories = db.SurveyCategories.Where(s => s.CustCode == CustCode).ToList();
                model.SurveyCreationModel.AnswerTypes = db.AnswerTypes.ToList(); // GLOBALIZATION!
                model.SurveyCreationModel.Departments = db.Departments.Where(d => d.CustCode == CustCode).Select(d => new { Department = d, Amount = d.People.Count }).ToDictionary(x => x.Department, x => x.Amount);
                model.SurveyCreationModel.SurveyTemplates = db.Surveys.Where(s => s.CustCode == CustCode && s.Template).ToList();

                model.SurveyResultsModel.SurveyCategories = db.SurveyCategories.Where(s => s.CustCode == CustCode).ToList();
                model.SurveyResultsModel.Surveys = db.Surveys.Where(s => s.CustCode == CustCode).ToList();
                model.SurveyResultsModel.AnswerTypes = db.AnswerTypes.ToList(); // GLOBALIZATION!
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