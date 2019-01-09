using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class SingleResultModel
    {
        #region Controller to View
        public int SvyId { get; set; }
        public string SvyText { get; set; }
        public TimeSpan Period { get; set; }
        public List<KeyValuePair<string, int>> Votes { get; set; }
        public List<string> FreeTextVotes { get; set; }
        #endregion
    }
}