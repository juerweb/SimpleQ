using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileTest
{
    public class Vote
    {
        public Vote(int svyId, string custCode, int ansId, string voteText, int? specId)
        {
            SvyId = svyId;
            CustCode = custCode;
            AnsId = ansId;
            VoteText = voteText;
            SpecId = specId;
        }

        public Vote()
        {

        }

        public int VoteId { get; set; }
        public int SvyId { get; set; }
        public string CustCode { get; set; }
        public int AnsId { get; set; }
        public string VoteText { get; set; }
        public int? SpecId { get; set; }
    }
}
