using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http.Headers;

namespace SimpleQ.Tests.MobileTest
{
    class Program
    {
        static List<AnswerType> predefinedAnswerTypes = new List<AnswerType>
        {
            new AnswerType { TypeId = 1, TypeDesc = "YesNo", BaseId = 2 },
            new AnswerType { TypeId = 2, TypeDesc = "YesNoDontKnow", BaseId = 3 },
            new AnswerType { TypeId = 3, TypeDesc = "TrafficLight", BaseId = 3 },
            new AnswerType { TypeId = 4, TypeDesc = "Open", BaseId = 1 },
            new AnswerType { TypeId = 5, TypeDesc = "Dichotomous", BaseId = 2 },
            new AnswerType { TypeId = 6, TypeDesc = "PolytomousUnorderedSingle", BaseId = 3 },
            new AnswerType { TypeId = 7, TypeDesc = "PolytomousUnorderedMultiple", BaseId = 3 },
            new AnswerType { TypeId = 8, TypeDesc = "PolytomousOrderedSingle", BaseId = 3 },
            new AnswerType { TypeId = 9, TypeDesc = "PolytomousOrderedMultiple", BaseId = 3 },
            new AnswerType { TypeId = 10, TypeDesc = "LikertScale3", BaseId = 3 },
            new AnswerType { TypeId = 11, TypeDesc = "LikertScale4", BaseId = 3 },
            new AnswerType { TypeId = 12, TypeDesc = "LikertScale5", BaseId = 3 },
            new AnswerType { TypeId = 13, TypeDesc = "LikertScale6", BaseId = 3 },
            new AnswerType { TypeId = 14, TypeDesc = "LikertScale7", BaseId = 3 },
            new AnswerType { TypeId = 15, TypeDesc = "LikertScale8", BaseId = 3 },
            new AnswerType { TypeId = 16, TypeDesc = "LikertScale9", BaseId = 3 }
        };

        static int persId;
        static string custCode;
        static string custName;
        static Dictionary<int, string> deps = new Dictionary<int, string>();
        static WebRequestHandler requestHandler = new WebRequestHandler();

        const string SERVER = "https://simpleq.at";
        const int YESNO_ID = 1;
        const int SPEC_ID = 4;
        const int FREE_ID = 5;

