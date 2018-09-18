using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Shared
{
    public class SurveyVote
    {
        public List<AnswerOption> ChosenAnswerOptions { get; set; }
        public string VoteText { get; set; }
        public string CustCode { get; set; }
    }
}