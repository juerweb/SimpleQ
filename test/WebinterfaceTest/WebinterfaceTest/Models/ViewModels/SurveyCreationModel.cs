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
        public Dictionary<Department, int> Departments { get; set; }
        public List<Survey> SurveyTemplates { get; set; }
        #endregion

        #region View to Controller
        public Survey Survey { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<int> SelectedDepartments { get; set; }
        public List<string> TextAnswerOptions { get; set; }
        #endregion
    }
}