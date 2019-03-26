using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class SupportModel
    {
        #region Controller to View
        public List<FaqEntry> FaqEntries { get; set; }
        #endregion

        #region View to Controller
        public string QuestionCatgeory { get; set; }
        public string QuestionText { get; set; }
        public string Email { get; set; }
        #endregion
    }
}