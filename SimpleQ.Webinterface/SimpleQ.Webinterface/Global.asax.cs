using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
                    using(var db = new SimpleQDBEntities())
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
                // Sleep bis um 03:00 AM
                Thread.Sleep((int)Literal.NextMidnight.Add(TimeSpan.FromHours(3)).TotalMilliseconds);
                using (var db = new SimpleQDBEntities())
                {
                    db.sp_CheckExceededSurveyData();
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
                // Sleep bis um 03:00 AM
               //Thread.Sleep((int)Literal.NextMidnight.Add(TimeSpan.FromHours(3)).TotalMilliseconds);
                using (var db = new SimpleQDBEntities())
                {
                    var result = db.sp_CreateBills().ToList();
                    foreach (int? billId in result)
                    {
                        try
                        {
                            Bill clinton = db.Bills.Where(b => b.BillId == billId).First();
                            MailMessage msg = new MailMessage("payment@simpleq.at", clinton.Customer.CustEmail)
                            {
                                Subject = "SIMPLEQ BILL",
                                Body = $"Bill: {clinton.BillPrice}"
                            };
                            SmtpClient client = new SmtpClient("smtp.1und1.de", 587)
                            {
                                Credentials = new System.Net.NetworkCredential("payment@simpleq.at", "123SimpleQ..."),
                                EnableSsl = true
                            };
                            client.Send(msg);
                            clinton.Sent = true;
                            db.SaveChanges();
                        }
                        catch (Exception ex) when (ex is SmtpException || ex is SmtpFailedRecipientsException)
                        {
                            // Logging
                            continue;
                        }
                    }
                }
            });
        }
    }
}
