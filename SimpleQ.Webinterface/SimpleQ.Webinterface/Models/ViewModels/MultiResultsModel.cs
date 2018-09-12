﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class MultiResultsModel
    {
        #region Controller to View
        public string CatName { get; set; }
        public string TypeName { get; set; }
        public List<string> DepartmentNames { get; set; }
        public Dictionary<string, int> Votes { get; set; }
        #endregion

        #region View to Controller
        public int CatId { get; set; }
        public int TypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        #endregion
    }
}