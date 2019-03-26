using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleQ.Webinterface.Models.ViewModels
{
    public class SettingsModel
    {
        public struct SurveyWrapper
        {
            public Survey Survey { get; set; }
            public int NumberOfAnswers { get; set; }
            public double SurveyPrice { get; set; }
        }

        public struct BillWrapper
        {
            public Bill Bill { get; set; }
            public List<SurveyWrapper> Surveys { get; set; } 
        }

        #region Controller to View
        public int MinGroupSize { get; set; }
        public List<SurveyCategory> Categories { get; set; }
        public List<PaymentMethod> PaymentMethods { get; set; }
        public List<AnswerType> ActivatedAnswerTypes { get; set; }
        public List<AnswerType> DeactivatedAnswerTypes { get; set; }
        public List<Survey> Templates { get; set; }
        public List<BillWrapper> Bills { get; set; }
        public double OutstandingBalance { get; set; }
        public int CurrentSurveyAmount { get; set; }
        public int CurrentVoteAmount { get; set; }
        #endregion

        #region Any direction
        public string Name { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string Plz { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string LanguageCode { get; set; }
        public int DataStoragePeriod { get; set; }
        public int PaymentMethodId { get; set; }
        #endregion

        #region View to Controller
        public string Password { get; set; }
        public List<int> CheckedAnswerTypes { get; set; }
        #endregion
    }
}