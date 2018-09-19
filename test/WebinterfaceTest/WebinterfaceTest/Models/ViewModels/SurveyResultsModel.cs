using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class SurveyResultsModel
    {
        #region Controller to View
        public List<Survey> Surveys { get; set; }
        public List<SurveyCategory> SurveyCategories { get; set; }
        public List<AnswerType> AnswerTypes { get; set; }
        #endregion

        #region View to Controller
        #endregion
    }
}