using NLog;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Properties;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class SupportController : BaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                logger.Debug($"Loading support page requested: {CustCode}");
                using (var db = new SimpleQDBEntities())
                {
                    var lang = (string)RouteData.Values["language"] ?? "de";
                    var model = new SupportModel
                    {
                        FaqEntries = await db.FaqEntries.Where(f => !f.IsMobile && f.LangCode == lang).ToListAsync()
                    };

                    ViewBag.emailConfirmed = (await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync()).EmailConfirmed;
                    logger.Debug("Support page loaded successfully");

                    return View("Support", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[GET]Index: Unexpected error");
                return View("Error", model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> AskQuestion(SupportModel req)
        {
            //req = new SupportModel();
            //req.Email = "test@kontaktfunktion.gabi";
            //req.QuestionCatgeory = "Lelelele";
            //req.QuestionText = "Sads ma BITTE NED BES";

            try
            {
                logger.Debug($"AskQuestion requested {CustCode}");
                bool err = false;

                if (req == null)
                    AddModelError("Model", BackendResources.ModelNull, ref err);

                if (string.IsNullOrEmpty(req.Email))
                    AddModelError("Email", BackendResources.EmailEmpty, ref err);

                if (string.IsNullOrEmpty(req.QuestionText))
                    AddModelError("QuestionText", BackendResources.QuestionTextEmpty, ref err);

                if (string.IsNullOrEmpty(req.QuestionCatgeory))
                    AddModelError("QuestionCatgeory", BackendResources.QuestionCategoryEmpty, ref err);

                if (err)
                {
                    logger.Debug("AskQuestion validation failed. Exiting method");
                    return await Index();
                }


                req.QuestionText = $"FROM: {req.Email}{Environment.NewLine}{req.QuestionText}";


                if (await Email.Send("contactform@simpleq.at", "support@simpleq.at", "SIMPLEQ SUPPORT: " + req.QuestionCatgeory, req.QuestionText))
                {
                    logger.Debug("Support e-mail sent successfully");
                    return RedirectToAction("Index", "Support");
                }
                else
                {
                    logger.Error($"Support e-mail sending failed for: {CustCode}");
                    var model = new ErrorModel { Title = BackendResources.EmailSendingFailedTitle, Message = BackendResources.EmailSendingFailedMessage };
                    return View("Error", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[POST]AskQuestion: Unexpected error");
                return View("Error", model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ContactForm(string name, string email, string subject, string text)
        {
            try
            {
                if (await Email.Send("contactform@simpleq.at", "support@simpleq.at", "KONTAKTFORM: " + subject, "Name: " + name + "<br/>" + "Email: " + email + "<br/>" + text))
                {
                    return Redirect("http://vhp.simpleq.at/");
                }
                else
                {
                    logger.Error($"Contafct form e-mail sending failed.");
                    return Redirect("http://vhp.simpleq.at/");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[POST]AskQuestion: Unexpected error");
                return Redirect("http://vhp.simpleq.at/");
            }
        }
        #endregion
    }
}