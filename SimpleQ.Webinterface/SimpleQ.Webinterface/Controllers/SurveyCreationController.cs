using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Models.Enums;
using SimpleQ.Webinterface.Extensions;
using System.Net.Http;
using Newtonsoft.Json;
using System.Security.Claims;
using NLog;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class SurveyCreationController : Controller
    {
        private static readonly HashSet<int> queuedSurveys = new HashSet<int>();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                logger.Debug("Loading survey creation");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                    {
                        logger.Warn($"Loading failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

                    var model = new SurveyCreationModel
                    {
                        SurveyCategories = cust.SurveyCategories.Where(s => !s.Deactivated).ToList(),
                        AnswerTypes = cust.AnswerTypes.ToList(),
                        Departments = cust.Departments.Select(d => new { Department = d, Amount = d.People.Count }).ToDictionary(x => x.Department, x => x.Amount),
                        SurveyTemplates = db.Surveys.Where(s => s.CustCode == CustCode && s.Template).ToList()
                    };

                    ViewBag.emailConfirmed = cust.EmailConfirmed;
                    logger.Debug("Survey creation loaded successfully");
                    return View("SurveyCreation", model);
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
        public ActionResult New(SurveyCreationModel req)
        {
            try
            {
                logger.Debug($"Requested to create new survey (CustCode: {CustCode}, SvyText: {req.Survey.SvyText}");
                bool err = false;

                if (req == null)
                    AddModelError("Model", "Model object must not be null.", ref err);
                if (req.Survey == null)
                    AddModelError("Survey", "Survey must not be null.", ref err);
                if (req.Survey.SvyText == null)
                    AddModelError("Survey.SvyText", "SvyText must not be null.", ref err);
                if (req.SelectedDepartments == null || req.SelectedDepartments.Count() == 0)
                    AddModelError("SelectedDepartments", "SelectedDepartments object must not be null or empty.", ref err);

                using (var db = new SimpleQDBEntities())
                {
                    if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    {
                        logger.Warn($"Creating new survey failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

                    if (!db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().EmailConfirmed)
                    {
                        logger.Debug($"Creating new survey failed. Email not confirmed: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Confirm your e-mail", Message = "Please confirm your e-mail address before creating surveys." });
                    }

                    req.Survey.CustCode = CustCode;
                    req.Survey.StartDate = req.StartDate.Date.Add(req.StartTime);
                    req.Survey.EndDate = req.EndDate.Date.Add(req.EndTime);
                    req.Survey.Sent = false;

                    if (req.Survey.StartDate >= req.Survey.EndDate)
                        AddModelError("Survey.StartDate", "StartDate must be earlier than EndDate.", ref err);

                    if (req.Survey.Amount <= 0)
                        AddModelError("Survey.Amount", "Amount must be at least 1.", ref err);

                    if (db.SurveyCategories.Where(c => c.CatId == req.Survey.CatId && c.CustCode == CustCode).FirstOrDefault() == null)
                        AddModelError("Survey.CatId", "Category not found.", ref err);

                    if (db.AnswerTypes.Where(a => a.TypeId == req.Survey.TypeId).FirstOrDefault() == null)
                        AddModelError("Survey.TypeId", "AnswerType does not exist.", ref err);

                    var baseId = db.AnswerTypes.Where(a => a.TypeId == req.Survey.TypeId).FirstOrDefault().BaseId;
                    if (baseId != (int)BaseQuestionTypes.FixedAnswerQuestion && baseId != (int)BaseQuestionTypes.OpenQuestion
                        && (req.TextAnswerOptions == null || req.TextAnswerOptions.Count() == 0))
                        AddModelError("TextAnswerOptions", "There must be submitted some AnswerOptions.", ref err);

                    if (baseId == (int)BaseQuestionTypes.DichotomousQuestion
                        && (req.TextAnswerOptions == null || req.TextAnswerOptions.Count() != 2))
                        AddModelError("TextAnswerOptions", "There must be submitted exactly two AnswerOptions.", ref err);

                    if (baseId == (int)BaseQuestionTypes.LikertScaleQuestion
                        && (req.TextAnswerOptions == null || req.TextAnswerOptions.Count() != 2))
                        AddModelError("TextAnswerOptions", "There must be submitted exactly two AnswerOptions.", ref err);

                    foreach (var depId in req.SelectedDepartments)
                    {
                        if (db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault() == null)
                        {
                            AddModelError("SelectedDepartments", "Department not found.", ref err);
                            break;
                        }
                    }

                    if (req.TextAnswerOptions != null)
                    {
                        foreach (var ans in req.TextAnswerOptions)
                        {
                            if (string.IsNullOrEmpty(ans))
                            {
                                AddModelError("TextAnswerOptions", "AnswerOptions must not be empty.", ref err);
                                break;
                            }
                        }
                    }

                    if (err)
                    {
                        logger.Debug("Creating new survey failed due to model validation failure. Exiting action");
                        return Index();
                    }


                    int totalPeople = db.Departments.Where(d => req.SelectedDepartments.Contains(d.DepId) && d.CustCode == CustCode)
                        .SelectMany(d => d.People).Distinct().Count();
                    if (req.Survey.Amount > totalPeople)
                        req.Survey.Amount = totalPeople;

                    logger.Debug($"Total people amount: {totalPeople} (CustCode: {CustCode}, SvyText: {req.Survey.SvyText})");

                    db.Surveys.Add(req.Survey);
                    db.SaveChanges();
                    logger.Debug($"Survey successfully created. SvyId: {req.Survey.SvyId}");

                    req.SelectedDepartments.ForEach(depId =>
                    {
                        req.Survey.Departments.Add(db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault());
                    });
                    db.SaveChanges();
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
                    db.SaveChanges();
                }

                TimeSpan timeout = req.Survey.StartDate - DateTime.Now;
                logger.Debug($"Timeout until sending survey {req.Survey.SvyId}: {timeout.ToString(@"hh\:mm\:ss\.fff")}");

                // Umfrage nur schedulen wenn sie bis zur nächsten Mitternacht (+1h Toleranz) startet
                if (timeout < Literal.NextMidnight.Add(TimeSpan.FromHours(1)))
                {
                    logger.Debug($"Scheduling survey {req.Survey.SvyId}");
                    ScheduleSurvey(req.Survey.SvyId, timeout, CustCode);
                }

                logger.Debug("Creating new survey finished successfully");
                return Index();
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[POST]New: Unexpected error");
                return View("Error", model);
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpGet]
        public ActionResult LoadTemplate(int svyId)
        {
            try
            {
                logger.Debug($"Requested loading survey template (CustCode: {CustCode}, SvyId: {svyId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Loading template failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    db.Configuration.LazyLoadingEnabled = false;
                    var survey = db.Surveys
                        .Include("Departments")
                        .Include("AnswerOptions")
                        .Where(s => s.SvyId == svyId && s.CustCode == CustCode && s.Template)
                        .FirstOrDefault();

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
        public ActionResult CancelSurvey(int svyId)
        {
            try
            {
                logger.Debug($"Requested to cancel survey {svyId}");
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Cancelling survey failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var survey = db.Surveys.Where(s => s.SvyId == svyId && s.CustCode == CustCode).FirstOrDefault();
                    if (survey == null)
                    {
                        logger.Debug($"Cancelling survey failed. Survey not found: {svyId}");
                        return Http.NotFound("Survey not found.");
                    }

                    if (survey.Sent)
                    {
                        logger.Debug("Survey already sent. Sending cancellation request");
                        survey.Departments.Where(c => c.CustCode == CustCode).ToList().ForEach(dep =>
                        {
                            dep.People.ToList().ForEach(p =>
                            {
                                using (var client = new HttpClient())
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
                                        var response = client.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", obj).Result;

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
                            });
                        });
                    }
                    else
                    {
                        logger.Debug("Survey not sent yet. Removing from queue");
                        lock (queuedSurveys)
                        {
                            queuedSurveys.Remove(svyId);
                        }
                    }
                    db.sp_DeleteSurvey(svyId);
                    db.SaveChanges();

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
        public double LoadPricePerClick(int amount)
        {
            try
            {
                logger.Trace($"Requested to load price per click for amount: {amount}");
                using (var db = new SimpleQDBEntities())
                {
                    var price = Convert.ToDouble(db.fn_CalcPricePerClick(amount, CustCode ?? ""));

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
        internal static void ScheduleSurvey(int svyId, TimeSpan timeout, string custCode)
        {
            try
            {
                logger.Debug($"Survey scheduling started (SvyId: {svyId}, CustCode: {custCode})");
                lock (queuedSurveys)
                {
                    if (!queuedSurveys.Add(svyId))
                    {
                        logger.Debug($"Survey {svyId} already scheduled.");
                        return;
                    }
                }

                HostingEnvironment.QueueBackgroundWorkItem(ct =>
                {
                    logger.Debug($"Survey {svyId} scheduled successfully. Sleeping for {timeout.TotalMilliseconds}ms");
                    if (timeout.TotalMilliseconds > 0)
                        Thread.Sleep((int)timeout.TotalMilliseconds);

                    lock (queuedSurveys)
                    {
                        if (!queuedSurveys.Contains(svyId))
                        {
                            logger.Debug($"Survey {svyId} removed during sleep phase. Exiting");
                            return;
                        }
                    }

                    using (var db = new SimpleQDBEntities())
                    {
                        Random rnd = new Random();

                    // Anzahl an zu befragenden Personen
                    int amount = db.Surveys.Where(s => s.SvyId == svyId).FirstOrDefault().Amount;

                    // Bereits befragte Personen (zwecks Verhinderung v. Mehrfachbefragungen)
                    HashSet<int> alreadyAsked = new HashSet<int>();

                    // DepIDs mit den errechneten Anzahlen v. zu befragenden Personen
                    Dictionary<int, int> depAmounts = new Dictionary<int, int>();

                    // Gesamtanzahl an Personen von allen ausgewählten Abteilungen ermitteln
                    int totalPeople = db.Surveys.Where(s => s.SvyId == svyId).FirstOrDefault().Departments.SelectMany(d => d.People).Distinct().Count();
                        logger.Debug($"Survey {svyId} - totalPeople: {totalPeople}");

                        db.Surveys.Where(s => s.SvyId == svyId).FirstOrDefault().Departments.ToList().ForEach(dep =>
                        {
                        // Anzahl an Personen in der aktuellen Abteilung (mit DepId = id)
                        int currPeople = dep.People.Distinct().Count();
                        // Abteilung überspringen, wenn keine Leute darin
                        if (currPeople == 0)
                                return;

                        // GEWICHTETE Anzahl an zu befragenden Personen in der aktuellen Abteilung
                        int toAsk = (int)Math.Round(amount * (currPeople / (double)totalPeople));

                            depAmounts.Add(dep.DepId, toAsk);
                        });

                        logger.Debug($"Survey {svyId} - DepAmounts before correction: {string.Join(";", depAmounts.Select(x => x.Key + "=" + x.Value))}");


                    // Solange Gesamtanzahl der zu Befragenden zu klein, die Anzahl einer zufälligen Abteilung erhöhen
                    while (depAmounts.Values.Sum() < amount)
                            depAmounts[depAmounts.ElementAt(rnd.Next(0, depAmounts.Count)).Key]++;

                    // Solange Gesamtanzahl der zu Befragenden zu groß, die Anzahl einer zufälligen Abteilung verringern
                    while (depAmounts.Values.Sum() > amount)
                            depAmounts[depAmounts.ElementAt(rnd.Next(0, depAmounts.Count)).Key]--;

                        logger.Debug($"Survey {svyId} - DepAmounts after correction: {string.Join(";", depAmounts.Select(x => x.Key + "=" + x.Value))}");

                        using (var client = new HttpClient())
                        {
                            foreach (var kv in depAmounts)
                            {
                                int i = 0;
                                db.Departments
                                    .Where(d => d.DepId == kv.Key && d.CustCode == custCode)
                                    .SelectMany(d => d.People)
                                    .ToList()
                                    .Where(p => !alreadyAsked.Contains(p.PersId))
                                    .OrderBy(p => rnd.Next())
                                    .Take(kv.Value)
                                    .ToList()
                                    .ForEach(p =>
                                    {
                                        alreadyAsked.Add(p.PersId);

                                        try
                                        {
                                        // SEND SURVEY
                                        logger.Debug($"Beginning to send survey to app (SvyId: {svyId}, CustCode: {custCode}, PersId: {p.PersId}, DeviceId: {p.DeviceId})");
                                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "ZDNmNGZjODMtNTEzNC00YjA1LTkyZmUtNDRkMWJkZjRhZjVj");
                                            var obj = new
                                            {
                                                app_id = "68b8996a-f664-4130-9854-9ed7f70d5540",
                                                include_player_ids = new string[] { p.DeviceId },
                                                contents = new { en = "New survey" },
                                                content_available = true,
                                                data = new { Cancel = false, SvyId = svyId }
                                            };
                                            var response = client.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", obj).Result;

                                            if (!response.IsSuccessStatusCode)
                                            {
                                                logger.Error($"Failed sending survey (StatusCode: {response.StatusCode}, Content: {response.Content})");
                                            }
                                            else
                                            {
                                                logger.Debug($"Survey sent successfully (StatusCode: {response.StatusCode}, Content: {response.Content})");
                                                i++;
                                            }
                                        }
                                        catch (AggregateException ex)
                                        {
                                            logger.Error(ex, $"Error while sending survey to app (SvyId: {svyId}, CustCode: {custCode}, PersId: {p.PersId}, DeviceId: {p.DeviceId})");
                                        }
                                    });
                                logger.Debug($"(SvyId {svyId}) Surveys sent to Department {kv.Key}: {i} == {kv.Value}");
                            }
                        }
                        logger.Debug($"(SvyId {svyId}) Total surveys sent: {alreadyAsked.Count}");

                        db.Surveys.Where(s => s.SvyId == svyId).First().Sent = true;
                        db.SaveChanges();
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "ScheduleSurvey: Unexpected error");
                throw ex;
            }
        }

        private void AddModelError(string key, string errorMessage, ref bool error)
        {
            try
            {
                logger.Debug($"Model error: {key}: {errorMessage}");
                ModelState.AddModelError(key, errorMessage);
                error = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "AddModelError: Unexpected error");
                throw ex;
            }
        }

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

        private ArgumentNullException ANEx(string paramName) => new ArgumentNullException(paramName);
        #endregion
    }
}