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
using SimpleQ.Webinterface.Models.Enums;

namespace SimpleQ.Webinterface.Schedulers
{
    public class PeriodicSurveyJob : IJob
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Debug($"Fired at {context.FireTimeUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");

                using (var db = new SimpleQDBEntities())
                {
                    var query = (await db.Surveys
                        .Where(s => s.Period != null)
                        .ToListAsync())
                        .Where(s => (s.StartDate + TimeSpan.FromTicks(s.Period.Value)).Date <= Helper.Tomorrow)
                        .ToList();

                    foreach (var s in query)
                    {
                        logger.Debug($"Survey to create new: {s.SvyId}");
                        var newSvy = await db.Surveys
                            .AsNoTracking()
                            .Include("Departments")
                            .Include("AnswerOptions")
                            .Where(svy => svy.SvyId == s.SvyId).FirstAsync();

                        if ((newSvy.StartDate + TimeSpan.FromTicks(s.Period.Value)).Date == Helper.Tomorrow)
                        {
                            newSvy.StartDate += TimeSpan.FromTicks(newSvy.Period.Value);
                            newSvy.EndDate += TimeSpan.FromTicks(newSvy.Period.Value);
                        }
                        else
                        {
                            newSvy.StartDate = Helper.Tomorrow
                                .AddHours(newSvy.StartDate.Hour)
                                .AddMinutes(newSvy.StartDate.Minute)
                                .AddSeconds(newSvy.StartDate.Second);

                            newSvy.EndDate += newSvy.StartDate - s.StartDate;
                        }

                        newSvy.Template = s.Template;
                        newSvy.Sent = false;
                        newSvy.AnswerType = null;
                        newSvy.SurveyCategory = null;
                        newSvy.SvyId = default(int);

                        s.Period = null;
                        s.Template = false;

                        var departments = newSvy.Departments.ToList();
                        var answerOptions = newSvy.AnswerOptions.ToList();
                        newSvy.Departments.Clear();
                        newSvy.AnswerOptions.Clear();

                        int totalPeople = (await db.Departments.ToListAsync()).Where(d => departments.Select(dep => dep.DepId).Contains(d.DepId) && d.CustCode == newSvy.CustCode)
                            .SelectMany(d => d.People).Distinct().Count();
                        if (newSvy.Amount > totalPeople)
                            newSvy.Amount = totalPeople;
                        logger.Debug($"Total people amount: {totalPeople} (CustCode: {newSvy.CustCode}, SvyText: {newSvy.SvyText})");

                        db.Surveys.Add(newSvy);
                        await db.SaveChangesAsync();
                        logger.Debug($"Survey successfully created. SvyId: {newSvy.SvyId}");

                        departments.RemoveAll(d => !db.Departments.Any(dep => dep.DepId == d.DepId && dep.CustCode == d.CustCode));
                        departments.ForEach(d => db.Departments.Where(dep => dep.DepId == d.DepId && dep.CustCode == d.CustCode).FirstOrDefault()?.Surveys.Add(newSvy));
                        await db.SaveChangesAsync();
                        logger.Debug($"Departments added successfully for SvyId: {newSvy.SvyId}");

                        var baseId = await db.AnswerTypes.Where(a => a.TypeId == newSvy.TypeId).Select(a => a.BaseId).FirstOrDefaultAsync();
                        if (baseId == (int)BaseQuestionTypes.LikertScaleQuestion)
                        {
                            db.AnswerOptions.Add(new AnswerOption { SvyId = newSvy.SvyId, AnsText = answerOptions.Where(a => a.FirstPosition == true).First().AnsText, FirstPosition = true });
                            db.AnswerOptions.Add(new AnswerOption { SvyId = newSvy.SvyId, AnsText = answerOptions.Where(a => a.FirstPosition == false).First().AnsText, FirstPosition = false });
                            logger.Debug($"Likert scale answer options set successfully for SvyId: {newSvy.SvyId}");
                        }
                        else if (baseId != (int)BaseQuestionTypes.FixedAnswerQuestion && baseId != (int)BaseQuestionTypes.OpenQuestion)
                        {
                            answerOptions.Select(a => a.AnsText).ToList().ForEach(text =>
                            {
                                db.AnswerOptions.Add(new AnswerOption { SvyId = newSvy.SvyId, AnsText = text });
                            });
                            logger.Debug($"Answer options set successfully for SvyId: {newSvy.SvyId}");
                        }
                        await db.SaveChangesAsync();
                        logger.Debug($"{newSvy.SvyId} created successfully");
                    }
                    logger.Debug($"{query.Count()} surveys created successfully");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Scheduling periodic surveys failed unexpectedly");
            }
            finally
            {
                logger.Debug($"Next fire time {context.NextFireTimeUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")}");
            }
        }
    }
}