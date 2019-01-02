using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
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
                    FaqEntries = db.FaqEntries.Where(f => !f.IsMobile).ToList()
                };

                ViewBag.emailConfirmed = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().EmailConfirmed;
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


            if (Email.Send("contactform@simpleq.at", "support@simpleq.at", "SIMPLEQ SUPPORT: " + req.QuestionCatgeory, req.QuestionText))
            {
                return Index();
            }
            else
            {
                var model = new ErrorModel { Title = "Unable to send e-mail", Message = "Sending failed due to internal error(s)." };
                return View("Error", model);
            }
        }
        #endregion

        #region Helpers
        private void AddModelError(string key, string errorMessage, ref bool error)
        {
            try
            {
                logger.Debug($"Model error: {key}: {errorMessage}");
                ModelState.AddModelError(key, errorMessage);
                error = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "AddModelError: Unexpected error");
                throw ex;
            }
        }

        private string CustCode => HttpContext.GetOwinContext().Authentication.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
        #endregion
    }
}