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

namespace SimpleQ.Webinterface.Controllers
{
    public class SurveyCreationController : Controller
    {
        [HttpPost]
        public ActionResult New(SurveyCreationModel req)
        {
            using (var db = new SimpleQDBEntities())
            {
                req.Survey.CustCode = CustCode;
                req.Survey.StartDate = req.StartDate.Date.Add(req.StartTime);
                req.Survey.EndDate = req.EndDate.Date.Add(req.EndTime);

                int totalPeople = db.Departments.Where(d => req.SelectedDepartments.Contains(d.DepId)).SelectMany(d => d.People).Count();
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

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                TimeSpan timeout = req.Survey.StartDate - DateTime.Now;
                if (timeout.Milliseconds > 0)
                    Thread.Sleep(req.Survey.StartDate - DateTime.Now);

                using (var db = new SimpleQDBEntities())
                {
                    // Gesamtanzahl an Personen von allen ausgewählten Abteilungen ermitteln
                    int totalPeople = db.Departments
                        .Where(d => req.SelectedDepartments.Contains(d.DepId) && d.CustCode == CustCode)
                        .Sum(d => d.People.Count);

                    req.SelectedDepartments.ForEach(d =>
                    {
                        // Anzahl an Personen in der aktuellen Abteilung (mit DepId = d)
                        int currPeople = db.Departments
                            .Where(dep => dep.DepId == d && dep.CustCode == CustCode)
                            .Select(dep => dep.People.Count).First();

                        // GEWICHTETE Anzahl an zu befragenden Personen in der aktuellen Abteilung
                        int toAsk = (int)Math.Round(req.Survey.Amount * (currPeople / (double)totalPeople));

                        SendSurveyNotification(d, toAsk, req.Survey.SvyId);
                    });
                }
            });

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public Survey LoadTemplate(int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                return db.Surveys.Where(s => s.SvyId == svyId && s.CustCode == CustCode && s.Template).FirstOrDefault();
            }
        }


        private void SendSurveyNotification(int depId, int amount, int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                Random rnd = new Random();
                //int i = 0;
                db.Departments
                    .Where(d => d.DepId == depId)
                    .SelectMany(d => d.People)
                    .ToList()
                    .OrderBy(p => rnd.Next())
                    .Take(amount)
                    .ToList()
                    .ForEach(p =>
                    {
                        //i++;
                    });
                //System.Diagnostics.Debug.WriteLine($"(SvyId {svyId}) SURVEYS SENT: {i} == {amount}");
            }
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