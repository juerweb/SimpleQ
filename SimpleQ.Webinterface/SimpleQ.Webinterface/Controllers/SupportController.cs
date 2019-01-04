using NLog;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
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
                    var model = new SupportModel
                    {
                        FaqEntries = await db.FaqEntries.Where(f => !f.IsMobile).ToListAsync()
                    };

                    ViewBag.emailConfirmed = (await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync()).EmailConfirmed;
                    logger.Debug("Support page loaded successfully");

                    return View("Support", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
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
                    AddModelError("Model", "Model object must not be null.", ref err);

                if (string.IsNullOrEmpty(req.Email))
                    AddModelError("Email", "Email must not be null.", ref err);

                if (string.IsNullOrEmpty(req.QuestionText))
                    AddModelError("QuestionText", "QuestionText must not be null.", ref err);

                if (string.IsNullOrEmpty(req.QuestionCatgeory))
                    AddModelError("QuestionCatgeory", "QuestionCatgeory must not be null.", ref err);

                if (err)
                {
                    logger.Debug("AskQuestion validation failed. Exiting method");
                    return await Index();
                }


                req.QuestionText = $"FROM: {req.Email}{Environment.NewLine}{req.QuestionText}";


                if (await Email.Send("contactform@simpleq.at", "support@simpleq.at", "SIMPLEQ SUPPORT: " + req.QuestionCatgeory, req.QuestionText))
                {
                    logger.Debug("Support e-mail sent successfully");
                    return await Index();
                }
                else
                {
                    logger.Error($"Support e-mail sending failed for: {CustCode}");
                    var model = new ErrorModel { Title = "Unable to send e-mail", Message = "Sending failed due to internal error(s)." };
                    return View("Error", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[POST]AskQuestion: Unexpected error");
                return View("Error", model);
            }
        }
        #endregion
    }
}