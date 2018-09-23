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
            //// SAMPLE DATA
            //req = new MultiResultsModel
            //{
            //    CatId = 4,
            //    TypeId = 1,
            //    StartDate = DateTime.Now.AddYears(-1),
            //    EndDate = DateTime.Now,
            //};

            using (var db = new SimpleQDBEntities())
            {
                var selectedSurveys = db.Surveys
                    .Where(s => s.CatId == req.CatId && s.TypeId == req.TypeId
                     && s.StartDate >= req.StartDate && s.EndDate <= req.EndDate);

                var model = new MultiResultModel
                {
                    CatName = db.SurveyCategories
                        .Where(c => c.CatId == req.CatId && c.CustCode == CustCode)
                        .Select(c => c.CatName)
                        .FirstOrDefault(),

                    TypeName = db.AnswerTypes
                        .Where(a => a.TypeId == req.TypeId)
                        .Select(a => a.TypeDesc) // GLOBALIZATION!
                        .FirstOrDefault(),

                    AvgAmount = (selectedSurveys.Sum(s => s.Amount) / (double)selectedSurveys.Count()),

                    DepartmentNames = selectedSurveys
                        .SelectMany(s => s.Departments)
                        .Select(d => d.DepName)
                        .Distinct()
                        .ToList(),

                    Votes = selectedSurveys
                        .ToList()
                        .Select(s => SelectVotesFromSurvey(db, s))
                        .ToList()
                };

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

        private string CustCode
        {
            get
            {
                return "m4rku5";//Session["custCode"] as string;
            }
        }
    }
}