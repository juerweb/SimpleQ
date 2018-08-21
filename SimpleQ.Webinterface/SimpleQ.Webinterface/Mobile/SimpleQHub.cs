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

namespace SimpleQ.Webinterface.Mobile
{
    public class SimpleQHub : Hub
    {
        private static Dictionary<string, AskedPerson> connectedPersons = new Dictionary<string, AskedPerson>();
        private static IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SimpleQHub>();

        #region Invoked by app
        public void Register(object usr)
        {

        }

        public bool Login(string persEmail, string persPwd, string custName)
        {
            using (var db = new SimpleQDBEntities())
            {
                AskedPerson person = db.AskedPersons.Where(p => p.PersEmail == persEmail && p.PersPwdHash == SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(persPwd)) && p.CustName == custName).FirstOrDefault();
                if (person != null)
                {
                    connectedPersons.Add(Context.ConnectionId, person);
                    return true;
                }
                else
                    return false;
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
                hubContext.Clients.Clients(connIDs).DUMMY_SENDSURVEY(svyData);
            }
        }

        internal static void ConfigChanged(object cfg)
        {

        }
        #endregion
    }
}