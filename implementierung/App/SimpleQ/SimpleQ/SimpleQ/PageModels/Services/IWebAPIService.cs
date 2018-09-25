using SimpleQ.Models;
using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public interface IWebAPIService
    {
        Task<RegistrationData> Register(String regCode, String deviceId);
        Task<Boolean> Unregister(String persId, String custCode);
        Task<Boolean> AnswerSurvey(SurveyVote surveyVote);
        Task<SurveyModel> GetSurveyData(int surveyID);
    }
}
