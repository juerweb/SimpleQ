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

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class SurveyCreationController : Controller
    {
        private static readonly HashSet<int> queuedSurveys = new HashSet<int>();

        #region MVC-Actions
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });

                var model = new SurveyCreationModel
                {
                    SurveyCategories = cust.SurveyCategories.Where(s => !s.Deactivated).ToList(),
                    AnswerTypes = cust.AnswerTypes.ToList(),
                    Departments = cust.Departments.Select(d => new { Department = d, Amount = d.People.Count }).ToDictionary(x => x.Department, x => x.Amount),
                    SurveyTemplates = db.Surveys.Where(s => s.CustCode == CustCode && s.Template).ToList()
                };

                ViewBag.emailConfirmed = cust.EmailConfirmed;
                return View("SurveyCreation", model);
            }
        }

        [HttpPost]
        public ActionResult New(SurveyCreationModel req)
        {
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
                    return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });

                if(!db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().EmailConfirmed)
                    return View("Error", new ErrorModel { Title = "Confirm your e-mail", Message = "Please confirm your e-mail address before creating surveys." });

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

                foreach (var ans in req.TextAnswerOptions)
                {
                    if (string.IsNullOrEmpty(ans))
                    {
                        AddModelError("TextAnswerOptions", "AnswerOptions must not be empty.", ref err);
                        break;
                    }
                }

                if (err) return Index();


                int totalPeople = db.Departments.Where(d => req.SelectedDepartments.Contains(d.DepId) && d.CustCode == CustCode)
                    .SelectMany(d => d.People).Distinct().Count();
                if (req.Survey.Amount > totalPeople)
                    req.Survey.Amount = totalPeople;

                db.Surveys.Add(req.Survey);
                db.SaveChanges();

                req.SelectedDepartments.ForEach(depId =>
                {
                    req.Survey.Departments.Add(db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault());
                });
                db.SaveChanges();

                if (baseId == (int)BaseQuestionTypes.LikertScaleQuestion)
                {
                    db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = req.TextAnswerOptions[0], FirstPosition = true });
                    db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = req.TextAnswerOptions[1], FirstPosition = false });
                }
                else if (baseId != (int)BaseQuestionTypes.FixedAnswerQuestion && baseId != (int)BaseQuestionTypes.OpenQuestion)
                {
                    req.TextAnswerOptions.ForEach(text =>
                    {
                        db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = text });
                    });
                }
                db.SaveChanges();
            }

            TimeSpan timeout = req.Survey.StartDate - DateTime.Now;

            // Umfrage nur schedulen wenn sie bis zur nächsten Mitternacht (+1h Toleranz) startet
            if (timeout < Literal.NextMidnight.Add(TimeSpan.FromHours(1)))
                ScheduleSurvey(req.Survey.SvyId, timeout, CustCode);

            return Index();
        }
        #endregion

        #region AJAX-Methods
        [HttpGet]
        public ActionResult LoadTemplate(int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                var survey = db.Surveys
                    .Include("Departments")
                    .Include("AnswerOptions")
                    .Where(s => s.SvyId == svyId && s.CustCode == CustCode && s.Template)
                    .FirstOrDefault();

                if (survey == null)
                    return Http.NotFound("Template not found.");

                return Json(survey, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CreateCategory(string catName)
        {
            return RedirectToAction("AddCategory", "Settings", new { catName });
        }

        [HttpGet]
        public ActionResult CancelSurvey(int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                var survey = db.Surveys.Where(s => s.SvyId == svyId && s.CustCode == CustCode).FirstOrDefault();
                if (survey == null)
                    return Http.NotFound("Survey not found.");

                if (survey.Sent)
                {
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
                                    var task = client.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", obj);
                                    task.Wait();
                                    var response = task.Result;
                                    System.Diagnostics.Debug.Write("RESPONSE:");
                                    System.Diagnostics.Debug.WriteLine(response.StatusCode);
                                    System.Diagnostics.Debug.WriteLine(response.Content);
                                }
                                catch (AggregateException ex)
                                {
                                    //logging
                                    throw ex; // just for testing...
                                }
                            }
                        });
                    });
                }
                else
                {
                    lock (queuedSurveys)
                    {
                        queuedSurveys.Remove(svyId);
                    }
                }
                db.sp_DeleteSurvey(svyId);
                db.SaveChanges();

                return Http.Ok();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public double LoadPricePerClick(int amount)
        {
            using (var db = new SimpleQDBEntities())
            {
                return Convert.ToDouble(db.fn_CalcPricePerClick(amount));
            }
        }
        #endregion

        #region Helpers
        internal static void ScheduleSurvey(int svyId, TimeSpan timeout, string custCode)
        {
            lock (queuedSurveys)
            {
                if (!queuedSurveys.Add(svyId)) return;
            }

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                if (timeout.TotalMilliseconds > 0)
                    Thread.Sleep((int)timeout.TotalMilliseconds);

                lock (queuedSurveys)
                {
                    if (!queuedSurveys.Contains(svyId)) return;
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


                    // Solange Gesamtanzahl der zu Befragenden zu klein, die Anzahl einer zufälligen Abteilung erhöhen
                    while (depAmounts.Values.Sum() < amount)
                        depAmounts[depAmounts.ElementAt(rnd.Next(0, depAmounts.Count)).Key]++;

                    // Solange Gesamtanzahl der zu Befragenden zu groß, die Anzahl einer zufälligen Abteilung verringern
                    while (depAmounts.Values.Sum() > amount)
                        depAmounts[depAmounts.ElementAt(rnd.Next(0, depAmounts.Count)).Key]--;

                    
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
                                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "ZDNmNGZjODMtNTEzNC00YjA1LTkyZmUtNDRkMWJkZjRhZjVj");
                                        var obj = new
                                        {
                                            app_id = "68b8996a-f664-4130-9854-9ed7f70d5540",
                                            include_player_ids = new string[] { p.DeviceId },
                                            contents = new { en = "New survey" },
                                            content_available = true,
                                            data = new { Cancel = false, SvyId = svyId }
                                        };
                                        var task = client.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", obj);
                                        task.Wait();
                                        var response = task.Result;
                                        System.Diagnostics.Debug.Write("RESPONSE:");
                                        System.Diagnostics.Debug.WriteLine(response.StatusCode);
                                        System.Diagnostics.Debug.WriteLine(response.Content);
                                    }
                                    catch (AggregateException ex)
                                    {
                                        //logging
                                        throw ex; // just for testing...
                                    }
                                    i++;
                                });
                            System.Diagnostics.Debug.WriteLine($"(SvyId {svyId}) SURVEYS SENT: {i} == {kv.Value}");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"(SvyId {svyId}) TOTAL SENT: {alreadyAsked.Count}");

                    db.Surveys.Where(s => s.SvyId == svyId).First().Sent = true;
                    db.SaveChanges();
                }
            });
        }

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