using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class SurveyResultsController : Controller
    {
        #region MVC-Actions
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                if (cust == null)
                    return Http.NotFound("Customer not found.");

                var model = new SurveyResultsModel
                {
                    SurveyCategories = cust.SurveyCategories.Where(s => !s.Deactivated).ToList(),
                    Surveys = db.Surveys.Where(s => s.CustCode == CustCode).ToList(),
                    AnswerTypes = db.Surveys.Where(s => s.CustCode == CustCode).Select(s => s.AnswerType)
                        .Where(a => a.BaseId != (int)BaseQuestionTypes.OpenQuestion).Distinct().ToList()
                };

                ViewBag.emailConfirmed = cust.EmailConfirmed;
                return View("SurveyResults", model);
            }
        }

        [HttpGet]
        public ActionResult LoadSingleResult(int svyId)
        {
            bool err = false;
            using (var db = new SimpleQDBEntities())
            {
                if (!db.Customers.Any(c => c.CustCode == CustCode))
                    return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });

                Survey survey = db.Surveys
                    .Where(s => s.SvyId == svyId && s.CustCode == CustCode)
                    .FirstOrDefault();

                if (survey == null)
                    AddModelError("svyId", "Survey not found.", ref err);

                if (err) return Index();


                var model = new SingleResultModel
                {
                    Survey = survey,

                    CatName = db.SurveyCategories
                        .Where(c => c.CatId == survey.CatId && c.CustCode == CustCode)
                        .Select(c => c.CatName)
                        .FirstOrDefault(),

                    TypeName = db.AnswerTypes
                        .Where(a => a.TypeId == survey.TypeId)
                        .Select(a => a.TypeDesc)
                        .FirstOrDefault(),

                    DepartmentNames = db.Surveys
                        .Where(s => s.SvyId == survey.SvyId)
                        .SelectMany(s => s.Departments)
                        .Select(d => d.DepName)
                        .ToList(),

                    Votes = (survey.AnswerType.BaseId != (int)BaseQuestionTypes.OpenQuestion) ? SelectVotesFromSurvey(survey) : null,

                    FreeTextVotes = (survey.AnswerType.BaseId != (int)BaseQuestionTypes.OpenQuestion) ? db.Votes
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
            bool err = false;
            //SAMPLE DATA
            //req = new MultiResultModel
            //{
            //    CatId = 4,
            //    TypeId = 2,
            //    StartDate = DateTime.Now.AddYears(-1),
            //    EndDate = DateTime.Now,
            //};

            if (req == null)
                AddModelError("Model", "Model object must not be null.", ref err);
            if (req.StartDate >= req.EndDate)
                AddModelError("StartDate", "StartDate must be smaller than EndDate.", ref err);

            using (var db = new SimpleQDBEntities())
            {
                if (!db.Customers.Any(c => c.CustCode == CustCode))
                    return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });

                var selectedSurveys = db.Surveys
                    .Where(s => s.CatId == req.CatId && s.TypeId == req.TypeId
                     && s.StartDate >= req.StartDate && s.StartDate <= req.EndDate
                     && s.CustCode == CustCode);

                string catName = db.SurveyCategories
                            .Where(c => c.CatId == req.CatId && c.CustCode == CustCode)
                            .Select(c => c.CatName)
                            .FirstOrDefault();
                if (catName == null)
                    AddModelError("CatId", "Category not found.", ref err);

                var type = db.AnswerTypes
                            .Where(a => a.TypeId == req.TypeId && a.BaseId != (int)BaseQuestionTypes.OpenQuestion)
                            .FirstOrDefault();
                if (type == null)
                    AddModelError("TypeId", "AnswerType does not exist.", ref err);

                if (err) return Index();

                MultiResultModel model;
                if (selectedSurveys.Count() == 0)
                {
                    model = new MultiResultModel
                    {
                        CatName = catName,

                        TypeName = type.TypeDesc,

                        AvgAmount = 0,

                        DepartmentNames = new List<string>(),

                        SurveyDates = new List<DateTime>(),

                        Votes = new List<KeyValuePair<string, List<int>>>()
                    };
                }
                else
                {
                    model = new MultiResultModel
                    {
                        CatName = catName,

                        TypeName = type.TypeDesc,

                        AvgAmount = (selectedSurveys.Sum(s => s.Amount) / (double)selectedSurveys.Count()),

                        DepartmentNames = selectedSurveys
                            .SelectMany(s => s.Departments)
                            .Select(d => d.DepName)
                            .Distinct()
                            .ToList(),

                        SurveyDates = selectedSurveys.Select(s => s.StartDate).ToList(),

                        Votes = SelectVotesFromSurveyGrouped(selectedSurveys, type.BaseId)
                    };
                }

                return PartialView(viewName: "_MultiResult", model: model);
            }
        }
        #endregion

        #region Helpers
        private List<KeyValuePair<string, int>> SelectVotesFromSurvey(Survey survey)
        {
            var list = new List<KeyValuePair<string, int>>();
            var query = survey.AnswerOptions.ToList();

            if (survey.AnswerType.BaseId == (int)BaseQuestionTypes.LikertScaleQuestion)
            {
                query = query.OrderBy(a => a.AnsText).ToList();

                var first = query.Where(a => a.FirstPosition == true).First();
                query.Remove(first);
                query.Insert(0, first);

                var last = query.Where(a => a.FirstPosition == false).First();
                query.Remove(last);
                query.Add(last);
            }
            else
            {
                query = query.OrderByDescending(a => a.Votes.Count()).ToList();
            }

            query.ForEach(a =>
            {
                list.Add(new KeyValuePair<string, int>(a.AnsText, a.Votes.Count()));
            });

            return list;
        }

        private List<KeyValuePair<string, List<int>>> SelectVotesFromSurveyGrouped(IQueryable<Survey> selectedSurveys, int baseId)
        {
            var list = new List<KeyValuePair<string, List<int>>>();
            var query = selectedSurveys.SelectMany(s => s.AnswerOptions).ToList();

            if (baseId == (int)BaseQuestionTypes.LikertScaleQuestion)
            {
                query = query.OrderBy(a => a.AnsText).ToList();

                var first = query.Where(a => a.FirstPosition == true).ToList();
                query.RemoveAll(a => a.FirstPosition == true);
                query.InsertRange(0, first);

                var last = query.Where(a => a.FirstPosition == false).ToList();
                query.RemoveAll(a => a.FirstPosition == false);
                query.AddRange(last);
            }

            query.Select(a => a.AnsText)
                .Distinct()
                .ToList()
                .ForEach(t => list.Add(new KeyValuePair<string, List<int>>(t, new List<int>())));

            selectedSurveys.ToList()
                .ForEach(s =>
                {
                    s.AnswerOptions
                        .ToList()
                        .ForEach(a =>
                        {
                            list.Where(kv => kv.Key == a.AnsText).First().Value.Add(a.Votes.Count());
                        });
                });

            return list;
        }

        private void AddModelError(string key, string errorMessage, ref bool error)
        {
            ModelState.AddModelError(key, errorMessage);
            error = true;
        }

        private string CustCode => HttpContext.GetOwinContext().Authentication.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
        #endregion
    }
}