using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class SupportController : Controller
    {
        [HttpGet]
        public ActionResult LoadFaqEntries()
        {
            using (var db = new SimpleQDBEntities())
            {
                var model = new SupportModel
                {
                    FaqEntries = db.FaqEntries.ToList()
                };

                return PartialView("_Support", model: model);
            }
        }

        [HttpPost]
        public ActionResult AskQuestion(SupportModel req)
        {
            //SupportModel req = new SupportModel();
            //req.Email = "test@kontaktfunktion.gabi";
            //req.QuestionCatgeory = "Lelelele";
            //req.QuestionText = "Es letzte vom letzn san de hawara vo da gis.";

            MailMessage msg = new MailMessage(req.Email, "support@simpleq.at")
            {
                Subject = "SIMPLEQ SUPPORT: " + req.QuestionCatgeory,
                Body = req.QuestionText
            };
            SmtpClient client = new SmtpClient
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.1und1.de"
            };
            client.Send(msg);

            return LoadFaqEntries();
        }
    }
}