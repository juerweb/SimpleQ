using SimpleQ.Webinterface.Extensions;
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
            //req = new SupportModel();
            //req.Email = "test@kontaktfunktion.gabi";
            //req.QuestionCatgeory = "Lelelele";
            //req.QuestionText = "Es letzte vom letzn san de hawara vo da gis.";

            if (req == null)
                return Http.BadRequest("Model object must not be null.");

            try
            {
                MailMessage msg = new MailMessage(req.Email ?? throw ANex("Email"), "support@simpleq.at")
                {
                    Subject = "SIMPLEQ SUPPORT: " + req.QuestionCatgeory ?? throw ANex("QuestionCategory"),
                    Body = req.QuestionText ?? throw ANex("QuestionText")
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
            catch (ArgumentNullException ex)
            {
                return Http.BadRequest($"{ex.ParamName} must not be null.");
            }
            catch (Exception ex) when (ex is SmtpException || ex is SmtpFailedRecipientsException)
            {
                return Http.ServiceUnavailable("Sending failed due to internal error(s).");
            }
        }

        private ArgumentNullException ANex(string paramName) => new ArgumentNullException(paramName);
    }
}