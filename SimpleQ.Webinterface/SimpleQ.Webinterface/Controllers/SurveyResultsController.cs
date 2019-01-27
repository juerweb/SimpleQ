using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Models.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
        public async Task<ActionResult> Index()
        {
            try
            {
                logger.Debug("Loading survey results.");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Warn($"Loading failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var model = new SurveyResultsModel
                    {
                        Surveys = (await db.Surveys.Where(s => s.CustCode == CustCode)
                            .ToListAsync())
                            .GroupBy(s => new { s.SvyText, s.SurveyCategory, s.AnswerType })
                            .Select(g =>
                                new SurveyResultsModel.SurveyResultWrapper
                                {
                                    SvyText = g.Key.SvyText,
                                    Amount = g.Count(),
                                    SurveyCategory = g.Key.SurveyCategory,
                                    AnswerType = g.Key.AnswerType,
                                    StartDate = g.Min(s => s.StartDate),
                                    EndDate = g.Max(s => s.EndDate),
                                    Departments = g.SelectMany(s => s.Departments.Select(d => d.DepName)).Distinct().ToList()
                                })
                             .ToList()
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
        public async Task<ActionResult> LoadSingleResult(string svyText)
        {
            try
            {
                logger.Debug($"Requested to load single result of survey. (SvyText: {svyText}, CustCode: {CustCode})");
                bool err = false;
                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Loading single result failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

                    if (await db.Surveys.Where(s => s.CustCode == CustCode).CountAsync() == 0)
                    {
                        logger.Debug($"Loading single result failed. No existing surveys for Customer: {CustCode}");
                        return PartialView("_Error", new ErrorModel { Title = "No Surveys", Message = "You haven't created any surveys yet." });
                    }

                    var surveys = await db.Surveys
                        .Where(s => s.SvyText == svyText && s.CustCode == CustCode)
                        .ToListAsync();

                    if (surveys.Count() == 0)
                        AddModelError("Survey", "Survey not found.", ref err);

                    if (err)
                    {
                        logger.Debug($"Loading single result failed due to model validation failure. Exiting action.");
                        return PartialView("_Error", new ErrorModel { Title = "Failed to load survey", Message = errString });
                    }


                    var model = new List<SingleResultModel>();

                    foreach (var s in surveys)
                    {
                        model.Add(new SingleResultModel
                        {
                            SvyId = s.SvyId,

                            SvyText = s.SvyText,

                            StartDate = s.StartDate,

                            EndDate = s.EndDate,

                            Period = TimeSpan.FromTicks(s.Period ?? 0L),

                            Votes = (s.AnswerType.BaseId != (int)BaseQuestionTypes.OpenQuestion) ? SelectVotesFromSurvey(s) : null,

                            FreeTextVotes = (s.AnswerType.BaseId == (int)BaseQuestionTypes.OpenQuestion)
                                ? await db.Votes
                                    .Where(v => v.AnswerOptions.FirstOrDefault().SvyId == s.SvyId)
                                    .Select(v => v.VoteText)
                                    .ToListAsync()
                                : null
                        });
                    }

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

        [HttpGet]
        public async Task<ActionResult> LoadMultiResult(string svyText, int catId, int typeId)
        {
            try
            {
                logger.Debug($"Requested to load multi result of surveys. (SurveyText: {svyText}, CustCode: {CustCode})");
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
                if (string.IsNullOrEmpty(svyText))
                    AddModelError("Survey", "Survey text must not be empty.", ref err);

                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Loading multi result failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

                    if (await db.Surveys.Where(s => s.CustCode == CustCode).CountAsync() == 0)
                    {
                        logger.Debug($"Loading single result failed. No existing surveys for Customer: {CustCode}");
                        return PartialView("_Error", new ErrorModel { Title = "No Surveys", Message = "You haven't created any surveys yet." });
                    }

                    var selectedSurveys = db.Surveys
                        .Where(s => s.CatId == catId 
                         && s.TypeId == typeId
                         && s.SvyText.Trim().ToLower() == svyText.Trim().ToLower()
                         && s.CustCode == CustCode);

                    logger.Debug($"{selectedSurveys.Count()} surveys selected for multi result. (SurveyText: {svyText}, CustCode: {CustCode}");

                    string catName = await db.SurveyCategories
                                .Where(c => c.CatId == catId && c.CustCode == CustCode)
                                .Select(c => c.CatName)
                                .FirstOrDefaultAsync();
                    logger.Debug($"Category name for CatId {catId} of Customer {CustCode}: {catName}");
                    if (catName == null)
                        AddModelError("Category", "Category not found.", ref err);


                    var type = await db.AnswerTypes
                                .Where(a => a.TypeId == typeId && a.BaseId != (int)BaseQuestionTypes.OpenQuestion)
                                .FirstOrDefaultAsync();
                    if (type == null)
                        AddModelError("Answer type", "Answer type does not exist.", ref err);

                    if (err)
                    {
                        logger.Debug($"Loading single result failed due to model validation failure. Exiting action.");
                        return PartialView("_Error", new ErrorModel { Title = "Failed to load survey", Message = errString });
                    }

                    MultiResultModel model;
                    if (await selectedSurveys.CountAsync() == 0)
                    {
                        logger.Debug("Creating empty multi result model.");
                        model = new MultiResultModel
                        {
                            SvyText = "",

                            SurveyDates = new List<DateTime>(),

                            Votes = new List<KeyValuePair<string, List<int>>>()
                        };
                    }
                    else
                    {
                        logger.Debug("Loading data for multi result model");
                        model = new MultiResultModel
                        {

                            SvyText = svyText,

                            SurveyDates = await selectedSurveys.Select(s => s.StartDate).ToListAsync(),

                            Votes = await SelectVotesFromSurveyGrouped(selectedSurveys, type.BaseId)
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

        //[HttpGet]
        //public ActionResult LoadTypesByCategory(int catId)
        //{
        //    try
        //    {
        //        logger.Debug($"Requested to load survey types by category. (CatId: {catId}, CustCode: {CustCode})");
        //        using (var db = new SimpleQDBEntities())
        //        {
        //            if (!db.Customers.Any(c => c.CustCode == CustCode))
        //            {
        //                logger.Warn($"Loading survey types failed. Customer not found: {CustCode}");
        //                return Http.NotFound("Customer not found.");
        //            }

        //            var cat = db.SurveyCategories.Where(c => c.CustCode == CustCode && c.CatId == catId).FirstOrDefault();
        //            if (cat == null)
        //            {
        //                logger.Debug("Category not found: {catId}");
        //                return Http.NotFound("Category not found.");
        //            }

        //            var types = cat.Surveys
        //                .Select(s => new AnswerType
        //                {
        //                    TypeId = s.AnswerType.TypeId,
        //                    TypeDesc = s.AnswerType.TypeDesc,
        //                    BaseId = s.AnswerType.BaseId
        //                })
        //                .Where(t => t.BaseId != (int)BaseQuestionTypes.OpenQuestion)
        //                .GroupBy(t => t.TypeId)
        //                .Select(g => g.First())
        //                .ToList();

        //            logger.Debug($"{types.Count} answer types loaded successfully.");

        //            return Json(types, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "[GET]LoadTypesByCategory: Unexpected error");
        //        return Http.InternalServerError("Something went wrong. Please try again later.");
        //    }
        //}

        //[HttpGet]
        //public ActionResult LoadSurveys(int catId, int typeId)
        //{
        //    try
        //    {
        //        logger.Debug($"Requested to load survey texts. (CatId: {catId}, TypeId: {typeId}, CustCode: {CustCode})");
        //        using (var db = new SimpleQDBEntities())
        //        {
        //            if (!db.Customers.Any(c => c.CustCode == CustCode))
        //            {
        //                logger.Warn($"Loading survey texts failed. Customer not found: {CustCode}");
        //                return Http.NotFound("Customer not found");
        //            }

        //            var query = db.Surveys
        //                .Where(s => s.CatId == catId
        //                    && s.TypeId == typeId
        //                    && s.CustCode == CustCode).ToList()
        //                .Select(s => s.SvyText)
        //                .Distinct();

        //            if (query.Count() == 0)
        //            {
        //                logger.Debug("Loading survey texts failed. No surveys found.");
        //                return Http.NotFound("No surveys found");
        //            }

        //            logger.Debug($"{query.Count()} distinct survey texts loaded successfully.");
        //            return Json(new { SurveyTexts = query.ToList() }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "[GET]LoadSurveys: Unexpected error");
        //        return Http.InternalServerError("Something went wrong. Please try again later.");
        //    }
        //}

        //[HttpGet]
        //public ActionResult LoadSurveyDates(int catId, int typeId, string svyText)
        //{
        //    try
        //    {
        //        logger.Debug($"Requested to load survey dates (CatId: {catId}, TypeId: {typeId}, SvyText: {svyText}, CustCode: {CustCode})");
        //        using (var db = new SimpleQDBEntities())
        //        {
        //            if (!db.Customers.Any(c => c.CustCode == CustCode))
        //            {
        //                logger.Warn($"Loading survey dates failed. Customer not found: {CustCode}");
        //                return Http.NotFound("Customer not found");
        //            }

        //            var query = db.Surveys.Where(s => s.CatId == catId && s.TypeId == typeId && s.CustCode == CustCode && s.SvyText.ToLower().Trim() == svyText.ToLower().Trim());
        //            if (query.Count() == 0)
        //            {
        //                logger.Debug("Loading survey dates failed. No surveys found.");
        //                return Http.NotFound("No surveys found");
        //            }

        //            var startDate = query.Select(s => s.StartDate).Min().ToString("yyyy-MM-dd");
        //            var endDate = query.Select(s => s.StartDate).Max().ToString("yyyy-MM-dd");

        //            logger.Debug($"Survey dates loaded successfully: {startDate} - {endDate}");

        //            return Json(new
        //            {
        //                StartDate = startDate,
        //                EndDate = endDate
        //            }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "[GET]LoadSurveyDates: Unexpected error");
        //        return Http.InternalServerError("Something went wrong. Please try again later.");
        //    }
        //}
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

        private async Task<List<KeyValuePair<string, List<int>>>> SelectVotesFromSurveyGrouped(IQueryable<Survey> selectedSurveys, int baseId)
        {
            try
            {
                logger.Debug("Starting to select votes from survey grouped");
                var list = new List<KeyValuePair<string, List<int>>>();
                var query = await selectedSurveys.SelectMany(s => s.AnswerOptions).ToListAsync();
                logger.Debug($"{query.Count()} answer options selected. (SurveyText: {await selectedSurveys.Select(s => s.SvyText).FirstOrDefaultAsync()}, CustCode: {CustCode}");

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

                await selectedSurveys.ForEachAsync(s =>
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
                logger.Debug($"Error: {key}: {errorMessage}");
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