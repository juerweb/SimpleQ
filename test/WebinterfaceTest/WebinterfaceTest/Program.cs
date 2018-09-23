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
                        else if (input == "loadsingleresult")
                        {
                            LoadSingleResult();
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
                        StartDate = DateTime.Now.AddSeconds(15),
                        EndDate = DateTime.Now.AddDays(7),
                        Amount = 4,
                        TypeId = 6
                    },
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
                        StartDate = DateTime.Now.AddSeconds(15),
                        EndDate = DateTime.Now.AddDays(7),
                        Amount = 5,
                        TypeId = 1
                    },
                    SelectedDepartments = new List<int> { 1 }
                };

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.PostAsync($"{SERVER}/surveyCreation/new/", new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
                    WriteLine(response.StatusCode);
                }
            }).Wait();
        }

        static void LoadSingleResult()
        {
            Task.Run(async () =>
            {
                WriteLine("SvyId:");
                if (!int.TryParse(ReadLine(), out int svyId)) return;

                using(HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{SERVER}/surveyResults/loadSingleResult?svyId={svyId}");
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
