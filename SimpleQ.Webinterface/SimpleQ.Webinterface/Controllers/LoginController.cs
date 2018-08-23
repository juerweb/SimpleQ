using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SimpleQ.Webinterface.Models;

namespace SimpleQ.Webinterface.Controllers
{
    public class LoginController : Controller
    {
        [HttpPost]
        public ActionResult Register(Customer cust)
        {
            return View();
        }
    }
}