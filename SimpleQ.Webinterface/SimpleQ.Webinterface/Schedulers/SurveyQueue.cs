using NLog;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using SimpleQ.Webinterface.Models;
using System.Net.Http;

namespace SimpleQ.Webinterface.Schedulers
{
    public static class SurveyQueue
    {
        private static readonly Dictionary<int, JobKey> queuedSurveys = new Dictionary<int, JobKey>();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
        private const int SURVEY_RETRY_INTERVAL = 5;

        static SurveyQueue()
        {
            scheduler.Start().Wait();
        }

        private class SurveyQueueJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                try
                {
                    logger.Debug($"Fired at {context.FireTimeUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");

                    var svyId = (int)context.JobDetail.JobDataMap["svyId"];
                    var custCode = context.JobDetail.JobDataMap["custCode"] as string;

                    lock (queuedSurveys)
                    {
                        if (!queuedSurveys.ContainsKey(svyId))
                        {
                            logger.Debug($"Survey {svyId} cancelled during sleep phase. Exiting");
                            return;
                        }
                    }

                    using (var db = new SimpleQDBEntities())
                    {
                        Random rnd = new Random();

                        var survey = await db.Surveys.Where(s => s.SvyId == svyId).FirstOrDefaultAsync();
                        if (survey == null)
                        {
                            logger.Debug($"Survey {svyId} has been deleted during sleep phase. Exiting");
                            return;
                        }

                        if (survey.Sent)
                        {
                            logger.Debug($"Survey {svyId} has been sent already. Exiting");
                            return;
                        }

                        // Anzahl an zu befragenden Personen
                        int amount = survey.Amount;

                        // Bereits befragte Personen (zwecks Verhinderung v. Mehrfachbefragungen)
                        HashSet<int> alreadyAsked = new HashSet<int>();

                        // DepIDs mit den errechneten Anzahlen v. zu befragenden Personen
                        Dictionary<int, int> depAmounts = new Dictionary<int, int>();

                        // Gesamtanzahl an Personen von allen ausgewählten Abteilungen ermitteln
                        int totalPeople = survey.Departments.SelectMany(d => d.People).Distinct().Count();
                        logger.Debug($"Survey {svyId} - totalPeople: {totalPeople}");

                        survey.Departments.ToList().ForEach(dep =>
                        {
                            // Anzahl an Personen in der aktuellen Abteilung (mit DepId = id)
                            int currPeople = dep.People.Distinct().Count();
                            // Abteilung überspringen, wenn keine Leute darin
                            if (currPeople == 0)
                                return;

                            // GEWICHTETE Anzahl an zu befragenden Personen in der aktuellen Abteilung
                            int toAsk = (int)Math.Round(amount * (currPeople / (double)totalPeople));

                            depAmounts.Add(dep.DepId, toAsk);
                        });

                        logger.Debug($"Survey {svyId} - DepAmounts before correction: {string.Join("; ", depAmounts.Select(x => "Dep" + x.Key + ": " + x.Value))}");


                        // Solange Gesamtanzahl der zu Befragenden zu klein, die Anzahl einer zufälligen Abteilung erhöhen
                        while (depAmounts.Count != 0 && depAmounts.Values.Sum() < amount)
                            depAmounts[depAmounts.ElementAt(rnd.Next(0, depAmounts.Count)).Key]++;

                        // Solange Gesamtanzahl der zu Befragenden zu groß, die Anzahl einer zufälligen Abteilung verringern
                        while (depAmounts.Count != 0 && depAmounts.Values.Sum() > amount)
                            depAmounts[depAmounts.ElementAt(rnd.Next(0, depAmounts.Count)).Key]--;

                        logger.Debug($"Survey {svyId} - DepAmounts after correction: {string.Join("; ", depAmounts.Select(x => "Dep" + x.Key + ": " + x.Value))}");

                        int totalSent = 0;
                        using (var client = new HttpClient())
                        {
                            foreach (var kv in depAmounts)
                            {
                                int sent = 0;
                                var query = (await db.Departments
                                    .Where(d => d.DepId == kv.Key && d.CustCode == custCode)
                                    .SelectMany(d => d.People)
                                    .ToListAsync())
                                    .Where(p => !alreadyAsked.Contains(p.PersId))
                                    .OrderBy(p => rnd.Next())
                                    .Take(kv.Value)
                                    .ToList();

                                foreach (var p in query)
                                {
                                    alreadyAsked.Add(p.PersId);

                                    try
                                    {
                                        // SEND SURVEY
                                        logger.Debug($"Beginning to send survey to app (SvyId: {svyId}, CustCode: {custCode}, PersId: {p.PersId}, DeviceId: {p.DeviceId})");
                                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "ZDNmNGZjODMtNTEzNC00YjA1LTkyZmUtNDRkMWJkZjRhZjVj");
                                        var obj = new
                                        {
                                            app_id = "68b8996a-f664-4130-9854-9ed7f70d5540",
                                            include_player_ids = new string[] { p.DeviceId },
                                            contents = new { en = "New survey" },
                                            content_available = true,
                                            data = new { Cancel = false, SvyId = svyId }
                                        };
                                        var response = await client.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", obj);

                                        if (!response.IsSuccessStatusCode)
                                        {
                                            logger.Error($"Failed sending survey {svyId} (StatusCode: {response.StatusCode}, Content: {response.Content})");
                                        }
                                        else
                                        {
                                            logger.Debug($"Survey {svyId} sent successfully (StatusCode: {response.StatusCode}, Content: {response.Content})");
                                            sent++;
                                        }
                                    }
                                    catch (AggregateException ex)
                                    {
                                        logger.Error(ex, $"Error while sending survey to app (SvyId: {svyId}, CustCode: {custCode}, PersId: {p.PersId}, DeviceId: {p.DeviceId})");
                                    }
                                }
                                logger.Debug($"(SvyId {svyId}) Surveys sent to Department {kv.Key}: Sent:{sent} Expected:{kv.Value}");
                                totalSent += sent;
                            }
                        }
                        logger.Debug($"(SvyId {svyId}) Total surveys sent: {totalSent}");


