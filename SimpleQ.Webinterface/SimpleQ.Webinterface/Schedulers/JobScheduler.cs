using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Quartz;
using Quartz.Impl;
using SimpleQ.Webinterface.Extensions;
using System.Threading.Tasks;
using Quartz.Impl.Matchers;

namespace SimpleQ.Webinterface.Schedulers
{
    public class JobScheduler
    {
        private IScheduler scheduler;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string PERIODIC_NAME = "__PERIODIC_SURVEY_JOB";
        private const string SURVEY_NAME = "__SURVEY_JOB";
        private const string EXCEEDED_NAME = "__EXCEEDED_SURVEY_DATA_JOB";
        private const string BILL_NAME = "__BILL_JOB";
        private const string BACKUP_NAME = "__BACKUP_JOB";

        public async Task Start()
        {
            try
            {
                scheduler = await StdSchedulerFactory.GetDefaultScheduler();
                await scheduler.Start();

                //var startTime = Literal.NextMidnight.AddHours(-1) < DateTime.Now
                //    ? Literal.NextMidnight.AddDays(1).AddHours(-1)
                //    : Literal.NextMidnight.AddHours(-1); // 23:00, either today or tomorrow depending on current time
                IJobDetail periodicJob = JobBuilder.Create<PeriodicSurveyJob>().WithIdentity(PERIODIC_NAME).Build();
                ITrigger periodicTrigger = TriggerBuilder.Create()
                    .WithIdentity(PERIODIC_NAME)
                    //.StartNow()
                    .StartAt(Helper.NextDateTime(23, 00)) // start at 23:00
                    .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()) // daily
                    .Build();
                if (await Available(PERIODIC_NAME))
                {
                    var periodicFt = await scheduler.ScheduleJob(periodicJob, periodicTrigger);
                    logger.Debug($"PeriodicSurveyJob started successfully. Firing at {periodicFt.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                }


                IJobDetail surveyJob = JobBuilder.Create<SurveyJob>().WithIdentity(SURVEY_NAME).Build();
                ITrigger surveyTrigger = TriggerBuilder.Create()
                    .WithIdentity(SURVEY_NAME)
                    //.StartAt(DateTime.Now.AddSeconds(10))
                    .StartAt(Helper.NextDateTime(00, 10)) // start at 00:10
                    .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()) // daily
                    .Build();
                if (await Available(SURVEY_NAME))
                {
                    var surveyFt = await scheduler.ScheduleJob(surveyJob, surveyTrigger);
                    logger.Debug($"SurveyJob started successfully. Firing at {surveyFt.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                }


                IJobDetail exceededJob = JobBuilder.Create<ExceededSurveyDataJob>().WithIdentity(EXCEEDED_NAME).Build();
                ITrigger exceededTrigger = TriggerBuilder.Create()
                    .WithIdentity(EXCEEDED_NAME)
                    //.StartNow()
                    .StartAt(Helper.NextDateTime(03, 00)) // start at 03:00
                    .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()) // daily
                    .Build();
                if (await Available(EXCEEDED_NAME))
                {
                    var exceededFt = await scheduler.ScheduleJob(exceededJob, exceededTrigger);
                    logger.Debug($"ExceededSurveyDataJob started successfully. Firing at {exceededFt.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                }


                IJobDetail billJob = JobBuilder.Create<BillJob>().WithIdentity(BILL_NAME).Build();
                ITrigger billTrigger = TriggerBuilder.Create()
                    .WithIdentity(BILL_NAME)
                    //.StartNow()
                    .StartAt(Helper.NextDateTime(04, 20)) // start at 04:20
                    .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()) // daily
                    .Build();
                if (await Available(BILL_NAME))
                {
                    var billFt = await scheduler.ScheduleJob(billJob, billTrigger);
                    logger.Debug($"BillJob started successfully. Firing at {billFt.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                }

                IJobDetail backupJob = JobBuilder.Create<BackupJob>().WithIdentity(BACKUP_NAME).Build();
                ITrigger backupTrigger = TriggerBuilder.Create()
                    .WithIdentity(BACKUP_NAME)
                    //.StartNow()
                    .StartAt(Helper.NextDateTime(05, 00)) // start at 05:00
                    .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()) // daily
                    .Build();
                if (await Available(BACKUP_NAME))
                {
                    var backupFt = await scheduler.ScheduleJob(backupJob, backupTrigger);
                    logger.Debug($"BackupJob started successfully. Firing at {backupFt.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Start: Unexpected error");
                throw ex;
            }


            logger.Debug($"Jobs running currently: {string.Join("; ", await Names())}");
        }

        private async Task<IEnumerable<string>> Names()
        {
            return ((await scheduler.GetCurrentlyExecutingJobs())
               .Select(j => j.JobDetail.Key.Name))
               .Concat(
                   (await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()))
                   .Select(t => t.Name))
               .Distinct();
        }

        private async Task<bool> Available(string name)
        {
            return !(await Names()).Any(n => n == name);
        }
    }
}