﻿using Newtonsoft.Json;
using SimpleQ.Shared;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;

namespace APISimulator
{
    class Program
    {
        static string key;
        static void Main(string[] args)
        {
            Console.WriteLine("HTTP Server wird gestartet...");

            HttpListener httpListener = new HttpListener();

            key = File.ReadAllText("private.key");
            //Console.WriteLine("Key: " + key);

            httpListener.Prefixes.Add("http://localhost:1899/api/mobile/Register/");
            httpListener.Prefixes.Add("http://localhost:1899/api/mobile/Unregister/");
            httpListener.Prefixes.Add("http://localhost:1899/api/mobile/AnswerSurvey/");
            httpListener.Prefixes.Add("http://localhost:1899/api/mobile/GetSurveyData/");

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

            if (request.Headers.Get("auth-key") != key)
            {
                response.ContentType = "text/json";
                response.StatusCode = 401;
                response.StatusDescription = "Unauthorized";
            }
            else
            {

                Uri link = request.Url;

                Console.WriteLine(link.AbsolutePath);
                switch (link.AbsolutePath)
                {
                    case "/api/mobile/Register":
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
                    case "/api/mobile/Unregister":
                        response.ContentType = "text/json";
                        response.StatusCode = 200;
                        response.StatusDescription = "OK";
                        break;
                    case "/api/mobile/AnswerSurvey":
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
                    case "/api/mobile/GetSurveyData":
                        break;
                }
            }

            response.Close();
        }
    }
}