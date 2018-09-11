using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Mobile
{
    public class NotificationContent
    {
        public int SvyId { get; set; }
        public int SvyText { get; set; }
        public DateTime EndDate { get; set; }
        public int TypeId { get; set; }
        public string CatName { get; set; }
        public Dictionary<int, string> PossibleTextAnswers { get; set; }
    }
}