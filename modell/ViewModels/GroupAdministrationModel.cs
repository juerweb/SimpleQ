﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class GroupAdministrationModel
    {
        #region Controller to View
        public Dictionary<Department, int> Departments { get; set; }
        #endregion

        #region View to Controller
        public int DepId { get; set; }
        public List<string> Emails { get; set; }
        public string InvitationSubject { get; set; }
        [AllowHtml]
        public string InvitationText { get; set; }
        #endregion
    }
}