using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SimpleQ.Resources;
using SimpleQ.Shared;

namespace SimpleQ.PageModels.Services
{
    public class WebAPIService : IWebAPIService
    {
        HttpClient httpClient = new HttpClient();
        public Task AnswerSurvey(SurveyVote surveyVote)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Register(string regCode, string deviceId)
        {
            httpClient.GetAsync(AppResources.APIMainURL);
        }

        public Task Unregister(string persId, string custCode)
        {
            throw new NotImplementedException();
        }
    }
}
