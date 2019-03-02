using NLog;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SimpleQ.Webinterface.Extensions;
using System.Data.Entity;
using System.Net.Http;
using SimpleQ.Webinterface.Schedulers;
using SimpleQ.Webinterface.Properties;

namespace SimpleQ.Webinterface.Controllers
{
    public class CurrentController : BaseController
    {
        // GET: Current
        private string errString;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                logger.Debug("Loading current.");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Loading failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var model = new CurrentModel
                    {
                        Surveys = (await db.Surveys.Where(s => s.CustCode == CustCode && s.EndDate > DateTime.Now)
                            .ToListAsync())
                            .Select(g =>
                                new CurrentModel.SurveyWrapper
                                {
                                    SvyId = g.SvyId,
                                    SvyText = g.SvyText,
                                    StartDate = g.StartDate,
                                    EndDate = g.EndDate
                                })
                                .OrderByDescending(x => x.StartDate)
                             .ToList(),

                        PeriodicSurveys = db.Surveys.Where(s => s.CustCode == CustCode && s.Period != null).ToList()
                    };

                    ViewBag.emailConfirmed = cust.EmailConfirmed;
                    logger.Debug("Survey results loaded successfully.");

                    return View("Current", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[GET]Index: Unexpected error");
                return View("Error", model);
            }
        }
        #endregion

        #region AJAX-Methods
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
        public async Task<ActionResult> LoadSurveyInfo(int svyId)
        {
            try
            {
                logger.Debug($"Requested to load info of survey. (SvyId: {svyId}, CustCode: {CustCode})");
                using (var db = new SimpleQDBEntities())
                {
                    Survey svy = db.Surveys.Where(s => s.CustCode == CustCode && s.SvyId == svyId).First();
                    List<string> dps = svy.Departments.Select(x => x.DepName).ToList();
                    string startDate = svy.StartDate.ToShortDateString();
                    string endDate = svy.EndDate.ToShortDateString();
                    //return Json(new { svy.SvyText, svy.SurveyCategory, svy.AnswerType.TypeDesc, svy.StartDate, svy.EndDate, svy.Departments }, JsonRequestBehavior.AllowGet);
                    return Json(new { svy.SvyText, svy.SurveyCategory.CatName, svy.AnswerType.TypeDesc, startDate, endDate, dps}, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[GET]LoadSingleResult: Unexpected error");
                return PartialView("_Error", model);
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
    }
}