using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class GroupAdministrationModel
    {
        #region Controller to View
        public List<Department> Departments { get; set; }
        #endregion

        #region View to Controller
        public int DepId { get; set; }
        public List<string> Emails { get; set; }
        public string InvitationSubject { get; set; }
        public string InvitationText { get; set; }
        #endregion
    }
}