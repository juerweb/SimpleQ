using SimpleQ.Shared;
using System;
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

            httpListener.Prefixes.Add("http://localhost:1899/test/");

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

            switch (link.AbsolutePath)
            {
                case "Register":
                    RegistrationData


                    response.ContentType = "text/json";
                    response.StatusCode = 200;
                    response.StatusDescription = "OK";
                    StreamWriter writer = new StreamWriter(response.OutputStream);
                    writer.WriteLine("<http><body>Test</body></http>");
                    writer.Close();
                    break;
                case "Unregister":
                    break;
                case "AnswerSurvey":
                    break;
            }
            response.Close();
        }
    }
}
