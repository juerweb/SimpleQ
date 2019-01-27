using NLog;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Properties;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class SettingsController : BaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                logger.Debug($"Loading settings: {CustCode}");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Loading failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = BackendResources.CustomerNotFoundTitle, Message = BackendResources.CustomerNotFoundMsg });
                    }

                    var bills = new List<SettingsModel.BillWrapper>();
                    foreach (var bill in await db.Bills.Where(b => b.CustCode == CustCode).OrderByDescending(b => b.BillDate).ToListAsync())
                    {
                        var lastBillDate = await db.Bills.Where(b => b.CustCode == bill.CustCode)
                            .OrderByDescending(b => b.BillDate)
                            .Select(b => b.BillDate)
                            .Skip(1)
                            .FirstOrDefaultAsync();

                        var surveys = (await db.Surveys.Where(s => s.CustCode == bill.CustCode
                                                        && s.StartDate <= bill.BillDate
                                                        && s.EndDate > lastBillDate)
                                                .OrderBy(s => s.StartDate)
                                                .ToListAsync())
                                                .Select(s => new { s, n = s.AnswerOptions.SelectMany(a => a.Votes).Distinct().Where(v => v.VoteDate > lastBillDate).Count() })
                                                .Select(x => new SettingsModel.SurveyWrapper
                                                {
                                                    Survey = x.s,
                                                    NumberOfAnswers = x.n,
                                                    SurveyPrice = Convert.ToDouble(x.s.PricePerClick * x.n)

                                                })
                                                .ToList();
                        bills.Add(new SettingsModel.BillWrapper { Bill = bill, Surveys = surveys });
                    };

                    var last = cust.Bills.Select(b => b.BillDate).DefaultIfEmpty().Max();
                    var currentSurveyAmount = await db.Surveys.Where(s => s.StartDate >= last && s.CustCode == CustCode).CountAsync();
                    var currentVoteAmount = await db.Votes.Where(v => v.VoteDate >= last && v.AnswerOptions.All(a => a.Survey.CustCode == CustCode)).CountAsync();   //currentSurveys.SelectMany(s => s.AnswerOptions.SelectMany(a => a.Votes)).Count();

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
                        Bills = bills,
                        OutstandingBalance = decimal.ToDouble(cust.CostBalance),
                        CurrentSurveyAmount = currentSurveyAmount,
                        CurrentVoteAmount = currentVoteAmount,
                        PeriodicSurveys = db.Surveys.Where(s => s.CustCode == CustCode && s.Period != null).ToList(),
                        AccountingDay = cust.AccountingDate.Day,
                        AccountingPeriod = cust.AccountingPeriod
                    };

                    ViewBag.emailConfirmed = cust.EmailConfirmed;
                    logger.Debug("Settings loaded successfully");
                    return View("Settings", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[GET]Index: Unexpected error");
                return View("Error", model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> ChangeAnswerTypes(SettingsModel req)
        {
            try
            {
                logger.Debug($"Change answer types requested {CustCode}");
                bool err = false;

                if (req == null)
                    AddModelError("Model", BackendResources.ModelNull, ref err);

                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Changing answer types failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = BackendResources.CustomerNotFoundTitle, Message = BackendResources.CustomerNotFoundMsg });
                    }

                    if (req.CheckedAnswerTypes == null)
                        AddModelError("CheckedAnswerTypes", BackendResources.AnswerTypesNull, ref err);

                    foreach (var typeId in req.CheckedAnswerTypes.Distinct())
                    {
                        if (await db.AnswerTypes.Where(a => a.TypeId == typeId).FirstOrDefaultAsync() == null)
                        {
                            AddModelError("CheckedAnswerTypes", BackendResources.AnswerTypeDoesNotExist, ref err);
                            break;
                        }
                    }

                    if (err)
                    {
                        logger.Debug("ChangeAnswerTypes validation failed. Exiting action.");
                        return await Index();
                    }

                    var uncheckedAnswerTypes = await db.AnswerTypes.Select(a => a.TypeId).Except(req.CheckedAnswerTypes).ToListAsync();

                    foreach (var typeId in uncheckedAnswerTypes)
                    {
                        cust.AnswerTypes.Remove(await db.AnswerTypes.Where(a => a.TypeId == typeId).FirstOrDefaultAsync());
                    };

                    foreach (var typeId in req.CheckedAnswerTypes)
                    {
                        cust.AnswerTypes.Add(await db.AnswerTypes.Where(a => a.TypeId == typeId).FirstOrDefaultAsync());
                    };

                    await db.SaveChangesAsync();
                    logger.Debug("Answer types changed successfully.");

                    return await Index();
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[POST]ChangeAnswerRequest: Unexpected error");
                return View("Error", model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCustomer(SettingsModel req)
        {
            try
            {
                logger.Debug($"Update customer requested {CustCode}");
                bool err = false;

                if (req == null)
                    AddModelError("Model", BackendResources.ModelNull, ref err);

                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Updating customer failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = BackendResources.CustomerNotFoundTitle, Message = BackendResources.CustomerNotFoundMsg });
                    }

                    if (string.IsNullOrEmpty(req.Name))
                        AddModelError("CustName", BackendResources.NameEmpty, ref err);

                    if (string.IsNullOrEmpty(req.Email))
                        AddModelError("CustEmail", BackendResources.EmailEmpty, ref err);

                    if (string.IsNullOrEmpty(req.Street))
                        AddModelError("Street", BackendResources.StreetEmpty, ref err);

                    if (string.IsNullOrEmpty(req.Plz))
                        AddModelError("Plz", BackendResources.PlzEmpty, ref err);

                    if (string.IsNullOrEmpty(req.City))
                        AddModelError("City", BackendResources.CityEmpty, ref err);

                    if (string.IsNullOrEmpty(req.Country))
                        AddModelError("Country", BackendResources.CountryEmpty, ref err);

                    if (string.IsNullOrEmpty(req.LanguageCode))
                        AddModelError("LanguageCode", BackendResources.LangCodeEmpty, ref err);

                    if (req.DataStoragePeriod <= 0)
                        AddModelError("DataStoragePeriod", BackendResources.DataStoragePeriodInvalid, ref err);

                    if (await db.PaymentMethods.Where(p => p.PaymentMethodId == req.PaymentMethodId).CountAsync() == 0)
                        AddModelError("PaymentMethodId", BackendResources.PaymentMethodInvalid, ref err);

                    if (err)
                    {
                        logger.Debug("Update customer validation failed. Exiting action.");
                        return await Index();
                    }

                    var prevEmail = cust.CustEmail;

                    cust.CustName = req.Name;
                    cust.CustEmail = req.Email;
                    cust.Street = req.Street;
                    cust.Plz = req.Plz;
                    cust.City = req.City;
                    cust.Country = req.Country;
                    cust.LanguageCode = req.LanguageCode;
                    cust.DataStoragePeriod = req.DataStoragePeriod;
                    cust.PaymentMethodId = req.PaymentMethodId;

                    await db.SaveChangesAsync();
                    logger.Debug("Updated customer successfully");

                    return await Index();
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[POST]UpdateCustomer: Unexpected error");
                return View("Error", model);
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpGet]
        public async Task<ActionResult> ChangeMinGroup(int size)
        {
            try
            {
                logger.Debug($"ChangeMinGroup requested: {CustCode} (Size: {size})");
                using (var db = new SimpleQDBEntities())
                {
                    int minAllowed = (await db.DataConstraints.Where(c => c.ConstrName == "MIN_GROUP_SIZE").FirstOrDefaultAsync()).ConstrValue;
                    if (size < minAllowed)
                        return Http.Conflict($"Size less than allowed ({minAllowed}).");

                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Changing Min Group failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }


                    cust.MinGroupSize = size;
                    await db.SaveChangesAsync();
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
        public async Task<ActionResult> AddCategory(string catName)
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
                    if (await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync() == null)
                    {
                        logger.Warn($"Adding category failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var query = db.SurveyCategories.Where(c => c.CustCode == CustCode);

                    var cat = new SurveyCategory
                    {
                        CatId = (await query.CountAsync() == 0) ? 1 : await query.MaxAsync(c => c.CatId) + 1,
                        CatName = catName,
                        CustCode = CustCode
                    };
                    db.SurveyCategories.Add(cat);
                    await db.SaveChangesAsync();
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
        public async Task<ActionResult> ModifyCategory(int catId, string catName)
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
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Modifying category failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var cat = await db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cat == null)
                    {
                        logger.Debug("Modifying category failed. Category not found");
                        return Http.NotFound("Category not found.");
                    }


                    cat.CatName = catName;
                    await db.SaveChangesAsync();
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
        public async Task<ActionResult> DeleteCategory(int catId)
        {
            try
            {
                logger.Debug($"DeleteCategory requested: {CustCode} (CatId: {catId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Deleting category failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var cat = await db.SurveyCategories.Where(c => c.CatId == catId && c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cat == null)
                    {
                        logger.Debug("Modifying category failed. Category not found");
                        return Http.NotFound("Category not found.");
                    }

                    if (await db.Surveys.Where(s => s.CatId == catId && s.CustCode == CustCode).CountAsync() != 0)
                    {
                        logger.Debug("Category will be deactivated due to still existing surveys.");
                        cat.Deactivated = true;
                    }
                    else
                    {
                        db.SurveyCategories.Remove(cat);
                    }

                    await db.SaveChangesAsync();
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
        public async Task<ActionResult> DeleteTemplate(int svyId)
        {
            try
            {
                logger.Debug($"Delete template requested: {CustCode} (SvyId: {svyId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Deleting template failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var template = await db.Surveys.Where(s => s.SvyId == svyId && s.Template && s.CustCode == CustCode).FirstOrDefaultAsync();
                    if (template == null)
                    {
                        logger.Debug("Deleting template failed. Template not found");
                        return Http.NotFound("Template does not exist.");
                    }

                    template.Template = false;
                    await db.SaveChangesAsync();
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
        public async Task<ActionResult> ChangePassword(string password)
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
                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Changing password failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    cust.CustPwdTmp = password;
                    await db.SaveChangesAsync();
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
        public async Task<ActionResult> ChangeAccountingPeriod(int period)
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
                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Changing accounting period failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    cust.AccountingPeriod = period;
                    await db.SaveChangesAsync();
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
        public async Task<ActionResult> ChangeAccountingDay(int day)
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
                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Changing accounting period failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var old = cust.AccountingDate;
                    cust.AccountingDate = new DateTime(old.Year, old.Month, day);
                    await db.SaveChangesAsync();

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
        public async Task<ActionResult> DownloadBill(int billId)
        {
            try
            {
                logger.Debug($"Download bill requested: {CustCode} (BillId: {billId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Downloading bill failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }


                    var bill = await db.Bills.Where(b => b.BillId == billId && b.CustCode == CustCode).FirstOrDefaultAsync();
                    if (bill == null)
                    {
                        logger.Debug("Downloading bill failed. Bill not found");
                        return Http.NotFound("Bill not found");
                    }

                    var lastBillDate = await db.Bills.Where(b => b.CustCode == bill.CustCode)
                                    .OrderByDescending(b => b.BillDate)
                                    .Select(b => b.BillDate)
                                    .Skip(1)
                                    .FirstOrDefaultAsync();

                    var surveys = await db.Surveys
                        .Where(s => s.CustCode == bill.CustCode
                                && s.StartDate <= bill.BillDate
                                && s.EndDate > lastBillDate)
                        .OrderBy(s => s.StartDate)
                        .ToArrayAsync();

                    var stream = new MemoryStream();

                    if (Pdf.CreateBill(ref stream, bill.Customer, bill, surveys, HttpContext.Server.MapPath("~/Content/Images/Logos/simpleQ.png"), lastBillDate))
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

        [HttpDelete]
        public async Task<ActionResult> StopPeriodic(int svyId)
        {
            try
            {
                logger.Debug($"Stopping periodic survey requested: {CustCode} (SvyId: {svyId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Stopping periodic survey failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var svy = await db.Surveys.Where(s => s.SvyId == svyId && s.Period != null).FirstOrDefaultAsync();
                    if (svy == null)
                    {
                        logger.Debug("Stopping periodic survey. Survey not found");
                        return Http.NotFound("Survey not found");
                    }

                    svy.Period = null;
                    await db.SaveChangesAsync();

                    logger.Debug("Periodic survey stopped successfully");
                    return Http.Ok/*e gut*/();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[DELETE]StopPerodic: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }
        #endregion

        #region Helpers
        private ArgumentNullException ANEx(string paramName, string msg) => new ArgumentNullException(paramName, msg);
        #endregion
    }
}