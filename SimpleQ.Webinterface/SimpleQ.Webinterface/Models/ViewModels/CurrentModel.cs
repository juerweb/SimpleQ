using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class CurrentModel
    {
        public struct SurveyWrapper
        {
            public int SvyId { get; set; }
            public String SvyText { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        #region Controller to View
        public List<SurveyWrapper> Surveys { get; set; }
        public List<Survey> PeriodicSurveys { get; set; }
        #endregion
    }
}