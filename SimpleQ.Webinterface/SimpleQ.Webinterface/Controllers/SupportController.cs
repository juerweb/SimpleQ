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
    [CAuthorize]
    public class SupportController : Controller
    {
        #region MVC-Actions
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new SimpleQDBEntities())
            {
                var model = new SupportModel
                {
                    FaqEntries = db.FaqEntries.ToList()
                };

                return View("Support", model);
            }
        }

        [HttpPost]
        public ActionResult AskQuestion(SupportModel req)
        {
            //req = new SupportModel();
            //req.Email = "test@kontaktfunktion.gabi";
            //req.QuestionCatgeory = "Lelelele";
            //req.QuestionText = "Sads ma BITTE NED BES";

            bool err = false;

            if (req == null)
                AddModelError("Model", "Model object must not be null.", ref err);

            if (string.IsNullOrEmpty(req.Email))
                AddModelError("Email", "Email must not be null.", ref err);

            if (string.IsNullOrEmpty(req.QuestionText))
                AddModelError("QuestionText", "QuestionText must not be null.", ref err);

            if (string.IsNullOrEmpty(req.QuestionCatgeory))
                AddModelError("QuestionCatgeory", "QuestionCatgeory must not be null.", ref err);

            if (err) return Index();


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

                return Index();
            }
            catch (Exception ex) when (ex is SmtpException || ex is SmtpFailedRecipientsException)
            {
                return Http.ServiceUnavailable("Sending failed due to internal error(s).");
            }
        }
        #endregion

        #region Helpers
        private void AddModelError(string key, string errorMessage, ref bool error)
        {
            ModelState.AddModelError(key, errorMessage);
            error = true;
        }
        #endregion
    }
}