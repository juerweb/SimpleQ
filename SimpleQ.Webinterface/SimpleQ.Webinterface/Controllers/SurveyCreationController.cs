using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Models.Enums;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Properties;
using SimpleQ.Webinterface.Schedulers;
using System.Net.Http;
using Newtonsoft.Json;
using NLog;
using System.Threading.Tasks;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class SurveyCreationController : BaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                logger.Debug("Loading survey creation");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Loading failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = BackendResources.CustomerNotFoundTitle, Message = BackendResources.CustomerNotFoundMsg });
                    }

                    var model = new SurveyCreationModel
                    {
                        SurveyCategories = cust.SurveyCategories.Where(s => !s.Deactivated).ToList(),
                        AnswerTypes = cust.AnswerTypes.ToList(),
                        Departments = cust.Departments.Select(d => new { Department = d, Amount = d.People.Count }).ToDictionary(x => x.Department, x => x.Amount),
                        SurveyTemplates = await db.Surveys.Where(s => s.CustCode == CustCode && s.Template).ToListAsync()
                    };

                    ViewBag.emailConfirmed = cust.EmailConfirmed;
                    logger.Debug("Survey creation loaded successfully");
                    return View("SurveyCreation", model);
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
        public async Task<ActionResult> New(SurveyCreationModel req)
        {
            try
            {
                logger.Debug($"Requested to create new survey (CustCode: {CustCode}, SvyText: {req.Survey.SvyText}");
                bool err = false;

                if (req == null)
                    AddModelError("Model", BackendResources.ModelNull, ref err);
                if (req.Survey == null)
                    AddModelError("Survey", BackendResources.SurveyNull, ref err);
                if (string.IsNullOrEmpty(req.Survey.SvyText))
                    AddModelError("Survey.SvyText", BackendResources.SurveyTextEmpty, ref err);
                if (req.SelectedDepartments == null || req.SelectedDepartments.Count() == 0)
                    AddModelError("SelectedDepartments", BackendResources.SelectedDepartmentsEmpty, ref err);

                using (var db = new SimpleQDBEntities())
                {
                    if (await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync() == null)
                    {
                        logger.Warn($"Creating new survey failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = BackendResources.CustomerNotFoundTitle, Message = BackendResources.CustomerNotFoundMsg });
                    }

                    if (!(await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync()).EmailConfirmed)
                    {
                        logger.Debug($"Creating new survey failed. Email not confirmed: {CustCode}");
                        return View("Error", new ErrorModel { Title = BackendResources.ConfirmYourEmailTitle, Message = BackendResources.ConfirmYourEmailMsg });
                    }

                    req.Survey.CustCode = CustCode;
                    req.Survey.StartDate = req.StartDate.Date.Add(req.StartTime);
                    req.Survey.EndDate = req.EndDate.Date.Add(req.EndTime);
                    req.Survey.Sent = false;
                    TimeSpan? period = req.Period.HasValue ? (TimeSpan?)(req.IsWeek == true ? TimeSpan.FromDays(req.Period.Value * 7) : TimeSpan.FromDays(req.Period.Value)) : null;
                    req.Survey.Period = period?.Ticks;

                    if (req.Period.HasValue && period.Value < TimeSpan.FromDays(1))
                        AddModelError("Period", BackendResources.PeriodAtLeast1Day, ref err);

                    if (req.Period.HasValue && req.Survey.StartDate.Add(period.Value) < req.Survey.EndDate)
                        AddModelError("Period", BackendResources.PeriodMustBiggerThanDuration, ref err);

                    if (req.Survey.StartDate >= req.Survey.EndDate)
                        AddModelError("Survey.StartDate", BackendResources.StartDateMustEarlierEndDate, ref err);

                    if (req.Survey.Amount <= 0)
                        AddModelError("Survey.Amount", BackendResources.AmountMustAtLeast1, ref err);

                    if (await db.SurveyCategories.Where(c => c.CatId == req.Survey.CatId && c.CustCode == CustCode).FirstOrDefaultAsync() == null)
                        AddModelError("Survey.CatId", BackendResources.CategoryNotFound, ref err);

                    if (await db.AnswerTypes.Where(a => a.TypeId == req.Survey.TypeId).FirstOrDefaultAsync() == null)
                        AddModelError("Survey.TypeId", BackendResources.AnswerTypeNotExist, ref err);

                    var baseId = (await db.AnswerTypes.Where(a => a.TypeId == req.Survey.TypeId).FirstOrDefaultAsync()).BaseId;
                    if (baseId != (int)BaseQuestionTypes.FixedAnswerQuestion && baseId != (int)BaseQuestionTypes.OpenQuestion
                        && (req.TextAnswerOptions == null || req.TextAnswerOptions.Count() == 0))
                        AddModelError("TextAnswerOptions", BackendResources.SubmitAnswerOptions, ref err);

                    if (baseId == (int)BaseQuestionTypes.DichotomousQuestion
                        && (req.TextAnswerOptions == null || req.TextAnswerOptions.Count() != 2))
                        AddModelError("TextAnswerOptions", BackendResources.Submit2AnswerOptions, ref err);

                    if (baseId == (int)BaseQuestionTypes.LikertScaleQuestion
                        && (req.TextAnswerOptions == null || req.TextAnswerOptions.Count() != 2))
                        AddModelError("TextAnswerOptions", BackendResources.Submit2AnswerOptions, ref err);

                    foreach (var depId in req.SelectedDepartments ?? new List<int>())
                    {
                        if (await db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefaultAsync() == null)
                        {
                            AddModelError("SelectedDepartments", BackendResources.DepNotFound, ref err);
                            break;
                        }
                    }

                    if (req.TextAnswerOptions != null)
                    {
                        foreach (var ans in req.TextAnswerOptions ?? new List<string>())
                        {
                            if (string.IsNullOrEmpty(ans))
                            {
                                AddModelError("TextAnswerOptions", BackendResources.AnswerOptionsNotEmpty, ref err);
                                break;
                            }
                        }
                    }

                    if (err)
                    {
                        logger.Debug("Creating new survey failed due to model validation failure. Exiting action");
                        return await Index();
                    }


                    int totalPeople = await db.Departments.Where(d => req.SelectedDepartments.Contains(d.DepId) && d.CustCode == CustCode)
                        .SelectMany(d => d.People).Distinct().CountAsync();
                    if (req.Survey.Amount > totalPeople)
                        req.Survey.Amount = totalPeople;

                    logger.Debug($"Total people amount: {totalPeople} (CustCode: {CustCode}, SvyText: {req.Survey.SvyText})");

                    db.Surveys.Add(req.Survey);
                    await db.SaveChangesAsync();
                    logger.Debug($"Survey successfully created. SvyId: {req.Survey.SvyId}");

                    foreach (var depId in req.SelectedDepartments)
                    {
                        req.Survey.Departments.Add(await db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefaultAsync());
                    };
                    await db.SaveChangesAsync();
                    logger.Debug($"Departments added successfully for SvyId: {req.Survey.SvyId}");

                    if (baseId == (int)BaseQuestionTypes.LikertScaleQuestion)
                    {
                        db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = req.TextAnswerOptions[0], FirstPosition = true });
                        db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = req.TextAnswerOptions[1], FirstPosition = false });
                        logger.Debug($"Likert scale answer options set successfully for SvyId: {req.Survey.SvyId}");
                    }
                    else if (baseId != (int)BaseQuestionTypes.FixedAnswerQuestion && baseId != (int)BaseQuestionTypes.OpenQuestion)
                    {
                        req.TextAnswerOptions.ForEach(text =>
                        {
                            db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = text });
                        });
                        logger.Debug($"Answer options set successfully for SvyId: {req.Survey.SvyId}");
                    }
                    await db.SaveChangesAsync();
                }

                // Only queue survey if starting before 00:10
                if (req.Survey.StartDate < Literal.NextMidnight.Add(TimeSpan.FromMinutes(10)))
                {
                    logger.Debug($"Scheduling survey {req.Survey.SvyId}");
                    var success = await SurveyQueue.EnqueueSurvey(req.Survey.SvyId, req.Survey.StartDate, CustCode);
                    logger.Debug($"Survey {req.Survey.SvyId} {(success ? "" : "not")} scheduled {(success ? "successfully" : "")}");
                }

                logger.Debug("Creating new survey finished successfully");
                return await Index();
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[POST]New: Unexpected error");
                return View("Error", model);
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpGet]
        public async Task<ActionResult> LoadTemplate(int svyId)
        {
            try
            {
                logger.Debug($"Requested loading survey template (CustCode: {CustCode}, SvyId: {svyId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Loading template failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    db.Configuration.LazyLoadingEnabled = false;
                    var survey = await db.Surveys
                        .Include("Departments")
                        .Include("AnswerOptions")
                        .Where(s => s.SvyId == svyId && s.CustCode == CustCode && s.Template)
                        .FirstOrDefaultAsync();

                    if (survey == null)
                    {
                        logger.Debug($"Loading template failed. Survey template not found: {svyId}");
                        return Http.NotFound("Template not found.");
                    }

                    logger.Debug($"Survey template loaded successfully: {svyId}");
                    return Json(survey, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]LoadTemplate: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }

        }

        [HttpGet]
        public ActionResult CreateCategory(string catName)
        {
            try
            {
                logger.Debug($"Requested to create category: {catName} (CustCode: {CustCode})");
                logger.Debug("Redirecting to Settings/AddCategory");
                return RedirectToAction("AddCategory", "Settings", new { catName });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]CreateCategory: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public async Task<ActionResult> CancelSurvey(int svyId)
        {
            try
            {
                logger.Debug($"Requested to cancel survey {svyId}");
                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Cancelling survey failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var survey = await db.Surveys.Where(s => s.SvyId == svyId && s.CustCode == CustCode).FirstOrDefaultAsync();
                    if (survey == null)
                    {
                        logger.Debug($"Cancelling survey failed. Survey not found: {svyId}");
                        return Http.NotFound("Survey not found.");
                    }

                    if (survey.Sent)
                    {
                        logger.Debug("Survey already sent. Sending cancellation request");
                        using (var client = new HttpClient())
                        {
                            foreach (var dep in survey.Departments.Where(c => c.CustCode == CustCode).ToList())
                            {
                                foreach (var p in dep.People.ToList())
                                {
                                    try
                                    {
                                        // SEND SURVEY
                                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "ZDNmNGZjODMtNTEzNC00YjA1LTkyZmUtNDRkMWJkZjRhZjVj");
                                        var obj = new
                                        {
                                            app_id = "68b8996a-f664-4130-9854-9ed7f70d5540",
                                            include_player_ids = new string[] { p.DeviceId },
                                            contents = new { en = "Cancel survey" },
                                            data = new { Cancel = true, SvyId = svyId }
                                        };
                                        var response = await client.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", obj);

                                        if (!response.IsSuccessStatusCode)
                                        {
                                            logger.Error($"Failed cancelling survey (StatusCode: {response.StatusCode}, Content: {response.Content})");
                                        }
                                        else
                                        {
                                            logger.Debug($"Survey cancelled successfully (StatusCode: {response.StatusCode}, Content: {response.Content})");
                                        }
                                    }
                                    catch (AggregateException ex)
                                    {
                                        logger.Error(ex, $"Error while sending survey to app (SvyId: {svyId}, CustCode: {CustCode}, PersId: {p.PersId}, DeviceId: {p.DeviceId})");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Debug("Survey not sent yet. Removing from queue");
                        await SurveyQueue.DequeueSurvey(svyId);
                    }
                    await Task.Run(() => db.sp_DeleteSurvey(svyId));
                    await db.SaveChangesAsync();

                    logger.Debug($"Survey cancelled successfully: {svyId}");
                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]CancelSurvey: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<double> LoadPricePerClick(int amount)
        {
            try
            {
                logger.Trace($"Requested to load price per click for amount: {amount}");
                using (var db = new SimpleQDBEntities())
                {
                    var price = Convert.ToDouble(await Task.Run(() => db.fn_CalcPricePerClick(amount, CustCode ?? "")));

                    logger.Trace($"Price per click for {amount} people: ");
                    return price;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]LoadPricePerClick: Unexpected error");
                return double.NaN;
            }
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public void Test()
        //{
        //    using (var client = new WebClient())
        //    {
        //        var obj = new
        //        {
        //            app_id = "68b8996a-f664-4130-9854-9ed7f70d5540",
        //            include_player_ids = new string[] { "null" },
        //            contents = new { en = "New survey" },
        //            content_available = true,
        //            data = new { Cancel = false, SvyId = 19 }
        //        };
        //        client.Headers.Add("Authorization", "Basic ZDNmNGZjODMtNTEzNC00YjA1LTkyZmUtNDRkMWJkZjRhZjVj");
        //        client.Headers.Add("Content-Type", "application/json");
        //        client.UploadString(new Uri("https://onesignal.com/api/v1/notifications", UriKind.Absolute), JsonConvert.SerializeObject(obj));
        //    }
        //}
        #endregion

        #region Helpers
        private ArgumentNullException ANEx(string paramName) => new ArgumentNullException(paramName);
        #endregion
    }
}