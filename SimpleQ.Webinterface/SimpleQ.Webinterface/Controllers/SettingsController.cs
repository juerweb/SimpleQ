using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class SettingsController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return Http.NotFound("Customer not found.");


                var model = new SettingsModel
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
                };
                return View(viewName: "Settings", model: model);
            }
        }

        [HttpGet]
        public ActionResult ChangeMinGroup(int size)
        {
            using (var db = new SimpleQDBEntities())
            {
                int minAllowed = db.DsgvoConstraints.Where(d => d.ConstrName == "MIN_GROUP_SIZE").FirstOrDefault().ConstrValue;
                if (size < minAllowed)
                    return Http.Conflict($"Size less than allowed ({minAllowed}).");

                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return Http.NotFound("Customer not found.");


                cust.MinGroupSize = size;
                db.SaveChanges();
                return Http.Ok();
            }
        }

        [HttpGet]
        public ActionResult AddCategory(string catName)
        {
            if (string.IsNullOrEmpty(catName))
                return Http.BadRequest("Category name must not be empty.");

            using (var db = new SimpleQDBEntities())
            {
                if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    return Http.NotFound("Customer not found.");

                var query = db.SurveyCategories.Where(c => c.CustCode == CustCode);

                var cat = new SurveyCategory
                {
                    CatId = (query.Count() == 0) ? 1 : query.Max(c => c.CatId) + 1,
                    CatName = catName,
                    CustCode = CustCode
                };
                db.SurveyCategories.Add(cat);
                db.SaveChanges();

                return Content($"{cat.CatId}", "text/plain");
            }
        }

        [HttpGet]
        public ActionResult ModifyCategory(int catId, string catName)
        {
            if (string.IsNullOrEmpty(catName))
                return Http.BadRequest("Category name must not be empty.");

            using (var db = new SimpleQDBEntities())
            {
                var cat = db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode).FirstOrDefault();
                if (cat == null)
                    return Http.NotFound("Category not found.");


                cat.CatName = catName;
                db.SaveChanges();
                return Http.Ok();
            }
        }

        [HttpGet]
        public ActionResult DeleteCategory(int catId)
        {
            using (var db = new SimpleQDBEntities())
            {
                var cat = db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode).FirstOrDefault();
                if (cat == null)
                    return Http.NotFound("Category not found.");

                if (db.Surveys.Where(s => s.CatId == catId && s.CustCode == CustCode).Count() != 0)
                    cat.Deactivated = true;
                else
                    db.SurveyCategories.Remove(cat);

                db.SaveChanges();
                return Http.Ok();
            }
        }

        [HttpPost]
        public ActionResult ChangePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return Http.BadRequest("Password must not be null.");

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return Http.NotFound("Customer not found.");


                cust.CustPwdTmp = password;
                db.SaveChanges();
                return Http.Ok();
            }
        }

        [HttpPost]
        public ActionResult ChangeAnswerTypes(SettingsModel req)
        {
            if (req == null)
                return Http.BadRequest("Model object must not be null.");

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return Http.NotFound("Customer not found.");

                if (req.CheckedAnswerTypes == null)
                    return Http.BadRequest("CheckedAnswerTypes must not be null.");

                if (req.UncheckedAnswerTypes == null)
                    return Http.BadRequest("UncheckedAnswerTypes must not be null.");

                foreach (var typeId in req.CheckedAnswerTypes.Concat(req.UncheckedAnswerTypes).Distinct())
                {
                    if (db.AnswerTypes.Where(a => a.TypeId == typeId).FirstOrDefault() == null)
                        return Http.Conflict("AnswerType does not exist.");
                }


                req.UncheckedAnswerTypes.ForEach(typeId =>
                {
                    cust.AnswerTypes.Remove(db.AnswerTypes.Where(a => a.TypeId == typeId).FirstOrDefault());
                });

                req.CheckedAnswerTypes.ForEach(typeId =>
                {
                    cust.AnswerTypes.Add(db.AnswerTypes.Where(a => a.TypeId == typeId).FirstOrDefault());
                });

                db.SaveChanges();

                return Index();
            }
        }

        [HttpPost]
        public ActionResult UpdateCustomer(SettingsModel req)
        {
            if (req == null)
                return Http.BadRequest("Model object must not be null.");

            using (var db = new SimpleQDBEntities())
            {
                try
                {
                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                        return Http.NotFound("Customer not found.");

                    cust.CustName = req.Name ?? throw ANEx("CustName");
                    cust.CustEmail = req.Email ?? throw ANEx("CustEmail");
                    cust.Street = req.Street ?? throw ANEx("Street");
                    cust.Plz = req.Plz ?? throw ANEx("Plz");
                    cust.City = req.City ?? throw ANEx("City");
                    cust.Country = req.Country ?? throw ANEx("Country");
                    cust.LanguageCode = req.LanguageCode ?? throw ANEx("LanguageCode");

                    if (db.PaymentMethods.Where(p => p.PaymentMethodId == req.PaymentMethodId).Count() == 0)
                        return Http.Conflict("PaymentMethod does not exist.");

                    if (req.DataStoragePeriod <= 0)
                        return Http.Conflict("DataStoragePeriod must be greater than 0");

                    cust.DataStoragePeriod = req.DataStoragePeriod;
                    cust.PaymentMethodId = req.PaymentMethodId;

                    db.SaveChanges();
                    
                    return Index();
                }
                catch (ArgumentNullException ex)
                {
                    return Http.BadRequest($"{ex.ParamName} must not be null.");
                }
            }
        }

        private ArgumentNullException ANEx(string paramName) => new ArgumentNullException(paramName);

        private string CustCode
        {
            get
            {
                return Session["custCode"] as string;
            }
        }
    }
}