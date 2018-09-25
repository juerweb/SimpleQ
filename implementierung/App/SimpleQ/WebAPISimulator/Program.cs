using Newtonsoft.Json;
using SimpleQ.Shared;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;

namespace WebAPISimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("HTTP Server wird gestartet...");

            HttpListener httpListener = new HttpListener();

            httpListener.Prefixes.Add("http://localhost:1899/api/mobile/register/");
            httpListener.Prefixes.Add("http://localhost:1899/api/mobile/unregister/");
            httpListener.Prefixes.Add("http://localhost:1899/api/mobile/answerSurvey/");
            httpListener.Prefixes.Add("http://localhost:1899/api/mobile/getSurveyData/");

            httpListener.Start();

            new Thread(() => {
                HandleListener(httpListener);
            }).Start();
        }

        private static void HandleListener(HttpListener httpListener)
        {
            while (true)
            {
                HttpListenerContext context = httpListener.GetContext();

                new Thread(() => {
                    HandleRequest(context);
                }).Start();
            }

        }

        private static void HandleRequest(HttpListenerContext context)
        {

            Console.WriteLine("Handle new Connection...");
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            Uri link = request.Url;


            Console.WriteLine(link.AbsolutePath);
            switch (link.AbsolutePath)
            {
                case "/api/mobile/register":
                    NameValueCollection parameters = request.QueryString;
                    Console.WriteLine(parameters.Get("Test") == null);
                    if (parameters.Count == 2)
                    {
                        String regCode = parameters.Get("regCode");
                        String deviceId = parameters.Get("deviceId");
                        if (regCode != null && deviceId != null)
                        {
                            RegistrationData registrationData = new RegistrationData();
                            registrationData.CustCode = "1234";
                            registrationData.DepId = 1;
                            registrationData.DepName = "Software Entwicklung";
                            registrationData.PersId = 501;

                            response.ContentType = "text/json";
                            response.StatusCode = 200;
                            response.StatusDescription = "OK";

                            StreamWriter writer = new StreamWriter(response.OutputStream);
                            writer.WriteLine(JsonConvert.SerializeObject(registrationData));
                            writer.Close();
                        }
                        else
                        {
                            response.ContentType = "text/json";
                            response.StatusCode = 901;
                            response.StatusDescription = "Not enough or wrong paramter";
                        }
                    }
                    else
                    {
                        response.ContentType = "text/json";
                        response.StatusCode = 901;
                        response.StatusDescription = "Not enough or wrong paramter";
                    }

                    break;
                case "/api/mobile/unregister":
                    response.ContentType = "text/json";
                    response.StatusCode = 200;
                    response.StatusDescription = "OK";
                    break;
                case "/api/mobile/answerSurvey":
                    Console.WriteLine("api/mobile/AnswerSurvey");
                    StreamReader reader = new StreamReader(request.InputStream);
                    SurveyVote vote = JsonConvert.DeserializeObject<SurveyVote>(reader.ReadToEnd());

                    Console.WriteLine(reader.ReadToEnd());

                    if (vote != null)
                    {
                        response.ContentType = "text/json";
                        response.StatusCode = 200;
                        response.StatusDescription = "OK";
                    }
                    else
                    {
                        response.ContentType = "text/json";
                        response.StatusCode = 901;
                        response.StatusDescription = "Not enough or wrong paramter";
                    }
                    break;
                case "/api/mobile/getSurveyData":
                    break;
            }
            response.Close();
        }
    }
}
