using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class SurveyResultsModel
    { 
        public struct SurveyResultWrapper
        {
            public string SvyText { get; set; }
            public int Amount { get; set; }
            public SurveyCategory SurveyCategory { get; set; }
            public AnswerType AnswerType { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<string> Departments { get; set; }
        }

        #region Controller to View
        public List<SurveyResultWrapper> Surveys { get; set; }
        #endregion
    }
}