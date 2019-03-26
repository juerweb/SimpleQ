using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class SingleResultModel
    {
        #region Controller to View
        public Survey Survey { get; set; }
        public string CatName { get; set; }
        public string TypeName { get; set; }
        public List<string> DepartmentNames { get; set; }
        public Dictionary<string, int> Votes { get; set; }
        public List<string> FreeTextVotes { get; set; }
        #endregion

        #region View to Controller
        #endregion
    }
}