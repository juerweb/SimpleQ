﻿using System;
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
        public List<Department> Departments { get; set; }
        public List<Survey> RecentSurveys { get; set; }
        #endregion

        #region View to Controller
        public Survey Survey { get; set; }
        public string CatName { get; set; }
        public List<int> SelectedDepartments { get; set; }
        public List<string> TextAnswerOptions { get; set; }
        #endregion
    }
}