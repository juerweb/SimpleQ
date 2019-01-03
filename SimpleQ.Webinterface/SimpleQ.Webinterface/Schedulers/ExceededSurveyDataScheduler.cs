using NLog;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace SimpleQ.Webinterface.Schedulers
{
    public class ExceededSurveyDataScheduler : Scheduler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        protected override string Name => "exceeded survey data scheduler";

        protected override void Schedule()
        {
            try
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
            catch (Exception ex)
            {
                logger.Error(ex, "Schedule: Check for exceeded survey data failed unexpectedly.");
            }
        }
    }
}