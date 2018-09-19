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
                        .SelectMany(s => s.Askings)
                        .Select(a => a.Department.DepName)
                        .ToList(),

                    Votes = (survey.TypeId != 4) ? db.Votes
                        .Where(v => v.AnswerOptions.FirstOrDefault().SvyId == survey.SvyId)
                        .SelectMany(v => v.AnswerOptions)
                        .GroupBy(a => a.AnsId)
                        .ToDictionary(g => g.Where(a => a.AnsId == g.Key).Select(a => a.AnsText).FirstOrDefault(), g => g.Count())
                        .Concat(db.AnswerOptions.Where(a => a.SvyId == survey.SvyId && a.Votes.Count == 0).AsEnumerable().Select(a => new KeyValuePair<string, int>(a.AnsText, 0)))
                        .ToDictionary(kv => kv.Key, kv => kv.Value)
                        : null,

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
        public ActionResult LoadMultiResult(MultiResultsModel req)
        {
            using (var db = new SimpleQDBEntities())
            {
                IQueryable<Survey> surveys = db.Surveys
                    .Where(s => s.CatId == req.CatId && s.TypeId == req.TypeId
                            && s.StartDate >= req.StartDate && s.EndDate <= req.EndDate
                            && s.CustCode == CustCode);

                var model = new MultiResultsModel
                {
                    CatName = db.SurveyCategories
                        .Where(c => c.CatId == req.CatId && c.CustCode == CustCode)
                        .Select(c => c.CatName)
                        .First(),

                    TypeName = db.AnswerTypes
                        .Where(a => a.TypeId == req.TypeId)
                        .Select(a => a.TypeDesc) // GLOBALIZATION!
                        .First(),

                    DepartmentNames = surveys
                        .SelectMany(s => s.Askings)
                        .Select(a => a.Department.DepName)
                        .Distinct()
                        .ToList()

                    //Votes = ...
                };

                return PartialView(viewName: "_MultiResult", model: null);
            }
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