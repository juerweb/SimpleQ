using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.Mobile;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SimpleQ.Webinterface.Controllers
{
    public class MobileController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Register(string regCode, string deviceId)
        {
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

                    return Request.CreateResponse(HttpStatusCode.OK, new RegistrationData { CustCode = custCode, PersId = person.PersId, DepId = depId, DepName = dep.DepName });
                }
                catch (DbUpdateException ex) when ((ex?.InnerException?.InnerException as SqlException)?.Number == 2627 || (ex?.InnerException?.InnerException as SqlException)?.Number == 2601)
                {
                    return Request.CreateResponse(HttpStatusCode.MethodNotAllowed, "Already registered");
                }
                catch (DbUpdateException ex) when ((ex?.InnerException?.InnerException as SqlException)?.Number == 547)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                catch (Exception ex) when (ex is FormatException || ex is ArgumentOutOfRangeException)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
        }

        [HttpGet]
        public HttpResponseMessage Unregister(int persId, string custCode)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.People.RemoveRange(db.People.Where(p => p.PersId == persId && p.CustCode == custCode));
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetSurveyData(int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                Survey svy = db.Surveys.Where(s => s.SvyId == svyId).FirstOrDefault();

                return (svy != null) ?
                    Request.CreateResponse(HttpStatusCode.OK, new SurveyNotification
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
                        })
                    : Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        public HttpResponseMessage AnswerSurvey([FromBody] SurveyVote sv)
        {
            using (var db = new SimpleQDBEntities())
            {
                Vote vote = new Vote { VoteText = sv.VoteText };
                db.Votes.Add(vote);
                db.SaveChanges();

                sv.ChosenAnswerOptions.Select(a => a.AnsId).ToList().ForEach(id =>
                {
                    vote.AnswerOptions.Add(db.AnswerOptions.Where(a => a.AnsId == id).First());
                });

                db.Customers.Where(c => c.CustCode == sv.CustCode).First().CostBalance += db.Customers.Where(c => c.CustCode == sv.CustCode).First().PricePerClick; //decimal.Parse(ConfigurationManager.AppSettings["SurveyCost"], System.Globalization.CultureInfo.InvariantCulture);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }


        internal static void SendSurveyNotification(int depId, int amount, int svyId)
        {
            using (var db = new SimpleQDBEntities())
            {
                Random rnd = new Random();
                //int i = 0;
                db.Departments
                    .Where(d => d.DepId == depId)
                    .SelectMany(d => d.People)
                    .ToList()
                    .OrderBy(p => rnd.Next())
                    .Take(amount)
                    .ToList()
                    .ForEach(p =>
                    {
                        //i++;  
                    });
                //System.Diagnostics.Debug.WriteLine($"(SvyId {svyId}) SURVEYS SENT: {i} == {amount}");
            }
        }
    }
}
