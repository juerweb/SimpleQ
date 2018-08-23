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

namespace SimpleQ.Webinterface.Mobile
{
    public class SimpleQHub : Hub
    {
        private static Dictionary<string, AskedPerson> connectedPersons = new Dictionary<string, AskedPerson>();
        private static IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SimpleQHub>();

        #region Invoked by app
        public OperationStatus Register(AskedPerson person)
        {
            using (var db = new SimpleQDBEntities())
            {
                try
                {
                    db.AskedPersons.Add(person);
                    db.SaveChanges();

                    return new OperationStatus(StatusCode.REGISTERED, "Registered");
                }
                catch (SqlException ex)
                {
                    string msg = ex.Number == 2627 || ex.Number == 2601 ? "Already registered"
                        : ex.Number == 547 ? "No such company"
                        : ex.Number == 8152 ? "Maximum input length exceeded"
                        : "Unknown registration error";


                    return new OperationStatus(StatusCode.REGISTRATION_FAILED, msg);
                }
            }
        }

        public OperationStatus Login(string persEmail, string persPwd, string custName)
        {
            if (LoggedIn(persEmail)) return new OperationStatus(StatusCode.LOGIN_FAILED, "Already logged in");
            using (var db = new SimpleQDBEntities())
            {
                AskedPerson person = db.AskedPersons.Where(p => p.PersEmail == persEmail && p.PersPwdHash == persPwd.GetSHA512() && p.CustName == custName).FirstOrDefault();
                if (person != null)
                {
                    connectedPersons.Add(Context.ConnectionId, person);
                    return new OperationStatus(StatusCode.LOGGED_IN, "Logged in");
                }
                else
                    return new OperationStatus(StatusCode.LOGIN_FAILED, "Invalid credentials");
            }

        }

        public void ChangeData(object data)
        {

        }

        public void AnswerSurvey(object answer)
        {

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
        internal static void SendSurvey(int groupId, string custName, object svyData)
        {
            using (var db = new SimpleQDBEntities())
            {
                List<string> connIDs = new List<string>();
                db.Contains.Where(c => c.GroupId == groupId && c.CustName == custName).ToList().ForEach(c =>
                {
                    c.Department.AskedPersons.TakeRandom(c.Amount).ToList().ForEach(p => 
                    {
                        string connId = connectedPersons.Where(kv => kv.Value.PersEmail == p.PersEmail).Select(kv => kv.Key).FirstOrDefault();
                        if (connId != null)
                            connIDs.Add(connId);
                    });
                });
                hubContext.Clients.Clients(connIDs).SendSurvey(svyData);
            }
        }

        internal static void ConfigChanged(object cfg)
        {

        }
        #endregion

        private void SendStatus(string client, OperationStatus status)
        {
            Clients.Client(client).ReceiveStatus(status);
        }

        private bool LoggedIn(string email)
        {
            return connectedPersons.Values.ToList().Exists(p => p.PersEmail.ToLower() == email.ToLower()) || connectedPersons.Keys.ToList().Exists(k => k == Context.ConnectionId);
        }
    }
}