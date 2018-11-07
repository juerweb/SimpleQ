using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class SettingsController : Controller
    {
        [HttpGet]
        public ActionResult LoadSettings()
        {
            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                var model = new SettingsModel
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
                };
                return PartialView(viewName: "_Settings", model: model);
            }
        }

        [HttpGet]
        public ActionResult ChangeMinGroup(int size)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().MinGroupSize = size;
                db.SaveChanges();
            }
            return LoadSettings();
        }

        [HttpGet]
        public ActionResult AddCategory(string catName)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.SurveyCategories.Add(new SurveyCategory
                {
                    CatId = db.SurveyCategories.Where(c => c.CustCode == CustCode).Max(c => c.CatId) + 1,
                    CatName = catName,
                    CustCode = CustCode
                });
                db.SaveChanges();
            }
            return LoadSettings();
        }

        [HttpGet]
        public ActionResult ModifyCategory(int catId, string catName)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode).FirstOrDefault().CatName = catName;
                db.SaveChanges();
            }
            return LoadSettings();
        }

        [HttpGet]
        public ActionResult DeleteCategory(int catId)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.SurveyCategories.RemoveRange(db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode));
                db.SaveChanges();
            }
            return LoadSettings();
        }

        [HttpPost]
        public ActionResult UpdateCustomer(SettingsModel req)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().CustName = req.Name;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().CustEmail = req.Email;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().CustPwdTmp = req.Password;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().Street = req.Street;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().Plz = req.Plz;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().City = req.City;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().Country = req.Country;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().LanguageCode = req.LanguageCode;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().DataStoragePeriod = req.DataStoragePeriod;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().PaymentMethodId = req.PaymentMethodId;
                db.SaveChanges();
            }
            return LoadSettings();
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