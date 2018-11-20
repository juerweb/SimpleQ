using SimpleQ.Webinterface.Extensions;
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
            Session["custCode"] = "420420";

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return Http.NotFound("Customer not found.");

                var model = new ContainerViewModel
                {
                    SurveyCreationModel = new SurveyCreationModel
                    {
                        SurveyCategories = cust.SurveyCategories.Where(s => !s.Deactivated).ToList(),
                        AnswerTypes = cust.AnswerTypes.ToList(),
                        Departments = cust.Departments.Select(d => new { Department = d, Amount = d.People.Count }).ToDictionary(x => x.Department, x => x.Amount),
                        SurveyTemplates = db.Surveys.Where(s => s.CustCode == CustCode && s.Template).ToList()
                    },

                    SurveyResultsModel = new SurveyResultsModel
                    {
                        SurveyCategories = cust.SurveyCategories.Where(s => !s.Deactivated).ToList(),
                        Surveys = db.Surveys.Where(s => s.CustCode == CustCode).ToList(),
                        AnswerTypes = cust.AnswerTypes.ToList()
                    },

                    GroupAdministrationModel = new GroupAdministrationModel
                    {
                        Departments = cust.Departments.ToList()
                    },

                    SettingsModel = new SettingsModel
                    {
                        MinGroupSize = cust.MinGroupSize,
                        Categories = cust.SurveyCategories.Where(s => !s.Deactivated).ToList(),
                        ActivatedAnswerTypes = cust.AnswerTypes.ToList(),
                        DeactivatedAnswerTypes = db.AnswerTypes.ToList().Except(cust.AnswerTypes.ToList()).ToList(),
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