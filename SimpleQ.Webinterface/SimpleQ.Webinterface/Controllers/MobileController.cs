using NLog;
using SimpleQ.Webinterface.Attributes;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.Mobile;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace SimpleQ.Webinterface.Controllers
{
    [RequireAuth]
    public class MobileController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public async Task<IHttpActionResult> Register(string regCode, string deviceId)
        {
            try
            {
                logger.Debug($"Mobile registration requested (RegCode: {regCode}, DeviceId: {deviceId})");
                if (string.IsNullOrEmpty(regCode))
                {
                    logger.Debug($"Mobile registration failed. RegCode was null or empty");
                    return BadRequest();
                }

                using (var db = new SimpleQDBEntities())
                {
                    try
                    {
                        string custCode = regCode.Substring(0, 6);
                        int depId = int.Parse(regCode.Substring(6));

                        var cust = await db.Customers.Where(c => c.CustCode == custCode).FirstOrDefaultAsync();
                        if (cust == null)
                        {
                            logger.Debug($"Mobile registration failed. Invalid RegCode. Customer not found {custCode}");
                            return NotFound();
                        }

                        var dep = await db.Departments.Where(d => d.DepId == depId && d.CustCode == custCode).FirstOrDefaultAsync();
                        if (dep == null)
                        {
                            logger.Debug($"Mobile registration failed. Invalid RegCode. Department not found {depId}");
                            return NotFound();
                        }

                        var person = new Person { DeviceId = deviceId };
                        //db.People.Add(person);
                        //db.SaveChanges();
                        dep.People.Add(person);
                        await db.SaveChangesAsync();
                        logger.Debug("Mobile registration successful");

                        return Ok(new RegistrationData { CustCode = custCode, PersId = person.PersId, DepId = depId, DepName = dep.DepName, CustName = cust.CustName, AuthToken = person.AuthToken });
                    }
                    catch (Exception ex) when (ex is FormatException || ex is ArgumentOutOfRangeException)
                    {
                        logger.Debug("Mobile registration failed. Invalidly formatted RegCode");
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]Register: Unexpected error");
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> JoinDepartment(string regCode, int persId)
        {
            try
            {
                logger.Debug($"Join department requested. (RegCode: {regCode}, PersId: {persId})");
                if (string.IsNullOrEmpty(regCode))
                {
                    logger.Debug($"Joining failed. RegCode was null or empty");
                    return BadRequest();
                }

                using (var db = new SimpleQDBEntities())
                {
                    try
                    {
                        string custCode = regCode.Substring(0, 6);
                        int depId = int.Parse(regCode.Substring(6));

                        var cust = await db.Customers.Where(c => c.CustCode == custCode).FirstOrDefaultAsync();
                        if (cust == null)
                        {
                            logger.Debug($"Joining failed. Invalid RegCode. Customer not found {custCode}");
                            return NotFound();
                        }

                        var dep = await db.Departments.Where(d => d.DepId == depId && d.CustCode == custCode).FirstOrDefaultAsync();
                        if (dep == null)
                        {
                            logger.Debug($"Joining failed. Invalid RegCode. Department not found {depId}");
                            return NotFound();
                        }

                        var person = await db.People.Where(p => p.PersId == persId).FirstOrDefaultAsync();
                        if (person == null)
                        {
                            logger.Debug($"Joining failed. Person not found {persId}");
                            return NotFound();
                        }

                        dep.People.Add(person);
                        await db.SaveChangesAsync();
                        logger.Debug("Joined successfully");

                        return Ok(new RegistrationData { CustCode = custCode, PersId = person.PersId, DepId = depId, DepName = dep.DepName, CustName = cust.CustName });

                    }
                    catch (Exception ex) when (ex is FormatException || ex is ArgumentOutOfRangeException)
                    {
                        logger.Debug("Joining failed. Invalidly formatted RegCode");
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]JoinDepartment: Unexpected error");
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> LeaveDepartment(int persId, int depId, string custCode)
        {
            try
            {
                logger.Debug($"Leave department requested. (PersId: {persId}, DepId: {depId}, CustCode: {custCode})");
                if (string.IsNullOrEmpty(custCode))
                {
                    logger.Debug($"Leaving failed. CustCode was null or empty");
                    return BadRequest();
                }

                using (var db = new SimpleQDBEntities())
                {
                    var dep = await db.Departments.Where(d => d.CustCode == custCode && d.DepId == depId).FirstOrDefaultAsync();
                    if (dep == null)
                    {
                        logger.Debug($"Leaving failed. Department not found {depId}");
                        return NotFound();
                    }

                    var pers = await db.People.Where(p => p.PersId == persId).FirstOrDefaultAsync();
                    if (pers == null)
                    {
                        logger.Debug($"Leaving failed. Person not found {persId}");
                        return NotFound();
                    }

                    dep.People.Remove(pers);
                    await db.SaveChangesAsync();

                    logger.Debug("Left successfully");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]LeaveDepartment: Unexpected error");
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> Unregister(int persId, string custCode)
        {
            try
            {
                logger.Debug($"Unregister requested. (PersId: {persId}, CustCode: {custCode})");
                if (string.IsNullOrEmpty(custCode))
                {
                    logger.Debug($"Unregister failed. CustCode was null or empty");
                    return BadRequest();
                }

                using (var db = new SimpleQDBEntities())
                {
                    var query = db.Departments.Where(d => d.CustCode == custCode);
                    if (await query.CountAsync() == 0)
                    {
                        logger.Debug($"Unregister failed. Customer not found {custCode}");
                        return NotFound();
                    }

                    var pers = await db.People.Where(p => p.PersId == persId).FirstOrDefaultAsync();
                    if (pers == null)
                    {
                        logger.Debug($"Unregister failed. Person not found {persId}");
                        return NotFound();
                    }

                    await query.ForEachAsync(d =>
                    {
                        d.People.Remove(pers);
                    });
                    db.People.Remove(pers);
                    await db.SaveChangesAsync();

                    logger.Debug("Unregistered successfully");

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]Unregister: Unexpected error");
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetSurveyData(int svyId)
        {
            try
            {
                logger.Debug($"Get survey data requested. (SvyId: {svyId})");
                using (var db = new SimpleQDBEntities())
                {
                    Survey svy = await db.Surveys.Where(s => s.SvyId == svyId).FirstOrDefaultAsync();
                    if (svy == null)
                    {
                        logger.Debug($"Loading survey data failed. Survey not found. {svyId}");
                        return NotFound();
                    }

                    if (svy.EndDate < DateTime.Now)
                    {
                        logger.Debug($"Loading survey data failed. Survey exceeded. {svyId}");
                        return Conflict();
                    }

                    var catName = svy.SurveyCategory.CatName;
                    var answerOptions = svy.AnswerOptions
                        .Select(a => new AnswerOption
                        {
                            AnsId = a.AnsId,
                            AnsText = a.AnsText,
                            SvyId = a.SvyId,
                            FirstPosition = a.FirstPosition
                        })
                        .ToList();

                    logger.Debug("Survey data loaded successfully");
                    return Ok(new SurveyData
                    {
                        SvyId = svy.SvyId,
                        SvyText = svy.SvyText,
                        EndDate = svy.EndDate,
                        TypeId = svy.TypeId,
                        CatName = catName,
                        AnswerOptions = answerOptions
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]GetSurveyData: Unexpected error");
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> LoadFaqEntries()
        {
            try
            {
                using (var db = new SimpleQDBEntities())
                {
                    var query = await db.FaqEntries.Where(f => f.IsMobile).ToListAsync();

                    return Ok(query);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]LoadFaqEntries: Unexpected error");
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> AnswerSurvey([FromBody] SurveyVote sv)
        {
            try
            {
                logger.Debug("Answer survey requested");
                if (sv == null)
                {
                    logger.Debug("Answering failed. Survey vote was null");
                    return BadRequest();
                }

                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.CustCode == sv.CustCode).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Debug($"Answering failed. Customer not found {sv.CustCode}");
                        return NotFound();
                    }

                    Vote vote = new Vote { VoteText = sv.VoteText };
                    db.Votes.Add(vote);

                    var ansIds = sv.ChosenAnswerOptions.Select(a => a.AnsId).ToList();
                    if (await db.AnswerOptions.Where(a => ansIds.Contains(a.AnsId)).Select(a => a.SvyId).Distinct().CountAsync() != 1)
                    {
                        logger.Debug("Answering failed. 1 Answer is only possible for 1 survey");
                        return Conflict();
                    }

                    foreach (int ansId in sv.ChosenAnswerOptions.Select(a => a.AnsId))
                    {
                        if (await db.AnswerOptions.Where(a => a.AnsId == ansId).FirstOrDefaultAsync() == null)
                        {
                            logger.Debug($"Answering failed. AnswerOption not found {ansId}");
                            return NotFound();
                        }
                    }

                    await db.SaveChangesAsync();

                    sv.ChosenAnswerOptions.Select(a => a.AnsId).ToList().ForEach(id =>
                    {
                        vote.AnswerOptions.Add(db.AnswerOptions.Where(a => a.AnsId == id).FirstOrDefault());
                    });

                    await db.SaveChangesAsync();
                    logger.Debug("Answered survey successfully");

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[POST]AnswerSurvey: Unexpected error");
                return InternalServerError(ex);
            }
        }
    }
}
