using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileTest
{
    public class Survey
    {
        public int SvyId { get; set; }
        public string CustCode { get; set; }
        public string SvyText { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public int TypeId { get; set; }
        public int CatId { get; set; }
    }
}
