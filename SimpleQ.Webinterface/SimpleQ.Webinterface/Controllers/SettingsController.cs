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
        public string ChangeMinGroup(int size)
        {
            using (var db = new SimpleQDBEntities())
            {
                int minAllowed = db.DsgvoConstraints.Where(d => d.ConstrName == "MIN_GROUP_SIZE").FirstOrDefault().ConstrValue;
                if (size < minAllowed)
                    return $"Die Gruppengröße darf nicht kleiner als {minAllowed} sein.";

                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().MinGroupSize = size;
                db.SaveChanges();
                return "";
            }
        }

        [HttpGet]
        public string AddCategory(string catName)
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
                return "";
            }
        }

        [HttpGet]
        public string ModifyCategory(int catId, string catName)
        {
            using (var db = new SimpleQDBEntities())
            {
                try
                {
                    db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode).FirstOrDefault().CatName = catName;
                    db.SaveChanges();
                    return "";
                }
                catch (NullReferenceException)
                {
                    return $"Gewählte Kategorie existiert nicht mehr.";
                }
            }
        }

        [HttpGet]
        public string DeleteCategory(int catId)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.SurveyCategories.RemoveRange(db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode));
                int rows = db.SaveChanges();
                return (rows == 0) ? "Gewählte Kategorie existiert nicht mehr." : "";
            }
        }

        [HttpPost]
        public string ChangePassword(string password)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().CustPwdTmp = password;
                db.SaveChanges();
                return "";
            }
        }

        [HttpPost]
        public ActionResult UpdateCustomer(SettingsModel req)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().CustName = req.Name;
                db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().CustEmail = req.Email;
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