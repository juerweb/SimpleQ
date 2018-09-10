using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Extensions;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using System.Configuration;

namespace SimpleQ.Webinterface.Mobile
{
    public class SimpleQHub : Hub
    {
        private static Dictionary<string, AskedPerson> connectedPersons = new Dictionary<string, AskedPerson>();
        private static IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SimpleQHub>();

        #region Invoked by app
        public OperationStatus Register(string regCode)
        {
            using (var db = new SimpleQDBEntities())
            {
                try
                {
                    string custCode = regCode.Substring(0, 6);
                    int depId = int.Parse(regCode.Substring(6));

                    AskedPerson person = new AskedPerson { CustCode = custCode, DepId = depId };
                    db.AskedPersons.Add(person);
                    db.SaveChanges();

                    connectedPersons.Add(Context.ConnectionId, person);
                    Department dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == custCode).First();

                    return new OperationStatus(StatusCode.REGISTERED, "Registered") { AssignedDepartment = dep, PersId = person.PersId };
                }
                catch (DbUpdateException ex) when ((ex?.InnerException?.InnerException as SqlException)?.Number == 2627 || (ex?.InnerException?.InnerException as SqlException)?.Number == 2601)
                {
                    return new OperationStatus(StatusCode.REGISTRATION_FAILED_ALREADY_REGISTERED, "Already registered");
                }
                catch (DbUpdateException ex) when ((ex?.InnerException?.InnerException as SqlException)?.Number == 547)
                {
                    return new OperationStatus(StatusCode.REGISTRATION_FAILED_INVALID_CODE, "Invalid registration code");
                }
                catch (FormatException)
                {
                    return new OperationStatus(StatusCode.REGISTRATION_FAILED_INVALID_CODE, "Invalid registration code");
                }


            }
        }

        public void Unregister(int persId, string custCode)
        {
            using (var db = new SimpleQDBEntities())
            {
                connectedPersons.Remove(Context.ConnectionId);
                db.AskedPersons.RemoveRange(db.AskedPersons.Where(p => p.PersId == persId && p.CustCode == custCode));
                db.SaveChanges();
            }
        }

        public OperationStatus Login(int persId, string custCode)
        {
            if (LoggedIn(persId, custCode)) return new OperationStatus(StatusCode.LOGGED_IN, "Already logged in");
            using (var db = new SimpleQDBEntities())
            {
                AskedPerson person = db.AskedPersons.Where(p => p.PersId == persId && p.CustCode == custCode).FirstOrDefault();
                if (person != null)
                {
                    connectedPersons.Add(Context.ConnectionId, person);
                    return new OperationStatus(StatusCode.LOGGED_IN, "Logged in");
                }
                else
                    return new OperationStatus(StatusCode.LOGIN_FAILED_NOT_REGISTERED, $"No person registered with ID: {persId}, CustCode: {custCode}");
            }
        }

        public void Logout()
        {
            connectedPersons.Remove(Context.ConnectionId);
        }

        public Answer[] LoadAnswersOfType(int typeId)
        {
            using (var db = new SimpleQDBEntities())
            {
                return db.Answers.Where(a => a.TypeId == typeId).ToArray();
            }
        }

        public SpecifiedTextAnswer[] LoadSpecifiedTextAnswers(int svyId, string custCode)
        {
            using (var db = new SimpleQDBEntities())
            {
                return db.SpecifiedTextAnswers.Where(s => s.SvyId == svyId && s.CustCode == custCode).ToArray();
            }
        }

        public void AnswerSurvey(Vote vote)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Votes.Add(vote);
                db.Customers.Where(c => c.CustCode == vote.CustCode).First().CostBalance += decimal.Parse(ConfigurationManager.AppSettings["SurveyCost"], System.Globalization.CultureInfo.InvariantCulture);
                db.SaveChanges();
            }
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            connectedPersons.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
        #endregion


        #region Invoked by server
        internal static void SendSurvey(int depId, int amount, string custCode, Survey survey)
        {
            using (var db = new SimpleQDBEntities())
            {
                List<AskedPerson> list = db.Departments
                    .Where(d => d.DepId == depId && d.CustCode == custCode)
                    .SelectMany(d => d.AskedPersons)
                    .TakeRandom(amount)
                    .ToList();

                hubContext.Clients.Clients(connectedPersons
                    .Where(kvp => list.Exists(p => p.PersId == kvp.Value.PersId))
                    .Select(kvp => kvp.Key)
                    .ToList()).SendSurvey(survey);
            }
        }

        internal static void AnonymityChanged(int configValue)
        {

        }
        #endregion

        private void SendStatus(string client, OperationStatus status)
        {
            Clients.Client(client).ReceiveStatus(status);
        }

        private bool LoggedIn(int persId, string custCode)
        {
            return connectedPersons.Values.ToList().Exists(p => p.PersId == persId && p.CustCode == custCode) || connectedPersons.Keys.ToList().Exists(k => k == Context.ConnectionId);
        }
    }
}