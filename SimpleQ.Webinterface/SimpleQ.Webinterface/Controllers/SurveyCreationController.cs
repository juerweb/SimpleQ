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
using OneSignal.CSharp.SDK;
using OneSignal.CSharp.SDK.Resources.Devices;
using OneSignal.CSharp.SDK.Resources.Notifications;

namespace SimpleQ.Webinterface.Controllers
{
    public class SurveyCreationController : Controller
    {
        private static HashSet<int> queuedSurveys = new HashSet<int>();

        [HttpPost]
        public ActionResult New(SurveyCreationModel req)
        {
            using (var db = new SimpleQDBEntities())
            {
                req.Survey.CustCode = CustCode;
                req.Survey.StartDate = req.StartDate.Date.Add(req.StartTime);
                req.Survey.EndDate = req.EndDate.Date.Add(req.EndTime);

                int totalPeople = db.Departments.Where(d => req.SelectedDepartments.Contains(d.DepId) && d.CustCode == CustCode)
                    .SelectMany(d => d.People).Distinct().Count();
                if (req.Survey.Amount > totalPeople)
                    req.Survey.Amount = totalPeople;

                db.Surveys.Add(req.Survey);
                db.SaveChanges();

                req.SelectedDepartments.ForEach(depId =>
                {
                    db.Surveys.Where(s => s.SvyId == req.Survey.SvyId).First().Departments.Add(db.Departments.Where(d => d.DepId == depId).First());
                });
                db.SaveChanges();

                req.TextAnswerOptions?.ForEach(text =>
                {
                    db.AnswerOptions.Add(new AnswerOption { SvyId = req.Survey.SvyId, AnsText = text });
                });
                db.SaveChanges();
            }

            TimeSpan timeout = req.Survey.StartDate - DateTime.Now;

            // Umfrage nur schedulen wenn sie bis zur nächsten Mitternacht (+1h Toleranz) startet
            if (timeout < Extensions.NextMidnight.Add(TimeSpan.FromHours(1)))
                ScheduleSurvey(req.Survey.SvyId, timeout, CustCode);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public JsonResult LoadTemplate(int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                return Json(db.Surveys.Where(s => s.SvyId == svyId && s.CustCode == CustCode && s.Template).FirstOrDefault());
            }
        }

        [HttpGet]
        public void CreateCategory(string catName)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.SurveyCategories.Add(new SurveyCategory {
                    CatId = db.SurveyCategories.Where(c => c.CustCode == CustCode).Max(c => c.CatId) + 1,
                    CatName = catName, CustCode = CustCode });
                db.SaveChanges();
            }
        }

        [HttpGet]
        public void CancelSurvey(int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                var survey = db.Surveys.Where(s => s.SvyId == svyId).FirstOrDefault();
                if (survey.Sent)
                {
                    survey.Departments.ToList().ForEach(dep =>
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
                    queuedSurveys.Remove(svyId);
                }
                db.Surveys.Remove(survey);
                db.SaveChanges();
            }
        }


        internal static void ScheduleSurvey(int svyId, TimeSpan timeout, string custCode)
        {
            if (!queuedSurveys.Add(svyId)) return;

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                if (timeout.TotalMilliseconds > 0)
                    Thread.Sleep((int)timeout.TotalMilliseconds);

                if (!queuedSurveys.Contains(svyId)) return;

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


        private string CustCode
        {
            get
            {
                return "m4rku5";//Session["custCode"] as string;
            }
        }
    }
}