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

                    DepartmentNames = db.Askings
                        .Where(a => a.SvyId == survey.SvyId && a.CustCode == CustCode)
                        .Select(a => a.Department.DepName)
                        .ToList(),

                    Votes = (survey.TypeId == 3) // EIN-WORT
                        ? db.Votes
                            .Where(v => v.SvyId == survey.SvyId && v.CustCode == CustCode)
                            .GroupBy(v => v.VoteText)
                            .Select(g => new { Key = g.Key, Value = g.Count() })
                            .ToDictionary(x => x.Key, x => x.Value)
                        : (survey.TypeId == 4) // VORGEGEBENE TEXTANTWORT
                        ? db.Votes
                            .Where(v => v.SvyId == survey.SvyId && v.CustCode == CustCode)
                            .GroupBy(v => v.SpecId)
                            .Select(g => new
                            {
                                Key = g.Where(v => v.SpecId == g.Key).Select(v => v.SpecifiedTextAnswer.SpecText).FirstOrDefault(),
                                Value = g.Count()
                            })
                            .ToDictionary(x => x.Key, x => x.Value)
                        : db.Votes
                            .Where(v => v.SvyId == survey.SvyId && v.CustCode == CustCode)
                            .GroupBy(v => v.AnsId)
                            .Select(g => new
                            {
                                Key = g.Where(v => v.AnsId == g.Key).Select(v => v.Answer.AnsDesc).FirstOrDefault(),
                                Value = g.Count()
                            })
                            .ToDictionary(x => x.Key, x => x.Value)

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
                return Session["custCode"] as string;
            }
        }
    }
}