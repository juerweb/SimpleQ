using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class SurveyResultsController : Controller
    {
        [HttpGet]
        public ActionResult LoadSingleResult(int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                // EXCEPTION HANDLING (NO SURVEY FOUND)
                Survey survey = db.Surveys
                    .Where(s => s.SvyId == svyId && s.CustCode == CustCode)
                    .FirstOrDefault();

                if (survey == null)
                    return Http.NotFound("Survey not found.");
                

                var model = new SingleResultModel
                {
                    Survey = survey,

                    CatName = db.SurveyCategories
                        .Where(c => c.CatId == survey.CatId && c.CustCode == CustCode)
                        .Select(c => c.CatName)
                        .FirstOrDefault(),

                    TypeName = db.AnswerTypes
                        .Where(a => a.TypeId == survey.TypeId)
                        .Select(a => a.TypeDesc) // GLOBALIZATION!
                        .FirstOrDefault(),

                    DepartmentNames = db.Surveys
                        .Where(s => s.SvyId == survey.SvyId)
                        .SelectMany(s => s.Departments)
                        .Select(d => d.DepName)
                        .ToList(),

                    Votes = (survey.TypeId != 4) ? SelectVotesFromSurvey(db, survey) : null,

                    FreeTextVotes = (survey.TypeId == 4) ? db.Votes
                        .Where(v => v.AnswerOptions.FirstOrDefault().SvyId == survey.SvyId)
                        .Select(v => v.VoteText)
                        .ToList()
                        : null
                };
                return PartialView(viewName: "_SingleResult", model: model);
            }
        }

        [HttpPost]
        public ActionResult LoadMultiResult(MultiResultModel req)
        {
            // SAMPLE DATA
            //req = new MultiResultModel
            //{
            //    CatId = 4,
            //    TypeId = 1,
            //    StartDate = DateTime.Now.AddYears(-1),
            //    EndDate = DateTime.Now,
            //};

            if (req == null)
                return Http.BadRequest("Model object must not be null.");
            if (req.StartDate >= req.EndDate)
                return Http.Conflict("StartDate must be smaller than EndDate.");

            using (var db = new SimpleQDBEntities())
            {
                var selectedSurveys = db.Surveys
                    .Where(s => s.CatId == req.CatId && s.TypeId == req.TypeId
                     && s.StartDate >= req.StartDate && s.StartDate <= req.EndDate
                     && s.CustCode == CustCode);

                string catName = db.SurveyCategories
                            .Where(c => c.CatId == req.CatId && c.CustCode == CustCode)
                            .Select(c => c.CatName)
                            .FirstOrDefault();
                if (catName == null)
                    return Http.NotFound("Category not found.");

                string typeName = db.AnswerTypes
                            .Where(a => a.TypeId == req.TypeId)
                            .Select(a => a.TypeDesc) // GLOBALIZATION!
                            .FirstOrDefault();
                if (typeName == null)
                    return Http.Conflict("Answer type does not exist.");


                MultiResultModel model;
                if (selectedSurveys.Count() == 0)
                {
                    model = new MultiResultModel
                    {
                        CatName = catName,

                        TypeName = typeName,

                        AvgAmount = 0,

                        DepartmentNames = new List<string>(),

                        SurveyDates = new List<DateTime>(),

                        Votes = new Dictionary<string, List<int>>()
                    };
                }
                else
                {
                    model = new MultiResultModel
                    {
                        CatName = catName,

                        TypeName = typeName,

                        AvgAmount = (selectedSurveys.Sum(s => s.Amount) / (double)selectedSurveys.Count()),

                        DepartmentNames = selectedSurveys
                            .SelectMany(s => s.Departments)
                            .Select(d => d.DepName)
                            .Distinct()
                            .ToList(),

                        SurveyDates = selectedSurveys.Select(s => s.StartDate).ToList(),

                        Votes = SelectVotesFromSurveyGrouped(db, selectedSurveys)
                    };
                }

                return PartialView(viewName: "_MultiResult", model: model);
            }
        }

        private Dictionary<string, int> SelectVotesFromSurvey(SimpleQDBEntities db, Survey survey)
        {
            return db.Votes
                .Where(v => v.AnswerOptions.FirstOrDefault().SvyId == survey.SvyId)
                .SelectMany(v => v.AnswerOptions)
                .GroupBy(a => a.AnsId)
                .ToDictionary(g => g.Where(a => a.AnsId == g.Key).Select(a => a.AnsText).FirstOrDefault(), g => g.Count())
                .Concat(db.AnswerOptions.Where(a => a.SvyId == survey.SvyId && a.Votes.Count == 0).AsEnumerable().Select(a => new KeyValuePair<string, int>(a.AnsText, 0)))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private Dictionary<string, List<int>> SelectVotesFromSurveyGrouped(SimpleQDBEntities db, IQueryable<Survey> selectedSurveys)
        {
            Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();

            selectedSurveys
                .SelectMany(s => s.AnswerOptions)
                .Select(a => a.AnsText)
                .Distinct()
                .ToList()
                .ForEach(t => dict.Add(t, new List<int>()));

            selectedSurveys.ToList()
                .ForEach(s =>
                {
                    s.AnswerOptions
                        .ToList()
                        .ForEach(a =>
                        {
                            dict[a.AnsText].Add(a.Votes.Count());
                        });
                });

            return dict;
        }

        private string CustCode
        {
            get
            {
                return Session["custCode"] as string;
            }
        }
    }
}