using SimpleQ.Webinterface.Extensions;
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

            using (var db = new SimpleQDBEntities())
            {
                try
                {
                    string custCode = regCode.Substring(0, 6);
                    int depId = int.Parse(regCode.Substring(6));

                    Person person = new Person { DepId = depId, CustCode = custCode, DeviceId = deviceId};
                    db.People.Add(person);
                    db.SaveChanges();
                    Department dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == custCode).First();

                    return Ok(new RegistrationData { CustCode = custCode, PersId = person.PersId, DepId = depId, DepName = dep.DepName });
                }
                catch (DbUpdateException ex) when ((ex?.InnerException?.InnerException as SqlException)?.Number == 2627 || (ex?.InnerException?.InnerException as SqlException)?.Number == 2601)
                {
                    return Conflict();
                }
                catch (DbUpdateException ex) when ((ex?.InnerException?.InnerException as SqlException)?.Number == 547)
                {
                    return NotFound();
                }
                catch (Exception ex) when (ex is FormatException || ex is ArgumentOutOfRangeException)
                {
                    return NotFound();
                }
            }
        }

        [HttpGet]
        public IHttpActionResult Unregister(int persId, string custCode)
        {
            if (!IsAuth(Request))
                return Unauthorized();

            using (var db = new SimpleQDBEntities())
            {
                db.People.RemoveRange(db.People.Where(p => p.PersId == persId && p.CustCode == custCode));
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

                if (svy != null)
                    return Ok(new SurveyNotification
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
                else
                    return NotFound();
            }
        }

        [HttpPost]
        public IHttpActionResult AnswerSurvey([FromBody] SurveyVote sv)
        {
            if (!IsAuth(Request))
                return Unauthorized();

            using (var db = new SimpleQDBEntities())
            {
                if (!db.Customers.Any(c => c.CustCode == sv.CustCode))
                    return NotFound();

                Vote vote = new Vote { VoteText = sv.VoteText };
                db.Votes.Add(vote);
                db.SaveChanges();

                sv.ChosenAnswerOptions.Select(a => a.AnsId).ToList().ForEach(id =>
                {
                    vote.AnswerOptions.Add(db.AnswerOptions.Where(a => a.AnsId == id).First());
                });

                db.Customers.Where(c => c.CustCode == sv.CustCode).First().CostBalance += db.Customers.Where(c => c.CustCode == sv.CustCode).First().PricePerClick; //decimal.Parse(ConfigurationManager.AppSettings["SurveyCost"], System.Globalization.CultureInfo.InvariantCulture);
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
