using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.Mobile;
using SimpleQ.Webinterface.Models.ViewModels;
//using OneSignal.CSharp.SDK;
//using OneSignal.CSharp.SDK.Resources.Devices;
//using OneSignal.CSharp.SDK.Resources.Notifications;
using SimpleQ.Webinterface.Extensions;

namespace SimpleQ.Webinterface.Controllers
{
    public class SurveyCreationController : Controller
    {
        private const int YES_NO = 1;
        private const int YES_NO_DONTKNOW = 2;
        private const int TRAFFIC_LIGHT = 3;
        private const int OPEN = 4;

        private const int LIKERT_SCALE_3 = 10;
        private const int LIKERT_SCALE_4 = 11;
        private const int LIKERT_SCALE_5 = 12;
        private const int LIKERT_SCALE_6 = 13;
        private const int LIKERT_SCALE_7 = 14;
        private const int LIKERT_SCALE_8 = 15;
        private const int LIKERT_SCALE_9 = 16;

        private static readonly int[] predefined = { YES_NO, YES_NO_DONTKNOW, TRAFFIC_LIGHT, OPEN };

        // <TypeId, intermediate values>
        private static readonly Dictionary<int, int> likertScales = new Dictionary<int, int>
        {
            { LIKERT_SCALE_3,  1},
            { LIKERT_SCALE_4,  2},
            { LIKERT_SCALE_5,  3},
            { LIKERT_SCALE_6,  4},
            { LIKERT_SCALE_7,  5},
            { LIKERT_SCALE_8,  6},
            { LIKERT_SCALE_9,  7},
        };

        private static readonly HashSet<int> queuedSurveys = new HashSet<int>();

        [HttpGet]
        public ActionResult Index()
        {
            Session["custCode"] = "420420";

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return Http.NotFound("Customer not found.");

                var model = new SurveyCreationModel
                {
                    SurveyCategories = cust.SurveyCategories.Where(s => !s.Deactivated).ToList(),
                    AnswerTypes = cust.AnswerTypes.ToList(),
                    Departments = cust.Departments.Select(d => new { Department = d, Amount = d.People.Count }).ToDictionary(x => x.Department, x => x.Amount),
                    SurveyTemplates = db.Surveys.Where(s => s.CustCode == CustCode && s.Template).ToList()
                };

                return View(viewName: "SurveyCreation", model: model);
            }
        }

        [HttpPost]
        public ActionResult New(SurveyCreationModel req)
        {
            if (req == null)
                return Http.BadRequest("Model object must not be null.");
            if (req.Survey == null)
                return Http.BadRequest("Survey must not be null.");
            if (req.Survey.SvyText == null)
                return Http.BadRequest("SvyText must not be null.");
            if (req.SelectedDepartments == null || req.SelectedDepartments.Count() == 0)
                return Http.BadRequest("SelectedDepartments must not be null or empty.");

            using (var db = new SimpleQDBEntities())
            {
                if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    return Http.NotFound("Customer not found.");

                req.Survey.CustCode = CustCode;
                req.Survey.StartDate = req.StartDate.Date.Add(req.StartTime);
                req.Survey.EndDate = req.EndDate.Date.Add(req.EndTime);
                req.Survey.Sent = false;

                if (req.Survey.StartDate >= req.Survey.EndDate)
                    return Http.Conflict("StartDate must be earlier than EndDate.");

                if (req.Survey.Amount <= 0)
                    return Http.Conflict("Amount must be at least 1.");

                if (db.SurveyCategories.Where(c => c.CatId == req.Survey.CatId && c.CustCode == CustCode).FirstOrDefault() == null)
                    return Http.NotFound("Category not found.");

                if (db.AnswerTypes.Where(a => a.TypeId == req.Survey.TypeId).FirstOrDefault() == null)
                    return Http.Conflict("AnswerType does not exist.");

                foreach (var depId in req.SelectedDepartments)
                {
                    if (db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault() == null)
                        return Http.NotFound("Department not found.");
                }


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

                if (likertScales.ContainsKey(req.Survey.TypeId))
                {
                    if (req.TextAnswerOptions == null || req.TextAnswerOptions.Count() != 2)
                        return Http.Conflict("Likert scales must have exactly two answer options.");

                    db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = req.TextAnswerOptions[0] });

                    for (int i = 0; i < likertScales[req.Survey.TypeId]; i++)
                        db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = $"{i + 2}" });

                    db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = req.TextAnswerOptions[1] });
                }
                else if (!predefined.Contains(req.Survey.TypeId))
                {
                    req.TextAnswerOptions?.ForEach(text =>
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
                            // CANCEL SURVEY
                            //var client = new OneSignalClient("ZDNmNGZjODMtNTEzNC00YjA1LTkyZmUtNDRkMWJkZjRhZjVj");
                            //var options = new NotificationCreateOptions
                            //{
                            //    AppId = new Guid("68b8996a-f664-4130-9854-9ed7f70d5540"),
                            //    IncludePlayerIds = {p.DeviceId}
                            //};
                            //options.Contents.Add("SvyId", $"{svyId}");
                            //client.Notifications.Create(options);
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

                                // SEND SURVEY
                                //var client = new OneSignalClient("ZDNmNGZjODMtNTEzNC00YjA1LTkyZmUtNDRkMWJkZjRhZjVj");
                                //var options = new NotificationCreateOptions
                                //{
                                //    AppId = new Guid("68b8996a-f664-4130-9854-9ed7f70d5540"),
                                //    IncludePlayerIds = {p.DeviceId}
                                //};
                                //options.Contents.Add("SvyId", $"{svyId}");
                                //client.Notifications.Create(options);

                                i++;
                            });
                        System.Diagnostics.Debug.WriteLine($"(SvyId {svyId}) SURVEYS SENT: {i} == {kv.Value}");
                    }
                    System.Diagnostics.Debug.WriteLine($"(SvyId {svyId}) TOTAL SENT: {alreadyAsked.Count} == {svyId}");

                    db.Surveys.Where(s => s.SvyId == svyId).First().Sent = true;
                    db.SaveChanges();
                }
            });
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