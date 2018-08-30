using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class SurveyCreationController : Controller
    {
        // GET: SurveyCreation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadPartialView(string partialView)
        {
            return PartialView(partialView);
        }
    }
}