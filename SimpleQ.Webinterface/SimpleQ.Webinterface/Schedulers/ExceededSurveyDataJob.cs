using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using SimpleQ.Webinterface.Models;
using NLog;

namespace SimpleQ.Webinterface.Schedulers
{
    public class ExceededSurveyDataJob : IJob
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Debug($"Fired at {context.FireTimeUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                using (var db = new SimpleQDBEntities())
                {
                    var result = await Task.Run(() => db.sp_CheckExceededSurveyData());
                    logger.Debug($"{result.FirstOrDefault()} surveys deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Check for exceeded survey data failed unexpectedly.");
            }
            finally
            {
                logger.Debug($"Next fire time {context.NextFireTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
            }
        }
    }
}