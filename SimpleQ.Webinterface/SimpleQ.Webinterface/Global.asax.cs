using MigraDoc.Rendering;
using PdfSharp.Pdf;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SimpleQ.Webinterface
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static bool surveySchedulerStarted = false;
        private static readonly object lck1 = new object();

        private static bool exceededSurveySchedulerStarted = false;
        private static readonly object lck2 = new object();

        private static bool createBillsSchedulerStarted = false;
        private static readonly object lck3 = new object();

        protected void Application_Start()
        {
            GlobalFilters.Filters.Add(new RequireHttpsAttribute());

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            StartSurveyScheduler();
            RestoreQueuedSurveys();
            StartExceededSurveyScheduler();
            StartCreateBillsScheduler();
        }

        private void StartSurveyScheduler()
        {
            lock (lck1)
            {
                if (surveySchedulerStarted) return;
                surveySchedulerStarted = true;
            }

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                while (true)
                {
                    // Sleep bis zur nächsten Mitternacht
                    Thread.Sleep((int)Literal.NextMidnight.TotalMilliseconds);
                    using (var db = new SimpleQDBEntities())
                    {
                        // Alle heutigen Umfragen schedulen
                        db.Surveys.Where(s => s.StartDate.Date == DateTime.Today).ToList().ForEach(s =>
                        {
                            Controllers.SurveyCreationController.ScheduleSurvey(s.SvyId, s.StartDate - DateTime.Now, s.CustCode);
                        });
                    }
                }
            });
        }

        private void RestoreQueuedSurveys()
        {
            using (var db = new SimpleQDBEntities())
            {
                // Alle Umfragen welche bis zur nächsten Mitternacht (+ 1h Toleranz) starten schedulen
                db.Surveys.Where(s => !s.Sent).ToList()
                    .Where(s => s.StartDate - DateTime.Now < Literal.NextMidnight.Add(TimeSpan.FromHours(1)))
                    .ToList().ForEach(s =>
                {
                    Controllers.SurveyCreationController.ScheduleSurvey(s.SvyId, s.StartDate - DateTime.Now, s.CustCode);
                });
            }
        }

        private void StartExceededSurveyScheduler()
        {
            lock (lck2)
            {
                if (exceededSurveySchedulerStarted) return;
                exceededSurveySchedulerStarted = true;
            }

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                while (true)
                {
                    // Sleep bis um 03:00 AM
                    Thread.Sleep((int)Literal.NextMidnight.Add(TimeSpan.FromHours(3)).TotalMilliseconds);
                    using (var db = new SimpleQDBEntities())
                    {
                        db.sp_CheckExceededSurveyData();
                    }
                }
            });
        }

        private void StartCreateBillsScheduler()
        {
            lock (lck3)
            {
                if (createBillsSchedulerStarted) return;
                createBillsSchedulerStarted = true;
            }

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                while (true)
                {
                    // Sleep bis um 03:00 AM
                    Thread.Sleep((int)Literal.NextMidnight.Add(TimeSpan.FromHours(3)).TotalMilliseconds);
                    using (var db = new SimpleQDBEntities())
                    {
                        db.sp_CreateBills().ToList();
                        var bills = db.Bills.Where(b => !b.Paid).ToList();

                        foreach (Bill clinton in bills)
                        {
                            var lastBillDate = db.Bills.Where(b => b.CustCode == clinton.CustCode)
                                .OrderByDescending(b => b.BillDate)
                                .Select(b => b.BillDate)
                                .Skip(1)
                                .FirstOrDefault();

                            var surveys = db.Surveys
                                .Where(s => s.CustCode == clinton.CustCode
                                        && s.StartDate <= clinton.BillDate
                                        && s.EndDate > lastBillDate)
                                .OrderBy(s => s.StartDate)
                                .ToArray();

                            var stream = new MemoryStream();
                            if (Pdf.CreateBill(ref stream, clinton, surveys, HostingEnvironment.MapPath("~/Content/Images/Logos/simpleQ.png"), lastBillDate))
                            {

                                var body = $"Dear sir or madam!{Environment.NewLine}{Environment.NewLine}" +
                                    $"Enclosed you will find your most recent bill.{Environment.NewLine}" +
                                    $"We stay at your entire disposal for further questions.{Environment.NewLine}" +
                                    $"{Environment.NewLine}{Environment.NewLine}" +
                                    $"Sincerely{Environment.NewLine}" +
                                    $"Your SimpleQ team";

                                if (Email.Send("payment@simpleq.at", clinton.Customer.CustEmail, "SimpleQ Bill", body, false,
                                    new System.Net.Mail.Attachment(stream, $"SimpleQ_Bill_{clinton.BillDate.ToString("yyyy-MM-dd")}.pdf", "application/pdf")))
                                {
                                    clinton.Sent = true;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    // Logging
                                }
                            }
                            else
                            {
                                // Logging
                            }
                        }
                    }
                }
            });
        }
    }
}