                        survey.Sent = totalSent != 0;
                        await db.SaveChangesAsync();

                        if (totalSent != 0)
                        {
                            queuedSurveys.Remove(svyId);
                        }
                        else
                        {
                            IJobDetail job = JobBuilder.Create<SurveyQueueJob>()
                                .UsingJobData("svyId", svyId)
                                .UsingJobData("custCode", custCode)
                                .Build();

                            ITrigger trigger = TriggerBuilder.Create()
                                .StartAt(DateTime.Now.AddHours(SURVEY_RETRY_INTERVAL))
                                .WithSimpleSchedule(x => x.WithRepeatCount(0))
                                .Build();

                            var ft = await scheduler.ScheduleJob(job, trigger);
                            queuedSurveys[svyId] = job.Key;
                            logger.Debug($"Sending survey {svyId} failed. Trying again at {ft.ToString("yyyy-MM-dd HH:mm:ss.fff")} (JobKey: {job.Key})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "SurveyQueueJob.Execute: Unexpected error");
                    throw ex;
                }
            }
        }


        public static async Task<bool> EnqueueSurvey(int svyId, DateTime startTime, string custCode)
        {
            try
            {
                logger.Debug($"Survey queuing started (SvyId: {svyId}, CustCode: {custCode})");
                lock (queuedSurveys)
                {
                    if (queuedSurveys.ContainsKey(svyId))
                    {
                        logger.Debug($"Survey {svyId} has already been queued");
                        return false;
                    }
                }

                IJobDetail job = JobBuilder.Create<SurveyQueueJob>()
                    .UsingJobData("svyId", svyId)
                    .UsingJobData("custCode", custCode)
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .StartAt(startTime < DateTime.Now ? DateTime.Now : startTime)
                    .WithSimpleSchedule(x => x.WithRepeatCount(0))
                    .Build();

                lock (queuedSurveys)
                {
                    queuedSurveys.Add(svyId, job.Key);
                }
                var ft = await scheduler.ScheduleJob(job, trigger);
                logger.Debug($"Survey {svyId} queued successfully for {ft.ToString("yyyy-MM-dd HH:mm:ss.fff")} (JobKey: {job.Key})");

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "EnqueueSurvey: Unexpected error");
                throw ex;
            }
        }

        public static async Task<bool> DequeueSurvey(int svyId)
        {
            try
            {
                JobKey key = null;
                lock (queuedSurveys)
                {
                    if (queuedSurveys.TryGetValue(svyId, out key))
                        queuedSurveys.Remove(svyId);
                }

                var deleted = await scheduler.DeleteJob(key);
                if (key != null && deleted)
                {
                    logger.Debug($"Survey {svyId} dequeued successfully");
                    return true;
                }
                else
                {
                    logger.Debug($"Survey {svyId} is not in queue ({(key == null ? "not" : "")} in dictionary, {(deleted ? "not" : "")} in schedule");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DequeueSurvey: Unexpected error");
                throw ex;
            }
        }
    }
}