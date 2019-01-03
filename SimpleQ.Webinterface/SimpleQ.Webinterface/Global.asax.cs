using NLog;
using SimpleQ.Webinterface.Schedulers;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SimpleQ.Webinterface
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static Scheduler surveyScheduler = new SurveyScheduler();
        private static Scheduler exceededSurveyDataScheduler = new ExceededSurveyDataScheduler();
        private static Scheduler billScheduler = new BillScheduler();
        private static Scheduler periodicSurveyScheduler = new PeriodicSurveyScheduler();

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

                RestoreQueuedSurveys();

                logger.Debug("Starting schedulers");
                surveyScheduler.Start();
                exceededSurveyDataScheduler.Start();
                billScheduler.Start();
                periodicSurveyScheduler.Start();
                logger.Debug("Application started successfully");
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Unexpected error while starting application");
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
                    // Alle Umfragen welche vor 00:00 starten schedulen (+10min Toleranz)
                    var query = db.Surveys.Where(s => !s.Sent)
                        .ToList()
                        .Where(s => s.StartDate - DateTime.Now < Literal.NextMidnight.Add(TimeSpan.FromMinutes(10)))
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
    }
}
