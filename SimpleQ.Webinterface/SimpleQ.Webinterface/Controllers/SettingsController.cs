﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings
        [HttpGet]
        public ActionResult ChangeMinGroup(int size)
        {
            return PartialView(viewName: "_Settings", model: null);
        }
    }
}