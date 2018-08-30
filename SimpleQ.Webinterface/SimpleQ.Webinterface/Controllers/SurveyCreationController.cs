using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SimpleQ.Webinterface.Mobile;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;

namespace SimpleQ.Webinterface.Controllers
{
    public class SurveyCreationController : Controller
    {
        [HttpPost]
        public ActionResult New(SurveyCreationModel s)
        {
            using (var db = new SimpleQDBEntities())
            {
                s.Survey.CustCode = CustCode;
                s.Survey.EndDate = s.Survey.StartDate.AddDays(7);
                db.Surveys.Add(s.Survey);
                s.SelectedGroups.ForEach(g =>
                {
                    db.Askings.Add(new Asking { SvyId = s.Survey.SvyId, GroupId = g, CustCode = CustCode });
                });
                db.SaveChanges();
            }

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                Thread.Sleep(s.Survey.StartDate - DateTime.Now);

                s.SelectedGroups.ForEach(g =>
                {
                    SimpleQHub.SendSurvey(g, CustCode, s.Survey);
                });
            });

            return RedirectToAction("Index", "Home");
        }


        private string CustCode
        {
            get
            {
                return Session["custCode"] as string;
            }
        }
    }
}