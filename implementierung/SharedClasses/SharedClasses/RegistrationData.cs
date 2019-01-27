using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Shared
{
    public class RegistrationData
    {
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public int PersId { get; set; }
        public int DepId { get; set; }
        public string DepName { get; set; }
        public string AuthToken { get; set; }
    }
}