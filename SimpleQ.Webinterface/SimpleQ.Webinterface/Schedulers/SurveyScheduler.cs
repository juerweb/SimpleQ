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
    public class SurveyScheduler : Scheduler
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        protected override string Name => "survey scheduler";

        protected override void Schedule()
        {
            try
            {
                // Sleep bis um 00:10 (damit DateTime.Today sicher korrekt)
                logger.Debug($"Survey scheduler sleeping for {Literal.NextMidnight.ToString(@"hh\:mm\:ss\.fff")}");
                Thread.Sleep((int)Literal.NextMidnight.Add(TimeSpan.FromMinutes(10)).TotalMilliseconds);
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
            catch (Exception ex)
            {
                logger.Error(ex, "Schedule: Scheduling surveys failed unexpectedly");
            }
        }
    }
}