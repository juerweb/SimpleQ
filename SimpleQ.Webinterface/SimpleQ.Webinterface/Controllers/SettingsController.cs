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
        public ActionResult LoadSettings()
        {
            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return HttpNotFound("Customer not found.");


                var model = new SettingsModel
                {
                    MinGroupSize = cust.MinGroupSize,
                    Categories = cust.SurveyCategories.ToList(),
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
                return PartialView(viewName: "_Settings", model: model);
            }
        }

        [HttpGet]
        public ActionResult ChangeMinGroup(int size)
        {
            using (var db = new SimpleQDBEntities())
            {
                int minAllowed = db.DsgvoConstraints.Where(d => d.ConstrName == "MIN_GROUP_SIZE").FirstOrDefault().ConstrValue;
                if (size < minAllowed)
                    return new HttpStatusCodeResult(HttpStatusCode.Conflict, $"Size less than allowed ({minAllowed}).");

                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return HttpNotFound("Customer not found.");


                cust.MinGroupSize = size;
                db.SaveChanges();
                return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }
        }

        [HttpGet]
        public ActionResult AddCategory(string catName)
        {
            if (catName == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Category name must not be null.");

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.SurveyCategories.Where(c => c.CustCode == CustCode);
                if (cust.Count() == 0)
                    return HttpNotFound("Customer not found.");


                db.SurveyCategories.Add(new SurveyCategory
                {
                    CatId = cust.Max(c => c.CatId) + 1,
                    CatName = catName,
                    CustCode = CustCode
                });
                db.SaveChanges();
                return new HttpStatusCodeResult(HttpStatusCode.Created);
            }
        }

        [HttpGet]
        public ActionResult ModifyCategory(int catId, string catName)
        {
            if (catName == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Category name must not be null.");

            using (var db = new SimpleQDBEntities())
            {
                var cat = db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode).FirstOrDefault();
                if (cat == null)
                    return HttpNotFound("Category not found.");


                cat.CatName = catName;
                db.SaveChanges();
                return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }
        }

        [HttpGet]
        public ActionResult DeleteCategory(int catId)
        {
            using (var db = new SimpleQDBEntities())
            {
                var query = db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode);
                if (query.Count() == 0)
                    return HttpNotFound("Category not found.");

                if (db.Surveys.Where(s => s.CatId == catId && s.CustCode == CustCode).Count() != 0)
                    query.FirstOrDefault().Deactivated = true;
                else
                    db.SurveyCategories.RemoveRange(query);

                db.SaveChanges();
                return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }
        }

        [HttpPost]
        public ActionResult ChangePassword(string password)
        {
            if (password == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Password must not be null.");

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return HttpNotFound("Customer not found.");


                cust.CustPwdTmp = password;
                db.SaveChanges();
                return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }
        }

        [HttpPost]
        public ActionResult ChangeAnswerTypes(SettingsModel req)
        {
            if (req == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Model object must not be null.");

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return HttpNotFound("Customer not found.");

                if (req.CheckedAnswerTypes == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "CheckedAnswerTypes must not be null.");

                if (req.UncheckedAnswerTypes == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "UncheckedAnswerTypes must not be null.");

                foreach (var typeId in req.CheckedAnswerTypes.Concat(req.UncheckedAnswerTypes).Distinct())
                {
                    if (db.AnswerTypes.Where(a => a.TypeId == typeId).FirstOrDefault() == null)
                        return new HttpStatusCodeResult(HttpStatusCode.Conflict, "AnswerType does not exist.");
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

                return LoadSettings();
            }
        }

        [HttpPost]
        public ActionResult UpdateCustomer(SettingsModel req)
        {
            if (req == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Model object must not be null.");

            using (var db = new SimpleQDBEntities())
            {
                try
                {
                    if (db.PaymentMethods.Where(p => p.PaymentMethodId == req.PaymentMethodId).Count() == 0)
                        return new HttpStatusCodeResult(HttpStatusCode.Conflict, "PaymentMethod does not exist.");

                    if (req.DataStoragePeriod <= 0)
                        return new HttpStatusCodeResult(HttpStatusCode.Conflict, "DataStoragePeriod must be greater than 0");

                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                        return new HttpNotFoundResult("Customer not found.");


                    cust.CustName = req.Name ?? throw ANEx("CustName");
                    cust.CustEmail = req.Email ?? throw ANEx("CustEmail");
                    cust.Street = req.Street ?? throw ANEx("Street");
                    cust.Plz = req.Plz ?? throw ANEx("Plz");
                    cust.City = req.City ?? throw ANEx("City");
                    cust.Country = req.Country ?? throw ANEx("Country");
                    cust.LanguageCode = req.LanguageCode ?? throw ANEx("LanguageCode");
                    cust.DataStoragePeriod = req.DataStoragePeriod;
                    cust.PaymentMethodId = req.PaymentMethodId;
                    db.SaveChanges();

                    return LoadSettings();
                }
                catch (ArgumentNullException ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, $"{ex.ParamName} must not be null.");
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