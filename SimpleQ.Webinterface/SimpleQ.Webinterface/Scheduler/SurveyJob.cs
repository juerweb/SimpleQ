using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using NLog;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;

namespace SimpleQ.Webinterface.Schedulers
{
    public class SurveyJob : IJob
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Debug($"Fired at {context.FireTimeUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                using (var db = new SimpleQDBEntities())
                {
                    // Queue all surveys for today starting at or after 00:10
                    var query = (await db.Surveys.ToListAsync()).Where(s => s.StartDate >= DateTime.Today.AddMinutes(10)).ToList();
                    int count = 0;

                    foreach (var s in query)
                    {
                        var success = await SurveyQueue.EnqueueSurvey(s.SvyId, s.StartDate, s.CustCode);
                        count += success ? 1 : 0;
                    }
                    logger.Debug($"{count} surveys scheduled successfully");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Scheduling surveys failed unexpectedly");
            }
            finally
            {
                logger.Debug($"Next fire time {context.NextFireTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
            }
        }
    }
}