using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

namespace SimpleQ.Tests.WebinterfaceTest
{
    class Program
    {
        const string SERVER = "https://localhost:44338";

        static JsonSerializer ser = new JsonSerializer();

        static void Main(string[] args)
        {
            try
            {
                string input;
                WriteLine("Type 'help' for further information.");

                while ((input = ReadLine()) != "exit")
                {

                    try
                    {
                        if (input == "newspecsurvey")
                        {
                            NewSpecSurvey();
                        }
                        else if (input == "newyesnosurvey")
                        {
                            NewYesNoSurvey();
                        }
                        else if (input == "cls")
                        {
                            Cls();
                        }
                        else if (input == "help")
                        {
                            Help();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.InnerException?.Message);
                        Console.WriteLine(ex.InnerException?.InnerException?.Message);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message);
                Console.WriteLine(ex.InnerException?.InnerException?.Message);
            }
            Console.ReadKey();
        }

        static void NewSpecSurvey()
        {
            Task.Run(async () =>
            {
                SurveyCreationModel model = new SurveyCreationModel
                {
                    Survey = new Survey
                    {
                        CatId = 1,
                        CustCode = "m4rku5",
                        SvyText = "Wie groß sind die Ohren von S.K.?",
                        Amount = 5,
                        TypeId = 6
                    },
                    StartDate = DateTime.Now.Date,
                    StartTime = DateTime.Now.AddSeconds(15) - DateTime.Now.Date,
                    EndDate = DateTime.Now.Date.AddDays(7),
                    EndTime = DateTime.Now - DateTime.Now.Date,
                    SelectedDepartments = new List<int> { 1, 2 },
                    TextAnswerOptions = new List<string> { "sehr groß", "gigantisch", "brobdingnagisch", "sooo huge", "muy grande" }
                };

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.PostAsync($"{SERVER}/surveyCreation/new/", new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
                    WriteLine(response.StatusCode);
                }
            }).Wait();
        }

        static void NewYesNoSurvey()
        {
            Task.Run(async () =>
            {
                SurveyCreationModel model = new SurveyCreationModel
                {
                    Survey = new Survey
                    {
                        CatId = 2,
                        CustCode = "m4rku5",
                        SvyText = "Rauchen Sie gerne Marihuana?",
                        Amount = 5,
                        TypeId = 1
                    },
                    StartDate = DateTime.Now.Date,
                    StartTime = DateTime.Now.AddSeconds(120) - DateTime.Now.Date,
                    EndDate = DateTime.Now.Date.AddDays(7),
                    EndTime = DateTime.Now - DateTime.Now.Date,
                    SelectedDepartments = new List<int> { 1 }
                };

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.PostAsync($"{SERVER}/surveyCreation/new/", new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
                    WriteLine(response.StatusCode);
                }
            }).Wait();
        }

        static void Cls()
        {
            Console.Clear();
        }

        static void Help()
        {
            WriteLine("Type 'newyesnosurvey' to create a new yes/no-survey with sample data,");
            WriteLine("'newspecsurvey' to create a new survey with specified text answers with sample data,");
            WriteLine("'cls' to clear the screen or 'exit' to close program.");
        }

        static string ReadLine()
        {
            Console.Write("> ");
            return Console.ReadLine();
        }

        static void WriteLine()
        {
            Console.WriteLine();
        }

        static void WriteLine(object obj)
        {
            Console.WriteLine($"> {obj.ToString()}");
        }
    }
}
