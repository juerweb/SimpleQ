using MigraDoc.Rendering;
using NLog;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                logger.Debug($"Loading settings: {CustCode}");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                    {
                        logger.Warn($"Loading failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

                    var bills = new List<SettingsModel.BillWrapper>();
                    foreach (var bill in db.Bills.Where(b => b.CustCode == CustCode).OrderByDescending(b => b.BillDate))
                    {
                        var lastBillDate = db.Bills.Where(b => b.CustCode == bill.CustCode)
                            .OrderByDescending(b => b.BillDate)
                            .Select(b => b.BillDate)
                            .Skip(1)
                            .FirstOrDefault();

                        var surveys = db.Surveys.Where(s => s.CustCode == bill.CustCode
                                                        && s.StartDate <= bill.BillDate
                                                        && s.EndDate > lastBillDate)
                                                .OrderBy(s => s.StartDate)
                                                .AsEnumerable()
                                                .Select(s => new { s, n = s.AnswerOptions.SelectMany(a => a.Votes).Distinct().Where(v => v.VoteDate > lastBillDate).Count() })
                                                .Select(x => new SettingsModel.SurveyWrapper
                                                {
                                                    Survey = x.s,
                                                    NumberOfAnswers = x.n,
                                                    SurveyPrice = Convert.ToDouble(x.s.PricePerClick * x.n)

                                                })
                                                .ToList();
                        bills.Add(new SettingsModel.BillWrapper { Bill = bill, Surveys = surveys });
                    }

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
                        PaymentMethodId = cust.PaymentMethodId,
                        Bills = bills
                    };

                    ViewBag.emailConfirmed = cust.EmailConfirmed;
                    logger.Debug("Settings loaded successfully");
                    return View("Settings", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[GET]Index: Unexpected error");
                return View("Error", model);
            }
        }

        [HttpPost]
        public ActionResult ChangeAnswerTypes(SettingsModel req)
        {
            try
            {
                logger.Debug($"Change answer types requested {CustCode}");
                bool err = false;

                if (req == null)
                    AddModelError("Model", "Model object must not be null.", ref err);

                using (var db = new SimpleQDBEntities())
                {
                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                    {
                        logger.Warn($"Changing answer types failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

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

                    if (err)
                    {
                        logger.Debug("ChangeAnswerTypes validation failed. Exiting action.");
                        return Index();
                    }

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
                    logger.Debug("Answer types changed successfully.");

                    return Index();
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[POST]ChangeAnswerRequest: Unexpected error");
                return View("Error", model);
            }
        }

        [HttpPost]
        public ActionResult UpdateCustomer(SettingsModel req)
        {
            try
            {
                logger.Debug($"Update customer requested {CustCode}");
                bool err = false;

                if (req == null)
                    AddModelError("Model", "Model object must not be null.", ref err);

                using (var db = new SimpleQDBEntities())
                {
                    try
                    {
                        var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                        if (cust == null)
                        {
                            logger.Warn($"Updating customer failed. Customer not found: {CustCode}");
                            return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                        }

                        var prevEmail = cust.CustEmail;

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

                        if (err)
                        {
                            logger.Debug("Update customer validation failed. Exiting action.");
                            return Index();
                        }

                        cust.DataStoragePeriod = req.DataStoragePeriod;
                        cust.PaymentMethodId = req.PaymentMethodId;

                        db.SaveChanges();
                        logger.Debug("Updated customer successfully");

                        return Index();
                    }
                    catch (ArgumentNullException ex)
                    {
                        AddModelError(ex.ParamName, $"{ex.ParamName} must not be null.", ref err);
                        logger.Debug("Update customer validation failed. Exiting action.");
                        return Index();
                    }
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[POST]UpdateCustomer: Unexpected error");
                return View("Error", model);
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpGet]
        public ActionResult ChangeMinGroup(int size)
        {
            try
            {
                logger.Debug($"ChangeMinGroup requested: {CustCode} (Size: {size})");
                using (var db = new SimpleQDBEntities())
                {
                    int minAllowed = db.DataConstraints.Where(c => c.ConstrName == "MIN_GROUP_SIZE").FirstOrDefault().ConstrValue;
                    if (size < minAllowed)
                        return Http.Conflict($"Size less than allowed ({minAllowed}).");

                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                    {
                        logger.Warn($"Changing Min Group failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }


                    cust.MinGroupSize = size;
                    db.SaveChanges();
                    logger.Debug("Changed Min Group successfully.");

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]ChangeMinGroup: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public ActionResult AddCategory(string catName)
        {
            try
            {
                logger.Debug($"AddCategory requested: {CustCode} (CatName: {catName})");
                if (string.IsNullOrEmpty(catName))
                {
                    logger.Debug("Adding category failed. CatName was null or empty.");
                    return Http.BadRequest("Category name must not be empty.");
                }

                using (var db = new SimpleQDBEntities())
                {
                    if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    {
                        logger.Warn($"Adding category failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var query = db.SurveyCategories.Where(c => c.CustCode == CustCode);

                    var cat = new SurveyCategory
                    {
                        CatId = (query.Count() == 0) ? 1 : query.Max(c => c.CatId) + 1,
                        CatName = catName,
                        CustCode = CustCode
                    };
                    db.SurveyCategories.Add(cat);
                    db.SaveChanges();
                    logger.Debug("Category added successfully.");

                    return Content($"{cat.CatId}", "text/plain");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]AddCategory: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public ActionResult ModifyCategory(int catId, string catName)
        {
            try
            {
                logger.Debug($"ModifyCategory requested: {CustCode} (CatId: {catId}, CatName: {catName})");
                if (string.IsNullOrEmpty(catName))
                {
                    logger.Debug("Modifying category failed. CatName was null or empty.");
                    return Http.BadRequest("Category name must not be empty.");
                }

                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Modifying category failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var cat = db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode).FirstOrDefault();
                    if (cat == null)
                    {
                        logger.Debug("Modifying category failed. Category not found");
                        return Http.NotFound("Category not found.");
                    }


                    cat.CatName = catName;
                    db.SaveChanges();
                    logger.Debug("Modifed category successfully.");

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]ModifyCategory: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public ActionResult DeleteCategory(int catId)
        {
            try
            {
                logger.Debug($"DeleteCategory requested: {CustCode} (CatId: {catId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Deleting category failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var cat = db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode).FirstOrDefault();
                    if (cat == null)
                    {
                        logger.Debug("Modifying category failed. Category not found");
                        return Http.NotFound("Category not found.");
                    }

                    if (db.Surveys.Where(s => s.CatId == catId && s.CustCode == CustCode).Count() != 0)
                    {
                        logger.Debug("Category will be deactivated due to still existing surveys.");
                        cat.Deactivated = true;
                    }
                    else
                    {
                        db.SurveyCategories.Remove(cat);
                    }

                    db.SaveChanges();
                    logger.Debug("Category deleted/deactivated successfully");

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]DeleteCategory: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public ActionResult DeleteTemplate(int svyId)
        {
            try
            {
                logger.Debug($"Delete template requested: {CustCode} (SvyId: {svyId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Deleting template failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var template = db.Surveys.Where(s => s.SvyId == svyId && s.Template && s.CustCode == CustCode).FirstOrDefault();
                    if (template == null)
                    {
                        logger.Debug("Deleting template failed. Template not found");
                        return Http.NotFound("Template does not exist.");
                    }

                    template.Template = false;
                    db.SaveChanges();
                    logger.Debug("Template deleted successfully.");

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]DeleteTemplate: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpPost]
        public ActionResult ChangePassword(string password)
        {
            try
            {
                logger.Debug($"Change password requested. {CustCode}");
                if (string.IsNullOrEmpty(password))
                {
                    logger.Debug("Changing password failed. Password was null or empty");
                    return Http.BadRequest("Password must not be null.");
                }

                using (var db = new SimpleQDBEntities())
                {
                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                    {
                        logger.Warn($"Changing password failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    cust.CustPwdTmp = password;
                    db.SaveChanges();
                    logger.Debug("Password changed successfully");

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]ChangePassword: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public ActionResult ChangeAccountingPeriod(int period)
        {
            try
            {
                logger.Debug("Change accounting period requested: {CustCode} (Period: {period})");
                if (!new[] { 1, 3, 6, 12 }.Contains(period))
                {
                    logger.Debug("Changing accounting period failed. Invalid period value.");
                    return Http.Conflict("Invalid period value");
                }

                using (var db = new SimpleQDBEntities())
                {
                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                    {
                        logger.Warn($"Changing accounting period failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    cust.AccountingPeriod = period;
                    db.SaveChanges();
                    logger.Debug("Accounting period changed successfully.");

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]ChangeAccountingPeriod: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }


        [HttpGet]
        public ActionResult ChangeAccountingDay(int day)
        {
            try
            {
                logger.Debug($"Change accounting day requested: {CustCode} (Day: {day})");
                if (day < 1 || day > 31)
                {
                    logger.Debug("Changing accounting day failed. Invalid day value");
                    return Http.Conflict("Invalid day value");
                }

                var date = DateTime.Now;
                day = (DateTime.DaysInMonth(date.Year, date.Month) < day) ? DateTime.DaysInMonth(date.Year, date.Month) : day;

                using (var db = new SimpleQDBEntities())
                {
                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                    {
                        logger.Warn($"Changing accounting period failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var old = cust.AccountingDate;
                    cust.AccountingDate = new DateTime(old.Year, old.Month, day);
                    db.SaveChanges();

                    logger.Debug($"Accounting day changed successfully: {cust.AccountingDate}");

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]ChangeAccountingDay: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }

        }

        [HttpGet]
        public ActionResult DownloadBill(int billId)
        {
            try
            {
                logger.Debug($"Download bill requested: {CustCode} (BillId: {billId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Downloading bill failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }


                    var bill = db.Bills.Where(b => b.BillId == billId && b.CustCode == CustCode).FirstOrDefault();
                    if (bill == null)
                    {
                        logger.Debug("Downloading bill failed. Bill not found");
                        return Http.NotFound("Bill not found");
                    }

                    var lastBillDate = db.Bills.Where(b => b.CustCode == bill.CustCode)
                                    .OrderByDescending(b => b.BillDate)
                                    .Select(b => b.BillDate)
                                    .Skip(1)
                                    .FirstOrDefault();

                    var surveys = db.Surveys
                        .Where(s => s.CustCode == bill.CustCode
                                && s.StartDate <= bill.BillDate
                                && s.EndDate > lastBillDate)
                        .OrderBy(s => s.StartDate)
                        .ToArray();

                    var stream = new MemoryStream();

                    if (Pdf.CreateBill(ref stream, bill, surveys, HttpContext.Server.MapPath("~/Content/Images/Logos/simpleQ.png"), lastBillDate))
                    {
                        logger.Debug("Bill created successfully.");
                        return File(stream, "application/pdf");
                    }
                    else
                    {
                        logger.Error($"Creating bill failed: {CustCode} (BillId: {billId})");
                        return Http.ServiceUnavailable("Downloading currently not available. Please try again later.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]DownloadBill: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
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

        private string CustCode
        {
            get
            {
                try
                {
                    return HttpContext.GetOwinContext().Authentication.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "CustCode: Unexpected error");
                    throw ex;
                }
            }
        }
        #endregion
    }
}