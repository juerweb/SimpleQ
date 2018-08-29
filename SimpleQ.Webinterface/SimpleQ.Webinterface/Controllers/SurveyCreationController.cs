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
        [HttpGet]
        public ActionResult Index()
        {
            var model = new SurveyCreationModel();
            using (var db = new SimpleQDBEntities())
            {
                model.SurveyCategories = db.SurveyCategories.Where(s => s.CustCode == CustCode).ToList();
                model.AnswerTypes = db.AnswerTypes.ToList();
                model.Groups = db.Groups.Where(g => g.CustCode == CustCode).ToList();
            }
            return View(model: model);
        }

        [HttpPost]
        public ActionResult New(SurveyCreationModel s)
        {
            using (var db = new SimpleQDBEntities())
            {
                db.Surveys.Add(s.Survey);
                s.SelectedGroups.ForEach(g =>
                {
                    db.Askings.Add(new Asking { SvyId = s.Survey.SvyId, GroupId = g.GroupId, CustCode = CustCode });
                });
                db.SaveChanges();
            }

            HostingEnvironment.QueueBackgroundWorkItem(ct =>
            {
                Thread.Sleep(s.Survey.StartDate - DateTime.Now);

                s.SelectedGroups.ForEach(g =>
                {
                    SimpleQHub.SendSurvey(g.GroupId, CustCode, s.Survey);
                });
            });

            return RedirectToAction("Index");
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