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

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();

                var model = new ContainerViewModel
                {
                    SurveyCreationModel = new SurveyCreationModel
                    {
                        SurveyCategories = db.SurveyCategories.Where(s => s.CustCode == CustCode).ToList(),
                        AnswerTypes = db.AnswerTypes.ToList(), // GLOBALIZATION!
                        Departments = db.Departments.Where(d => d.CustCode == CustCode).Select(d => new { Department = d, Amount = d.People.Count }).ToDictionary(x => x.Department, x => x.Amount),
                        SurveyTemplates = db.Surveys.Where(s => s.CustCode == CustCode && s.Template).ToList()
                    },

                    SurveyResultsModel = new SurveyResultsModel
                    {
                        SurveyCategories = db.SurveyCategories.Where(s => s.CustCode == CustCode).ToList(),
                        Surveys = db.Surveys.Where(s => s.CustCode == CustCode).ToList(),
                        AnswerTypes = db.AnswerTypes.ToList() // GLOBALIZATION!
                    },

                    GroupAdministrationModel = new GroupAdministrationModel
                    {
                        Departments = db.Departments.Where(d => d.CustCode == CustCode).ToList()
                    },

                    SettingsModel = new SettingsModel
                    {
                        MinGroupSize = cust.MinGroupSize,
                        Categories = cust.SurveyCategories.ToList(),
                        AnswerTypes = db.AnswerTypes.Where(a => !a.Inactive).ToList(),
                        PaymentMethods = db.PaymentMethods.ToList(),
                        Name = cust.CustName,
                        Email = cust.CustEmail,
                        Street = cust.Street,
                        Plz = cust.Plz,
                        City = cust.City,
                        Country = cust.Country,
                        LanguageCode = cust.LanguageCode,
                        DataStoragePeriod = cust.DataStoragePeriod,
                        PaymentMethodId = cust.PaymentMethodId
                    },

                    SupportModel = new SupportModel
                    {
                        FaqEntries = db.FaqEntries.ToList()
                    }
                };
                return View(model: model);
            }
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