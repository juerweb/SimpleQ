using System;
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
        public WebAPIService()
        {
            Debug.WriteLine("Constructor of WebAPIService...", "Info");
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(WebAPIService)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("SimpleQ.Resources.private.key");

            StreamReader streamReader = new StreamReader(stream);

            String key = streamReader.ReadToEnd();

            httpClient.DefaultRequestHeaders.Add("auth-key", key);
        }

        

        public async Task<Boolean> AnswerSurvey(SurveyVote surveyVote)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(surveyVote), Encoding.UTF8, "application/json");

            Debug.WriteLine(AppResources.APIMainURL + AppResources.APIAnswerSurveyPlusURL);

            Debug.WriteLine(JsonConvert.SerializeObject(surveyVote));

            HttpResponseMessage responseMessage = await httpClient.PostAsync(AppResources.APIMainURL + AppResources.APIAnswerSurveyPlusURL, content);

            Debug.WriteLine(responseMessage.StatusCode);

            return responseMessage.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<SurveyModel> GetSurveyData(int surveyID)
        {
            HttpResponseMessage responseMessage = await httpClient.GetAsync(AppResources.APIMainURL + AppResources.APIAnswerSurveyPlusURL + "?svyId=" + surveyID);

            Debug.WriteLine(responseMessage.StatusCode);

            String content = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                SurveyData notification = JsonConvert.DeserializeObject<SurveyData>(content);

                return new SurveyModel(notification);
            }
            else
            {
                return null;
            }
        }

        public async Task<RegistrationData> JoinDepartment(string regCode, int persId)
        {
            HttpResponseMessage responseMessage = await httpClient.GetAsync(AppResources.APIMainURL + AppResources.APIJoinDepartmentPlusURL + "?regCode=" + regCode + "&persId=" + persId);

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

        public async Task<bool> LeaveDepartment(int persId, int depId, string custCode)
        {
            Debug.WriteLine(AppResources.APIMainURL + AppResources.APILeaveDepartmentPlusURL);

            HttpResponseMessage responseMessage = await httpClient.GetAsync(AppResources.APIMainURL + AppResources.APILeaveDepartmentPlusURL + "?persId=" + persId + "&depId=" + depId + "&custCode=" + custCode);

            Debug.WriteLine("StatusCode of ResponseMessage: " + responseMessage.StatusCode);
            return responseMessage.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<RegistrationData> Register(string regCode, string deviceId)
        {
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

            Debug.WriteLine(AppResources.APIMainURL + AppResources.APIUnregisterPlusURL);

            HttpResponseMessage responseMessage = await httpClient.GetAsync(AppResources.APIMainURL + AppResources.APIUnregisterPlusURL + "?persId=" + persId + "&custCode=" + custCode);

            Debug.WriteLine("StatusCode of ResponseMessage: " + responseMessage.StatusCode);
            return responseMessage.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
