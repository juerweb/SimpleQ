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
using NLog;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class SurveyResultsController : BaseController
    {
        private string errString;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                logger.Debug("Loading survey results.");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault();
                    if (cust == null)
                    {
                        logger.Warn($"Loading failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var model = new SurveyResultsModel
                    {
                        SurveyCategories = cust.SurveyCategories.Where(s => !s.Deactivated).ToList(),
                        Surveys = db.Surveys.Where(s => s.CustCode == CustCode).ToList(),
                        AnswerTypes = db.Surveys.Where(s => s.CustCode == CustCode).Select(s => s.AnswerType)
                            .Where(a => a.BaseId != (int)BaseQuestionTypes.OpenQuestion).Distinct().ToList(),
                        SurveyTexts = db.Surveys.Select(s => s.SvyText).ToList()
                    };

                    ViewBag.emailConfirmed = cust.EmailConfirmed;
                    logger.Debug("Survey results loaded successfully.");

                    return View("SurveyResults", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[GET]Index: Unexpected error");
                return View("Error", model);
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpGet]
        public ActionResult LoadSingleResult(int svyId)
        {
            try
            {
                logger.Debug($"Requested to load single result of survey. (SvyId: {svyId}, CustCode: {CustCode})");
                bool err = false;
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Loading single result failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

                    if (db.Surveys.Where(s => s.CustCode == CustCode).Count() == 0)
                    {
                        logger.Debug($"Loading single result failed. No existing surveys for Customer: {CustCode}");
                        return PartialView("_Error", new ErrorModel { Title = "No Surveys", Message = "You haven't created any surveys yet." });
                    }

                    Survey survey = db.Surveys
                        .Where(s => s.SvyId == svyId && s.CustCode == CustCode)
                        .FirstOrDefault();

                    if (survey == null)
                        AddModelError("svyId", "Survey not found.", ref err);

                    if (err)
                    {
                        logger.Debug($"Loading single result failed due to model validation failure. Exiting action.");
                        return PartialView("_Error", new ErrorModel { Title = "Failed to load survey", Message = errString });
                    }


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

                    logger.Debug("Single result loaded successfully.");
                    return PartialView(viewName: "_SingleResult", model: model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[GET]LoadSingleResult: Unexpected error");
                return PartialView("_Error", model);
            }
        }

        [HttpPost]
        public ActionResult LoadMultiResult(MultiResultModel req)
        {
            try
            {
                logger.Debug($"Requested to load multi result of surveys. (SurveyText: {req.SurveyText}, CustCode: {CustCode})");
                bool err = false;
                //SAMPLE DATA
                //req = new MultiResultModel
                //{
                //    CatId = 4,
                //    TypeId = 2,
                //    StartDate = DateTime.Now.AddYears(-1),
                //    EndDate = DateTime.Now,
                //    SurveyText = "Ist der Chef leiwand?"//"Finden Sie der Chef ist ein Arschloch?"
                //};

                if (req == null)
                    AddModelError("Model", "Model object must not be null.", ref err);
                if (req.StartDate > req.EndDate)
                    AddModelError("StartDate", "Start date must be smaller than end date.", ref err);
                if (string.IsNullOrEmpty(req.SurveyText))
                    AddModelError("SurveyText", "Survey text must not be empty.", ref err);

                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Loading multi result failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

                    if (db.Surveys.Where(s => s.CustCode == CustCode).Count() == 0)
                    {
                        logger.Debug($"Loading single result failed. No existing surveys for Customer: {CustCode}");
                        return PartialView("_Error", new ErrorModel { Title = "No Surveys", Message = "You haven't created any surveys yet." });
                    }

                    var selectedSurveys = db.Surveys
                        .Where(s => s.CatId == req.CatId && s.TypeId == req.TypeId
                         && s.StartDate >= req.StartDate && s.StartDate <= req.EndDate
                         && s.SvyText.Trim().ToLower() == req.SurveyText.Trim().ToLower()
                         && s.CustCode == CustCode);

                    logger.Debug($"{selectedSurveys.Count()} surveys selected for multi result. (SurveyText: {req.SurveyText}, CustCode: {CustCode}");

                    string catName = db.SurveyCategories
                                .Where(c => c.CatId == req.CatId && c.CustCode == CustCode)
                                .Select(c => c.CatName)
                                .FirstOrDefault();
                    logger.Debug($"Category name for CatId {req.CatId} of Customer {CustCode}: {req.CatName}");
                    if (catName == null)
                        AddModelError("CatId", "Category not found.", ref err);


                    var type = db.AnswerTypes
                                .Where(a => a.TypeId == req.TypeId && a.BaseId != (int)BaseQuestionTypes.OpenQuestion)
                                .FirstOrDefault();
                    if (type == null)
                        AddModelError("TypeId", "AnswerType does not exist.", ref err);

                    if (err)
                    {
                        logger.Debug($"Loading single result failed due to model validation failure. Exiting action.");
                        return PartialView("_Error", new ErrorModel { Title = "Failed to load survey", Message = errString });
                    }

                    MultiResultModel model;
                    if (selectedSurveys?.Count() == 0)
                    {
                        logger.Debug("Creating empty multi result model.");
                        model = new MultiResultModel
                        {
                            CatName = catName,

                            TypeName = type?.TypeDesc,

                            AvgAmount = 0,

                            DepartmentNames = new List<string>(),

                            SurveyDates = new List<DateTime>(),

                            Votes = new List<KeyValuePair<string, List<int>>>()
                        };
                    }
                    else
                    {
                        logger.Debug("Loading data for multi result model");
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

                    logger.Debug("Multi result loaded successfully.");
                    return PartialView(viewName: "_MultiResult", model: model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[POST]LoadMultiResult: Unexpected error");
                return PartialView("_Error", model);
            }
        }

        [HttpGet]
        public ActionResult LoadTypesByCategory(int catId)
        {
            try
            {
                logger.Debug($"Requested to load survey types by category. (CatId: {catId}, CustCode: {CustCode})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Loading survey types failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var cat = db.SurveyCategories.Where(c => c.CustCode == CustCode && c.CatId == catId).FirstOrDefault();
                    if (cat == null)
                    {
                        logger.Debug("Category not found: {catId}");
                        return Http.NotFound("Category not found.");
                    }

                    var types = cat.Surveys
                        .Select(s => new AnswerType
                        {
                            TypeId = s.AnswerType.TypeId,
                            TypeDesc = s.AnswerType.TypeDesc,
                            BaseId = s.AnswerType.BaseId
                        })
                        .Where(t => t.BaseId != (int)BaseQuestionTypes.OpenQuestion)
                        .GroupBy(t => t.TypeId)
                        .Select(g => g.First())
                        .ToList();

                    logger.Debug($"{types.Count} answer types loaded successfully.");

                    return Json(types, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]LoadTypesByCategory: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public ActionResult LoadSurveys(int catId, int typeId)
        {
            try
            {
                logger.Debug($"Requested to load survey texts. (CatId: {catId}, TypeId: {typeId}, CustCode: {CustCode})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Loading survey texts failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var query = db.Surveys
                        .Where(s => s.CatId == catId
                            && s.TypeId == typeId
                            && s.CustCode == CustCode).ToList()
                        .Select(s => s.SvyText)
                        .Distinct();

                    if (query.Count() == 0)
                    {
                        logger.Debug("Loading survey texts failed. No surveys found.");
                        return Http.NotFound("No surveys found");
                    }

                    logger.Debug($"{query.Count()} distinct survey texts loaded successfully.");
                    return Json(new { SurveyTexts = query.ToList() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]LoadSurveys: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public ActionResult LoadSurveyDates(int catId, int typeId, string svyText)
        {
            try
            {
                logger.Debug($"Requested to load survey dates (CatId: {catId}, TypeId: {typeId}, SvyText: {svyText}, CustCode: {CustCode})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Loading survey dates failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found");
                    }

                    var query = db.Surveys.Where(s => s.CatId == catId && s.TypeId == typeId && s.CustCode == CustCode && s.SvyText.ToLower().Trim() == svyText.ToLower().Trim());
                    if (query.Count() == 0)
                    {
                        logger.Debug("Loading survey dates failed. No surveys found.");
                        return Http.NotFound("No surveys found");
                    }

                    var startDate = query.Select(s => s.StartDate).Min().ToString("yyyy-MM-dd");
                    var endDate = query.Select(s => s.StartDate).Max().ToString("yyyy-MM-dd");

                    logger.Debug($"Survey dates loaded successfully: {startDate} - {endDate}");

                    return Json(new
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]LoadSurveyDates: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }
        #endregion

        #region Helpers
        private List<KeyValuePair<string, int>> SelectVotesFromSurvey(Survey survey)
        {
            try
            {
                logger.Debug("Started selecting votes from survey");
                var list = new List<KeyValuePair<string, int>>();
                var query = survey.AnswerOptions.ToList();
                logger.Debug($"{query.Count()} answer options selected. (SvyId: {survey.SvyId}, CustCode: {CustCode}");

                if (survey.AnswerType.BaseId == (int)BaseQuestionTypes.LikertScaleQuestion)
                {
                    logger.Debug("Ordering Likert Scale answer options appropriately");
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
                    logger.Debug("Ordering answer options descending");
                    query = query.OrderByDescending(a => a.Votes.Count()).ToList();
                }

                query.ForEach(a =>
                {
                    list.Add(new KeyValuePair<string, int>(a.AnsText, a.Votes.Count()));
                });
                logger.Debug("Votes selected successfully");

                return list;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SelectVotesFromSurvey: Unexpected error");
                throw ex;
            }
        }

        private List<KeyValuePair<string, List<int>>> SelectVotesFromSurveyGrouped(IQueryable<Survey> selectedSurveys, int baseId)
        {
            try
            {
                logger.Debug("Starting to select votes from survey grouped");
                var list = new List<KeyValuePair<string, List<int>>>();
                var query = selectedSurveys.SelectMany(s => s.AnswerOptions).ToList();
                logger.Debug($"{query.Count()} answer options selected. (SurveyText: {selectedSurveys.Select(s => s.SvyText).FirstOrDefault()}, CustCode: {CustCode}");

                if (baseId == (int)BaseQuestionTypes.LikertScaleQuestion)
                {
                    logger.Debug("Ordering Likert Scale answer options appropriately");
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

                logger.Debug($"List with answer option texts created. ({list.Count} entries)");

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

                logger.Debug("Votes selected successfully");

                return list;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SelectVotesFromSurveyGrouped: Unexpected error");
                throw ex;
            }
        }

        protected override void AddModelError(string key, string errorMessage, ref bool error)
        {
            try
            {
                logger.Debug($"Model error: {key}: {errorMessage}");
                errString += $"{key}: {errorMessage}{Environment.NewLine}";
                error = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "AddModelError: Unexpected error");
                throw ex;
            }
        }
        #endregion
    }
}