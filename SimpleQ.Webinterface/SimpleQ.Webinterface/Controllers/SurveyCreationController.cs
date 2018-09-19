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
                db.Surveys.Add(req.Survey);
                db.SaveChanges();

                req.SelectedDepartments.ForEach(depId =>
                {
                    db.Askings.Add(new Asking { SvyId = req.Survey.SvyId, DepId = depId, CustCode = CustCode, Amount = req.Amount });
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
                    int totalPersons = db.Departments
                        .Where(d => req.SelectedDepartments.Contains(d.DepId) && d.CustCode == CustCode)
                        .Sum(d => d.People.Count);

                    req.SelectedDepartments.ForEach(d =>
                    {
                        // Anzahl an Personen in der aktuellen Abteilung (mit DepId = d)
                        int currPersons = db.Departments
                            .Where(dep => dep.DepId == d && dep.CustCode == CustCode)
                            .Select(dep => dep.People.Count).First();

                        // GEWICHTETE Anzahl an zu befragenden Personen in der aktuellen Abteilung
                        int toAsk = req.Amount * (int)Math.Round(currPersons / (double)totalPersons);

                        MobileController.SendSurveyNotification(d, toAsk, req.Survey.SvyId);
                    });
                }
            });

            return RedirectToAction("Index", "Home");
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