using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class SettingsController : Controller
    {
        #region MVC-Actions
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
                    Templates = db.Surveys.Where(s => s.CustCode == CustCode && s.Template).ToList(),
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
                return View("Settings", model);
            }
        }

        [HttpPost]
        public ActionResult ChangeAnswerTypes(SettingsModel req)
        {
            bool err = false;

            if (req == null)
                AddModelError("Model", "Model object must not be null.", ref err);

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return Http.NotFound("Customer not found.");

                if (req.CheckedAnswerTypes == null)
                    AddModelError("CheckedAnswerTypes", "AnswerTypes must not be null.", ref err);

                foreach (var typeId in req.CheckedAnswerTypes.Distinct())
                {
                    if (db.AnswerTypes.Where(a => a.TypeId == typeId).FirstOrDefault() == null)
                    {
                        AddModelError("CheckedAnswerTypes", "AnswerType does not exist.", ref err);
                        break;
                    }
                }

                if (err) return Index();

                var uncheckedAnswerTypes = db.AnswerTypes.Select(a => a.TypeId).Except(req.CheckedAnswerTypes).ToList();

                uncheckedAnswerTypes.ForEach(typeId =>
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
            bool err = false;

            if (req == null)
                AddModelError("Model", "Model object must not be null.", ref err);

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
                        AddModelError("PaymentMethodId", "PaymentMethod does not exist.", ref err);

                    if (req.DataStoragePeriod <= 0)
                        AddModelError("DataStoragePeriod", "DataStoragePeriod must be greater than 0.", ref err);

                    if (err) return Index();

                    cust.DataStoragePeriod = req.DataStoragePeriod;
                    cust.PaymentMethodId = req.PaymentMethodId;

                    db.SaveChanges();

                    return Index();
                }
                catch (ArgumentNullException ex)
                {
                    AddModelError(ex.ParamName, $"{ex.ParamName} must not be null.", ref err);
                    return Index();
                }
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpGet]
        public ActionResult ChangeMinGroup(int size)
        {
            using (var db = new SimpleQDBEntities())
            {
                int minAllowed = db.DataConstraints.Where(c => c.ConstrName == "MIN_GROUP_SIZE").FirstOrDefault().ConstrValue;
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

        [HttpGet]
        public ActionResult DeleteTemplate(int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                var template = db.Surveys.Where(s => s.SvyId == svyId && s.Template && s.CustCode == CustCode).FirstOrDefault();
                if (template == null)
                    return Http.NotFound("Template does not exist.");

                template.Template = false;
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
        #endregion

        #region Helpers
        private void AddModelError(string key, string errorMessage, ref bool error)
        {
            ModelState.AddModelError(key, errorMessage);
            error = true;
        }

        private ArgumentNullException ANEx(string paramName) => new ArgumentNullException(paramName);

        private string CustCode => HttpContext.GetOwinContext().Authentication.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
        #endregion
    }
}