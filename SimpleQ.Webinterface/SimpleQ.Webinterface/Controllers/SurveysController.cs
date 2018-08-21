using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SimpleQ.Webinterface.Models;

namespace SimpleQ.Webinterface.Controllers
{
    public class SurveysController : Controller
    {
        [HttpGet]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Survey svy)
        {
            return View();
        }
    }
}