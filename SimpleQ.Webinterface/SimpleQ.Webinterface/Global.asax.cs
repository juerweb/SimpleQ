﻿using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static JobScheduler jobScheduler = new JobScheduler();

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
                jobScheduler.Start().Wait();
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
                    // Schedule each survey starting before 00:10
                    var query = db.Surveys.Where(s => !s.Sent)
                        .ToList()
                        .Where(s => s.StartDate < Literal.NextMidnight.Add(TimeSpan.FromMinutes(10)))
                        .ToList();
                    int count = 0;
                    
                    foreach (var s in query)
                    {
                        var success = SurveyQueue.EnqueueSurvey(s.SvyId, s.StartDate, s.CustCode).Result;
                        count += success ? 1 : 0;
                    }
                    logger.Debug($"{count} surveys restored successfully");
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
