using NLog;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace SimpleQ.Webinterface.Schedulers
{
    public class PeriodicSurveyScheduler : Scheduler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        protected override string Name => "periodic survey scheduler";

        protected override void Schedule()
        {
            try
            {
                // Sleep bis um 23:00
                logger.Debug($"Survey scheduler sleeping for {Literal.NextMidnight.Subtract(TimeSpan.FromHours(1)).ToString(@"hh\:mm\:ss\.fff")}");
                if (DateTime.Now >= Convert.ToDateTime("23:00") && DateTime.Now <= Convert.ToDateTime("00:00"))
                    Thread.Sleep((int)Literal.NextMidnight.Add(TimeSpan.FromDays(1)).Subtract(TimeSpan.FromHours(1)).TotalMilliseconds);
                else
                    Thread.Sleep((int)Literal.NextMidnight.Subtract(TimeSpan.FromHours(1)).TotalMilliseconds);

                using (var db = new SimpleQDBEntities())
                {
                    var query = db.Surveys
                        .Where(s => s.Period != null)
                        .ToList()
                        .Where(s => (s.StartDate + TimeSpan.FromTicks(s.Period.Value)).Date == Literal.Tomorrow())
                        .ToList();

                    query.ForEach(s =>
                    {
                        logger.Debug($"Survey to create new: {s.SvyId}");
                        var newSvy = db.Surveys
                            .AsNoTracking()
                            .Include("Departments")
                            .Include("AnswerOptions")
                            .Where(svy => svy.SvyId == s.SvyId).First();

                        newSvy.StartDate += TimeSpan.FromTicks(newSvy.Period.Value);
                        newSvy.EndDate += TimeSpan.FromTicks(newSvy.Period.Value);
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

                        int totalPeople = db.Departments.ToList().Where(d => departments.Select(dep => dep.DepId).Contains(d.DepId) && d.CustCode == newSvy.CustCode)
                            .SelectMany(d => d.People).Distinct().Count();
                        if (newSvy.Amount > totalPeople)
                            newSvy.Amount = totalPeople;
                        logger.Debug($"Total people amount: {totalPeople} (CustCode: {newSvy.CustCode}, SvyText: {newSvy.SvyText})");

                        db.Surveys.Add(newSvy);
                        db.SaveChanges();
                        logger.Debug($"Survey successfully created. SvyId: {newSvy.SvyId}");

                        departments.RemoveAll(d => !db.Departments.Any(dep => dep.DepId == d.DepId && dep.CustCode == d.CustCode));
                        departments.ForEach(d => db.Departments.Where(dep => dep.DepId == d.DepId && dep.CustCode == d.CustCode).FirstOrDefault()?.Surveys.Add(newSvy));
                        db.SaveChanges();
                        logger.Debug($"Departments added successfully for SvyId: {newSvy.SvyId}");

                        var baseId = db.AnswerTypes.Where(a => a.TypeId == newSvy.TypeId).FirstOrDefault().BaseId;
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
                        db.SaveChanges();
                        logger.Debug($"{newSvy.SvyId} created successfully");
                    });
                    logger.Debug($"{query.Count()} surveys created successfully");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Schedule: Scheduling periodic surveys failed unexpectedly");
            }
        }
    }
}