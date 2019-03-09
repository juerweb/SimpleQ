using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using SimpleQ.Webinterface.Models;
using NLog;
using System.Data.Entity.Infrastructure;

namespace SimpleQ.Webinterface.Schedulers
{
    public class BackupJob : IJob
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Debug($"Fired at {context.FireTimeUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                using (var strm = File.Create($"{DateTime.Today.ToString("yyyy-MM-dd")}"))
                using (var writer = new StreamWriter(strm))
                using (var db = new SimpleQDBEntities())
                {
                    //var tables = new string[] { "DataConstraint", "FaqEntry", "PaymentMethod", "BaseQuestionType", "AnswerType", "PredefinedAnswerOption",
                    //    "Customer", "Bill", "Department", "Person", "Employs", "SurveyCategory", "Survey", "Asking", "AnswerOption", "Vote", "Chooses" };
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