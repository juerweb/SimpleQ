using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleQ.Webinterface.Models;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class SurveyCreationModel
    {
        #region Controller to View
        public List<SurveyCategory> SurveyCategories { get; set; }
        public List<AnswerType> AnswerTypes { get; set; }
        public List<Group> Groups { get; set; }
        #endregion

        #region View to Controller
        public Survey Survey { get; set; }
        public List<Group> SelectedGroups { get; set; }
        #endregion
    }
}