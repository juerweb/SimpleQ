using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleQ.Models;
using SimpleQ.Resources;
using SimpleQ.Shared;

namespace SimpleQ.PageModels.Services
{
    public class WebAPIService : IWebAPIService
    {
        HttpClient httpClient = new HttpClient();

        public async Task<Boolean> AnswerSurvey(SurveyVote surveyVote)
        {
            Debug.WriteLine("Hallo! " + App.Key);

            httpClient.DefaultRequestHeaders.Add("auth-key", App.Key);

            Debug.WriteLine("Hallo!");


            StringContent content = new StringContent(JsonConvert.SerializeObject(surveyVote), Encoding.UTF8, "application/json");

            Debug.WriteLine(AppResources.APIMainURL + AppResources.APIAnswerSurveyPlusURL);

            HttpResponseMessage responseMessage = await httpClient.PostAsync(AppResources.APIMainURL + AppResources.APIAnswerSurveyPlusURL, content);

            Debug.WriteLine(responseMessage.StatusCode);

            return responseMessage.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<SurveyModel> GetSurveyData(int surveyID)
        {
            httpClient.DefaultRequestHeaders.Add("auth-key", App.Key);

            HttpResponseMessage responseMessage = await httpClient.GetAsync(AppResources.APIMainURL + AppResources.APIAnswerSurveyPlusURL + "?svyId=" + surveyID);

            Debug.WriteLine(responseMessage.StatusCode);

            String content = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                SurveyNotification notification = JsonConvert.DeserializeObject<SurveyNotification>(content);

                return new SurveyModel(notification);
            }
            else
            {
                return null;
            }
        }

        public async Task<RegistrationData> Register(string regCode, string deviceId)
        {
            httpClient.DefaultRequestHeaders.Add("auth-key", App.Key);

            HttpResponseMessage responseMessage = await httpClient.GetAsync(AppResources.APIMainURL + AppResources.APIRegisterPlusURL + "?regCode=" + regCode + "&deviceId=" + deviceId);

            String content = await responseMessage.Content.ReadAsStringAsync();

            Debug.WriteLine("Content of HttpResponse: " + content);
            Debug.WriteLine("Statuscode of HttpResponse: " + responseMessage.StatusCode);

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<RegistrationData>(content);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> Unregister(string persId, string custCode)
        {
            httpClient.DefaultRequestHeaders.Add("auth-key", App.Key);

            Debug.WriteLine(AppResources.APIMainURL + AppResources.APIUnregisterPlusURL);

            HttpResponseMessage responseMessage = await httpClient.GetAsync(AppResources.APIMainURL + AppResources.APIUnregisterPlusURL + "?persId=" + persId + "&custCode" + custCode);

            Debug.WriteLine("StatusCode of ResponseMessage: " + responseMessage.StatusCode);
            return responseMessage.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
