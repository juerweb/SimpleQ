using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Shared
{
    public class SurveyNotification
    {
        public int SvyId { get; set; }
        public string SvyText { get; set; }
        public DateTime EndDate { get; set; }
        public int TypeId { get; set; }
        public string CatName { get; set; }
        public Dictionary<int, string> PossibleTextAnswers { get; set; }
    }
}