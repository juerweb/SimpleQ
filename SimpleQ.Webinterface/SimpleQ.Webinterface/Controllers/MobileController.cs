using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.Mobile;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;

namespace SimpleQ.Webinterface.Controllers
{
    public class MobileController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Register(string regCode, string deviceId)
        {
            if (!IsAuth(Request))
                return Unauthorized();

            if (string.IsNullOrEmpty(regCode))
                return BadRequest();

            using (var db = new SimpleQDBEntities())
            {
                try
                {
                    string custCode = regCode.Substring(0, 6);
                    int depId = int.Parse(regCode.Substring(6));

                    var cust = db.Customers.Where(c => c.CustCode == custCode).FirstOrDefault();
                    if (cust == null)
                        return NotFound();

                    var dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == custCode).FirstOrDefault();
                    if (dep == null)
                        return NotFound();

                    var person = new Person { DeviceId = deviceId };
                    db.People.Add(person);
                    db.SaveChanges();
                    dep.People.Add(person);
                    db.SaveChanges();

                    return Ok(new RegistrationData { CustCode = custCode, PersId = person.PersId, DepId = depId, DepName = dep.DepName, CustName = cust.CustName });
                }
                catch (Exception ex) when (ex is FormatException || ex is ArgumentOutOfRangeException)
                {
                    return BadRequest();
                }
            }
        }

        [HttpGet]
        public IHttpActionResult JoinDepartment(string regCode, int persId)
        {
            if (!IsAuth(Request))
                return Unauthorized();

            if (string.IsNullOrEmpty(regCode))
                return BadRequest();

            using (var db = new SimpleQDBEntities())
            {
                try
                {
                    string custCode = regCode.Substring(0, 6);
                    int depId = int.Parse(regCode.Substring(6));

                    var cust = db.Customers.Where(c => c.CustCode == custCode).FirstOrDefault();
                    if (cust == null)
                        return NotFound();

                    var dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == custCode).FirstOrDefault();
                    if (dep == null)
                        return NotFound();

                    var person = db.People.Where(p => p.PersId == persId).FirstOrDefault();
                    if (person == null)
                        return NotFound();

                    dep.People.Add(person);
                    db.SaveChanges();

                    return Ok(new RegistrationData { CustCode = custCode, PersId = person.PersId, DepId = depId, DepName = dep.DepName, CustName = cust.CustName });

                }
                catch (Exception ex) when (ex is FormatException || ex is ArgumentOutOfRangeException)
                {
                    return BadRequest();
                }
            }
        }

        [HttpGet]
        public IHttpActionResult LeaveDepartment(int persId, int depId, string custCode)
        {
            if (!IsAuth(Request))
                return Unauthorized();

            if (string.IsNullOrEmpty(custCode))
                return BadRequest();

            using (var db = new SimpleQDBEntities())
            {

                var dep = db.Departments.Where(d => d.CustCode == custCode && d.DepId == depId).FirstOrDefault();
                if (dep == null)
                    return NotFound();

                var pers = db.People.Where(p => p.PersId == persId).FirstOrDefault();
                if (pers == null)
                    return NotFound();

                dep.People.Remove(pers);

                db.SaveChanges();
                return Ok();
            }
        }

        [HttpGet]
        public IHttpActionResult Unregister(int persId, string custCode)
        {
            if (!IsAuth(Request))
                return Unauthorized();

            if (string.IsNullOrEmpty(custCode))
                return BadRequest();

            using (var db = new SimpleQDBEntities())
            {
                var query = db.Departments.Where(d => d.CustCode == custCode);
                if (query.Count() == 0)
                    return NotFound();

                var pers = db.People.Where(p => p.PersId == persId).FirstOrDefault();
                if (pers == null)
                    return NotFound();

                query.ToList().ForEach(d =>
                {
                    d.People.Remove(pers);
                });
                db.People.Remove(pers);
                db.SaveChanges();

                return Ok();
            }
        }

        [HttpGet]
        public IHttpActionResult GetSurveyData(int svyId)
        {
            if (!IsAuth(Request))
                return Unauthorized();

            using (var db = new SimpleQDBEntities())
            {
                Survey svy = db.Surveys.Where(s => s.SvyId == svyId).FirstOrDefault();
                if (svy == null)
                    return NotFound();

                return Ok(new SurveyData
                {
                    SvyId = svy.SvyId,
                    SvyText = svy.SvyText,
                    EndDate = svy.EndDate,
                    TypeId = svy.TypeId,
                    CatName = db.SurveyCategories
                        .Where(c => c.CatId == svy.CatId)
                        .Select(c => c.CatName)
                        .First(),
                    AnswerOptions = db.AnswerOptions
                        .Where(a => a.SvyId == svy.SvyId)
                        .ToList()
                });
            }
        }

        [HttpPost]
        public IHttpActionResult AnswerSurvey([FromBody] SurveyVote sv)
        {
            if (!IsAuth(Request))
                return Unauthorized();

            if (sv == null)
                return BadRequest();

            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == sv.CustCode).FirstOrDefault();
                if (cust == null)
                    return NotFound();

                Vote vote = new Vote { VoteText = sv.VoteText };
                db.Votes.Add(vote);
                db.SaveChanges();

                if (sv.ChosenAnswerOptions.Select(a => a.SvyId).Distinct().Count() != 1)
                    return Conflict();

                foreach (int ansId in sv.ChosenAnswerOptions.Select(a => a.AnsId))
                {
                    if (db.AnswerOptions.Where(a => a.AnsId == ansId).FirstOrDefault() == null)
                        return NotFound();
                }

                sv.ChosenAnswerOptions.Select(a => a.AnsId).ToList().ForEach(id =>
                {
                    vote.AnswerOptions.Add(db.AnswerOptions.Where(a => a.AnsId == id).FirstOrDefault());
                });

                cust.CostBalance += cust.PricePerClick; //decimal.Parse(ConfigurationManager.AppSettings["SurveyCost"], System.Globalization.CultureInfo.InvariantCulture);
                db.SaveChanges();

                return Ok();
            }
        }

        private bool IsAuth(HttpRequestMessage request)
        {
            string privateKey = File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\private.key"));
            bool headerExists = request.Headers.TryGetValues("auth-key", out IEnumerable<string> values);

            return headerExists && values?.FirstOrDefault() as string == privateKey;
        }
    }
}