        static void Main(string[] args)
        {
            var cert = new X509Certificate2(@"Certificates\SimpleQ.cer", "123SimpleQ...");
            requestHandler.ClientCertificates.Add(cert);

            try
            {
                string input;
                WriteLine("Type 'help' for further information.");

                while ((input = ReadLine()) != "exit")
                {
                    try
                    {
                        if (input == "register")
                        {
                            Register();
                        }
                        else if (input == "join")
                        {
                            Joint();
                        }
                        else if (input == "leave")
                        {
                            Leave();
                        }
                        else if (input == "unregister")
                        {
                            Unregister();
                        }
                        else if (input == "answeryesno")
                        {
                            AnswerYesNo();
                        }
                        else if (input == "answerspec")
                        {
                            AnswerSpec();
                        }
                        else if (input == "answerfree")
                        {
                            AnswerFree();
                        }
                        else if (input == "props")
                        {
                            Props();
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

        static void Register()
        {
            Task.Run(async () =>
            {
                WriteLine("Registration code:");
                string regCode = ReadLine();

                var cert = new X509Certificate2(@"Certificates\SimpleQ.cer", "123SimpleQ...");
                var requestHandler = new WebRequestHandler();
                requestHandler.ClientCertificates.Add(cert);
                using (HttpClient client = new HttpClient(requestHandler))
                {
                    HttpResponseMessage response = await client.GetAsync($"{SERVER}/api/mobile/register?regCode={Uri.EscapeDataString(regCode)}&deviceId=null");
                    string json = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        WriteLine(response.StatusCode);
                        return;
                    }

                    RegistrationData reg = JsonConvert.DeserializeObject<RegistrationData>(json);

                    persId = reg.PersId;
                    custCode = reg.CustCode;
                    custName = reg.CustName;
                    deps = new Dictionary<int, string>
                    {
                        { reg.DepId, reg.DepName }
                    };
                }
                Props();
            }).Wait();
        }

        static void Joint()
        {
            Task.Run(async () =>
            {
                WriteLine("Registration code:");
                string regCode = ReadLine();

                using (HttpClient client = new HttpClient(requestHandler))
                {
                    HttpResponseMessage response = await client.GetAsync($"{SERVER}/api/mobile/joinDepartment?regCode={Uri.EscapeDataString(regCode)}&persId={persId}");
                    string json = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        WriteLine(response.StatusCode);
                        return;
                    }

                    RegistrationData reg = JsonConvert.DeserializeObject<RegistrationData>(json);

                    persId = reg.PersId;
                    custCode = reg.CustCode;
                    custName = reg.CustName;
                    deps.Add(reg.DepId, reg.DepName);
                }
                Props();
            }).Wait();
        }

        static void Leave()
        {
            Task.Run(async () =>
            {
                WriteLine("DepId:");
                if (!int.TryParse(ReadLine(), out int depId)) return;
                using (HttpClient client = new HttpClient(requestHandler))
                {
                    HttpResponseMessage response = await client.GetAsync($"{SERVER}/api/mobile/leaveDepartment?persId={persId}&depId={depId}&custCode={Uri.EscapeDataString(custCode)}");
                    WriteLine(response.StatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        deps.Remove(depId);
                        Props();
                    }
                }
            }).Wait();
        }

        static void Unregister()
        {
            Task.Run(async () =>
            {
                using (HttpClient client = new HttpClient(requestHandler))
                {
                    WriteLine((await client.GetAsync($"{SERVER}/api/mobile/unregister?persId={persId}&custCode={Uri.EscapeDataString(custCode)}")).StatusCode);

                    persId = 0;
                    custCode = null;
                    custName = null;
                    deps = new Dictionary<int, string>();
                }
            }).Wait();
        }

        static void AnswerYesNo()
        {
            Task.Run(async () =>
            {
                HttpResponseMessage response;
                string json;
                SurveyData survey;

                using (HttpClient client = new HttpClient(requestHandler))
                {
                    response = await client.GetAsync($"{SERVER}/api/mobile/getSurveyData?svyId={YESNO_ID}");
                    json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        WriteLine(response.StatusCode);
                        return;
                    }

                    survey = JsonConvert.DeserializeObject<SurveyData>(json);

                    WriteLine(survey.SvyText);
                    WriteLine("Possible answers: ");
                    for (int i = 0; i < survey.AnswerOptions.Count; i++)
                    {
                        WriteLine($"  {survey.AnswerOptions[i].AnsId}: {survey.AnswerOptions[i].AnsText}");
                    }
                    WriteLine("Insert exactly one of the listed IDs:");

                    AnswerOption ansOpt = null;
                    if (int.TryParse(ReadLine().Trim(), out int ansId))
                    {
                        ansOpt = survey.AnswerOptions.Where(a => a.AnsId == ansId).FirstOrDefault();
                    }
                    if (ansOpt == null) return;

                    SurveyVote sv = new SurveyVote { VoteText = null, CustCode = custCode, ChosenAnswerOptions = new List<AnswerOption> { ansOpt } };

                    response = await client.PostAsync($"{SERVER}/api/mobile/answerSurvey", new StringContent(JsonConvert.SerializeObject(sv), Encoding.UTF8, "application/json"));
                    WriteLine(response.StatusCode);
                }
            }).Wait();
        }

        static void AnswerSpec()
        {
            Task.Run(async () =>
            {
                HttpResponseMessage response;
                string json;
                SurveyData survey;

                using (HttpClient client = new HttpClient(requestHandler))
                {
                    response = await client.GetAsync($"{SERVER}/api/mobile/getSurveyData?svyId={SPEC_ID}");
                    json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        WriteLine(response.StatusCode);
                        return;
                    }

                    survey = JsonConvert.DeserializeObject<SurveyData>(json);

                    WriteLine(survey.SvyText);
                    WriteLine("Possible answers: ");
                    for (int i = 0; i < survey.AnswerOptions.Count; i++)
                    {
                        WriteLine($"  {survey.AnswerOptions[i].AnsId}: {survey.AnswerOptions[i].AnsText}");
                    }
                    WriteLine("Insert at least one of the listed IDs, then confirm with 'done':");

                    string answer;
                    List<AnswerOption> ansOpts = new List<AnswerOption>();
                    while ((answer = ReadLine()).ToLower() != "done")
                    {
                        if (int.TryParse(answer, out int ansId))
                        {
                            AnswerOption ansOpt = survey.AnswerOptions.Where(a => a.AnsId == ansId).FirstOrDefault();
                            if (ansOpt != null && !ansOpts.Exists(a => a.AnsId == ansOpt.AnsId))
                                ansOpts.Add(ansOpt);
                        }
                    }

                    if (ansOpts.Count == 0) return;

                    SurveyVote sv = new SurveyVote { VoteText = null, CustCode = custCode, ChosenAnswerOptions = ansOpts };

                    response = await client.PostAsync($"{SERVER}/api/mobile/answerSurvey", new StringContent(JsonConvert.SerializeObject(sv), Encoding.UTF8, "application/json"));
                    WriteLine(response.StatusCode);
                }
            }).Wait();
        }

        static void AnswerFree()
        {
            Task.Run(async () =>
            {
                HttpResponseMessage response;
                string json;
                SurveyData survey;

                using (HttpClient client = new HttpClient(requestHandler))
                {
                    response = await client.GetAsync($"{SERVER}/api/mobile/getSurveyData?svyId={FREE_ID}");
                    json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        WriteLine(response.StatusCode);
                        return;
                    }

                    survey = JsonConvert.DeserializeObject<SurveyData>(json);

                    WriteLine(survey.SvyText);
                    WriteLine("Insert your answer:");

                    string answer = ReadLine();

                    SurveyVote sv = new SurveyVote { VoteText = answer, CustCode = custCode, ChosenAnswerOptions = new List<AnswerOption> { survey.AnswerOptions.FirstOrDefault() } };

                    response = await client.PostAsync($"{SERVER}/api/mobile/answerSurvey", new StringContent(JsonConvert.SerializeObject(sv), Encoding.UTF8, "application/json"));
                    WriteLine(response.StatusCode);
                }
            }).Wait();
        }

        static void Props()
        {
            WriteLine($"PersId: {persId}, CustCode: {custCode}, CustName: {custName}");
            WriteLine("Departments:");
            foreach (KeyValuePair<int, string> kv in deps)
                WriteLine($"   {kv.Key} {kv.Value}");
        }

        static void Cls()
        {
            Console.Clear();
        }

        static void Help()
        {
            WriteLine("Type 'register' to sign up, 'unregister' to unregister,");
            WriteLine("'join' to join a group, 'leave' to leave a group,");
            WriteLine("'props' to show the current properties,");
            WriteLine("'answeryesno' to answer a yes/no survey, 'answerspec' to answer a survey with specified text answers,");
            WriteLine("'answerfree' to answer a free-text survey,");
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
