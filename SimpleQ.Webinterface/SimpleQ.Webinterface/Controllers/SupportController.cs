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
        public ActionResult Load()
        {
            using (var db = new SimpleQDBEntities())
            {
                var model = new SupportModel
                {
                    FaqEntries = db.FaqEntries.ToList()
                };

                return View("Support", model: model);
            }
        }

        [HttpPost]
        public ActionResult AskQuestion(SupportModel req)
        {
            //req = new SupportModel();
            //req.Email = "test@kontaktfunktion.gabi";
            //req.QuestionCatgeory = "Lelelele";
            //req.QuestionText = "Sads ma BITTE NED BES";


            if (req == null)
                return Http.BadRequest("Model object must not be null.");

            if (string.IsNullOrEmpty(req.Email))
                return Http.BadRequest("Email must not be null.");

            if (string.IsNullOrEmpty(req.QuestionText))
                return Http.BadRequest("QuestionText must not be null.");

            if (string.IsNullOrEmpty(req.QuestionCatgeory))
                return Http.BadRequest("QuestionCategory must not be null.");


            req.QuestionText = $"FROM: {req.Email}{Environment.NewLine}{req.QuestionText}";

            try
            {
                MailMessage msg = new MailMessage("contactform@simpleq.at", "support@simpleq.at")
                {
                    Subject = "SIMPLEQ SUPPORT: " + req.QuestionCatgeory,
                    Body = req.QuestionText
                };
                SmtpClient client = new SmtpClient("smtp.1und1.de", 587)
                {
                    Credentials = new System.Net.NetworkCredential("contactform@simpleq.at", "123SimpleQ..."),
                    EnableSsl = true
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
    }
}