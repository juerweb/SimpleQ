using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public interface IWebAPIService
    {
        Task<Boolean> Register(String regCode, String deviceId);
        Task Unregister(String persId, String custCode);
        Task AnswerSurvey(SurveyVote surveyVote);
    }
}
