using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class MultiResultModel
    {
        #region Controller to View
        public string SvyText { get; set; }
        public List<DateTime> SurveyDates { get; set; }
        public List<KeyValuePair<string, List<int>>> Votes { get; set; }
        #endregion
    }
}