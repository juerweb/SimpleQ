using MigraDoc.Rendering;
using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static bool surveySchedulerStarted = false;
        private static readonly object lck1 = new object();

        private static bool exceededSurveySchedulerStarted = false;
        private static readonly object lck2 = new object();

        private static bool createBillsSchedulerStarted = false;
        private static readonly object lck3 = new object();

        protected void Application_Start()
        {
            try
            {
                logger.Debug("Starting application");
                GlobalFilters.Filters.Add(new RequireHttpsAttribute());

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
                AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

                logger.Debug("Starting schedulers");
                StartSurveyScheduler();
                RestoreQueuedSurveys();
                StartExceededSurveyScheduler();
                StartCreateBillsScheduler();
                logger.Debug("Application started successfully");
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Unexpected error while starting application");
                throw ex;
            }
        }

        private void StartSurveyScheduler()
        {
            try
            {
                logger.Debug("Starting survey scheduler");
                lock (lck1)
                {
                    if (surveySchedulerStarted)
                    {
                        logger.Debug("Survey scheduler running already");
                        return;
                    }

                    surveySchedulerStarted = true;
                }

                HostingEnvironment.QueueBackgroundWorkItem(ct =>
                {
                    try
                    {
                        while (true)
                        {
                            // Sleep bis zur nächsten Mitternacht
                            logger.Debug($"Survey scheduler sleeping for {Literal.NextMidnight.ToString(@"hh\:mm\:ss\.fff")}");
                            Thread.Sleep((int)Literal.NextMidnight.TotalMilliseconds);
                            using (var db = new SimpleQDBEntities())
                            {
                                // Alle heutigen Umfragen schedulen
                                var query = db.Surveys.Where(s => s.StartDate.Date == DateTime.Today).ToList();
                                query.ForEach(s =>
                                {
                                    Controllers.SurveyCreationController.ScheduleSurvey(s.SvyId, s.StartDate - DateTime.Now, s.CustCode);
                                });
                                logger.Debug($"{query.Count} surveys scheduled for today successfully");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "StartSurveyScheduler->QueueBackgroundWorkItem: Scheduling surveys failed unexpectedly");
                        throw ex;
                    }
                });

                logger.Debug("Survey scheduler started successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "StartSurveyScheduler: Unexpected error");
                throw ex;
            }
        }

        private void RestoreQueuedSurveys()
        {
            try
            {
                logger.Debug("Starting to restore queued surveys");
                using (var db = new SimpleQDBEntities())
                {
                    // Alle Umfragen welche bis zur nächsten Mitternacht (+ 1h Toleranz) starten schedulen
                    var query = db.Surveys.Where(s => !s.Sent).ToList()
                        .Where(s => s.StartDate - DateTime.Now < Literal.NextMidnight.Add(TimeSpan.FromHours(1)))
                        .ToList();
                    query.ForEach(s =>
                    {
                        Controllers.SurveyCreationController.ScheduleSurvey(s.SvyId, s.StartDate - DateTime.Now, s.CustCode);
                    });
                    logger.Debug($"{query.Count} surveys restored successfully");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "RestoreQueuedSurveys: Unexpected error");
                throw ex;
            }
        }

        private void StartExceededSurveyScheduler()
        {
            try
            {
                logger.Debug("Starting exceeded survey data scheduler");
                lock (lck2)
                {
                    if (exceededSurveySchedulerStarted)
                    {
                        logger.Debug("Exceeded survey data scheduler running already");
                        return;
                    }

                    exceededSurveySchedulerStarted = true;
                }
                HostingEnvironment.QueueBackgroundWorkItem(ct =>
                {
                    try
                    {
                        while (true)
                        {
                            // Sleep bis um 03:00 AM
                            logger.Debug($"Exceeded survey data scheduler sleeping for {Literal.NextMidnight.Add(TimeSpan.FromHours(3)).ToString(@"hh\:mm\:ss\.fff")}");
                            Thread.Sleep((int)Literal.NextMidnight.Add(TimeSpan.FromHours(3)).TotalMilliseconds);
                            using (var db = new SimpleQDBEntities())
                            {
                                var result = db.sp_CheckExceededSurveyData();
                                logger.Debug($"{result.FirstOrDefault()} surveys deleted successfully.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "StartExceededSurveyScheduler->QueueBackgroundWorkItem: Check for exceeded survey data failed unexpectedly");
                        throw ex;
                    }
                });

                logger.Debug("Exceeded survey data scheduler started successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "StartExceededSurveyScheduler: Unexpected error");
                throw ex;
            }
        }

        private void StartCreateBillsScheduler()
        {
            try
            {
                logger.Debug("Starting create bills scheduler");
                lock (lck3)
                {
                    if (createBillsSchedulerStarted)
                    {
                        logger.Debug("Create bills scheduler running already");
                        return;
                    }

                    createBillsSchedulerStarted = true;
                }

                HostingEnvironment.QueueBackgroundWorkItem(ct =>
                {
                    try
                    {
                        while (true)
                        {
                            // Sleep bis um 03:00 AM
                            logger.Debug($"Create bills scheduler sleeping for {Literal.NextMidnight.Add(TimeSpan.FromHours(3)).ToString(@"hh\:mm\:ss\.fff")}");
                            Thread.Sleep((int)Literal.NextMidnight.Add(TimeSpan.FromHours(3)).TotalMilliseconds);
                            using (var db = new SimpleQDBEntities())
                            {
                                var result = db.sp_CreateBills();
                                logger.Debug($"{result.Count()} bills created");
                                var bills = db.Bills.Where(b => !b.Paid).ToList();
                                logger.Debug($"{bills.Count} bills loaded for sending");

                                foreach (Bill clinton in bills)
                                {
                                    var lastBillDate = db.Bills.Where(b => b.CustCode == clinton.CustCode)
                                        .OrderByDescending(b => b.BillDate)
                                        .Select(b => b.BillDate)
                                        .Skip(1)
                                        .FirstOrDefault();
                                    logger.Debug($"Last bill date of customer {clinton.CustCode}: {lastBillDate.ToString("yyyy-MM-dd")}");

                                    var surveys = db.Surveys
                                        .Where(s => s.CustCode == clinton.CustCode
                                                && s.StartDate <= clinton.BillDate
                                                && s.EndDate > lastBillDate)
                                        .OrderBy(s => s.StartDate)
                                        .ToArray();
                                    logger.Debug($"Surveys to bill for customer {clinton.CustCode}: {surveys.Length}");

                                    var stream = new MemoryStream();
                                    if (Pdf.CreateBill(ref stream, clinton, surveys, HostingEnvironment.MapPath("~/Content/Images/Logos/simpleQ.png"), lastBillDate))
                                    {
                                        logger.Debug($"Pdf document for bill {clinton.BillId} created successfully");
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
                                            logger.Debug($"Bill {clinton.BillId} sent successfully");
                                        }
                                        else
                                        {
                                            logger.Error($"Failed to send bill {clinton.BillId}");
                                        }
                                    }
                                    else
                                    {
                                        logger.Error($"Failed to create pdf document for bill {clinton.BillId}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "StartCreateBillsScheduler->QueueBackgroundWorkItem: Creating/sending bill failed");
                        throw ex;
                    }
                });
                logger.Debug("Create bills scheduler started successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "StartCreateBillsScheduler: Unexpected error");
                throw ex;
            }
        }
    }
}
