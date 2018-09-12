using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SimpleQ.Webinterface.Mobile;
using SimpleQ.Webinterface.Models;
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
                //s.Survey.EndDate = s.Survey.StartDate.AddDays(7);
                db.Surveys.Add(req.Survey);
                req.SelectedDepartments.ForEach(d =>
                {
                    db.Askings.Add(new Asking { SvyId = req.Survey.SvyId, DepId = d, CustCode = CustCode });
                });
                db.SaveChanges();
            }

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                Thread.Sleep(req.Survey.StartDate - DateTime.Now);

                using (var db = new SimpleQDBEntities())
                {
                    // Gesamtanzahl an Personen von allen ausgewählten Abteilungen ermitteln
                    int totalPersons = db.Departments
                        .Where(d => req.SelectedDepartments.Contains(d.DepId) && d.CustCode == CustCode)
                        .Sum(d => d.AskedPersons.Count);

                    req.SelectedDepartments.ForEach(d =>
                    {
                        // Anzahl an Personen in der aktuellen Abteilung (mit DepId = d)
                        int currPersons = db.Departments
                            .Where(dep => dep.DepId == d && dep.CustCode == CustCode)
                            .Select(dep => dep.AskedPersons.Count).First();

                        // GEWICHTETE Anzahl an zu befragenden Personen in der aktuellen Abteilung
                        int toAsk = req.Amount * (int)Math.Round(currPersons / (double)totalPersons);

                        MobileController.SendSurvey(d, toAsk, CustCode, req.Survey);
                    });
                }
            });

            return RedirectToAction("Index", "Home");
        }


        private string CustCode
        {
            get
            {
                return Session["custCode"] as string;
            }
        }
    }
}