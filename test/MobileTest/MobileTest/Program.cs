using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Mobile;

namespace MobileTest
{
    class Program
    {
        static HubConnection conn = new HubConnection("http://localhost:55445");
        static IHubProxy hub;

        static int persId;
        static string custCode;
        static int depId;
        static string depName;

        static void Main(string[] args)
        {
            LoadConfig();
            WriteLine("Connecting...");
            try
            {
                hub = conn.CreateHubProxy("SimpleQHub");
                conn.Start().Wait();
                WriteLine($"Connected to {conn.Url}");

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
                        else if (input == "unregister")
                        {
                            Unregister();
                        }
                        else if (input == "login")
                        {
                            Login();
                        }
                        else if (input.StartsWith("relogin"))
                        {
                            Relogin(int.Parse(input.Split(' ')[1]), input.Split(' ')[2]);
                        }
                        else if (input == "logout")
                        {
                            Logout();
                        }
                        else if (input == "props")
                        {
                            WriteLine($"PersId: {persId}, CustCode: {custCode}, DepId: {depId}, DepName: {depName}");
                        }
                        else if (input == "answeryesno")
                        {
                            AnswerYesNo();
                        }
                        else if (input == "answerspec")
                        {
                            AnswerSpec();
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
                Logout();
                conn.Stop();
                SaveConfig();
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
            WriteLine("Registration code:");
            string regCode = ReadLine();

            Task<OperationStatus> t = hub.Invoke<OperationStatus>("Register", regCode);
            t.Wait();

            if (t.Result.StatusCode == StatusCode.REGISTERED)
            {
                persId = t.Result.PersId;
                custCode = t.Result.AssignedDepartment.CustCode;
                depId = t.Result.AssignedDepartment.DepId;
                depName = t.Result.AssignedDepartment.DepName;
            }

            WriteLine(t.Result);
            WriteLine($"{persId} {custCode} {depId} {depName}");
        }

        static void Unregister()
        {
            hub.Invoke("Unregister", persId, custCode).Wait();
            WriteLine("Unregistered successfully.");
        }

        static void Login()
        {
            Task<OperationStatus> t = hub.Invoke<OperationStatus>("Login", persId, custCode);
            t.Wait();
            WriteLine(t.Result);
        }

        static void Relogin(int p, string c)
        {
            persId = p;
            custCode = c;
            Login();
            SaveConfig();
        }

        static void Logout()
        {
            hub.Invoke("Logout").Wait();
            WriteLine("Logged out.");
        }

        static void AnswerYesNo()
        {
            // sample survey
            Survey svy = new Survey { SvyId = 1, CustCode = "m4rku5", SvyText = "Ist N.H. ein Nazi?", StartDate = DateTime.Parse("2018-08-24"), EndDate = DateTime.Parse("2019-08-24"), TypeId = 1, CatId = 1 };

            if (custCode == null)
            {
                WriteLine("Login or register first!");
                return;
            }

            Task<Answer[]> t = hub.Invoke<Answer[]>("LoadAnswersOfType", svy.TypeId);
            t.Wait();
            Answer[] answers = t.Result;

            WriteLine(svy.SvyText);
            WriteLine("Possible answers:");
            answers.ToList().ForEach(a =>
            {
                WriteLine("  " + a.AnsDesc);
            });

            string ans;
            int? ansId;
            do
            {
                ans = ReadLine();
                ansId = answers.Where(a => a.AnsDesc.ToLower() == ans.ToLower()).Select(a => a.AnsId as int?).FirstOrDefault();
            } while (ansId == null);

            hub.Invoke("AnswerSurvey", new Vote(svy.SvyId, custCode, ansId.Value, null, null)).Wait();
            WriteLine("Answered successfully.");
        }

        static void AnswerSpec()
        {
            // sample survey
            Survey svy = new Survey { SvyId = 2, CustCode = "m4rku5", SvyText = "Sind Sie ein Pajero", StartDate = DateTime.Parse("2018-08-24"), EndDate = DateTime.Parse("2019-08-24"), TypeId = 4, CatId = 1 };

            if (custCode == null)
            {
                WriteLine("Login or register first!");
                return;
            }

            Task<Answer[]> t1 = hub.Invoke<Answer[]>("LoadAnswersOfType", svy.TypeId);
            t1.Wait();
            int ansId = t1.Result[0].AnsId;

            Task<SpecifiedTextAnswer[]> t2 = hub.Invoke<SpecifiedTextAnswer[]>("LoadSpecifiedTextAnswers", svy.SvyId, custCode);
            t2.Wait();
            SpecifiedTextAnswer[] specs = t2.Result;

            WriteLine(svy.SvyText);
            WriteLine("Possible answers:");
            specs.ToList().ForEach(a =>
            {
                WriteLine("  " + a.SpecText);
            });

            string spec;
            int? specId;
            do
            {
                spec = ReadLine();
                specId = specs.Where(s => s.SpecText.ToLower() == spec.ToLower()).Select(s => s.SpecId as int?).FirstOrDefault();
            } while (specId == null);

            hub.Invoke("AnswerSurvey", new Vote(svy.SvyId, custCode, ansId, null, specId)).Wait();
            WriteLine("Answered successfully.");
        }

        static void Cls()
        {
            Console.Clear();
        }

        static void Help()
        {
            WriteLine("Type 'register' to sign up, 'unregister' to unregister 'login' to sign in, 'logout' to sign out,");
            WriteLine("'relogin <persId> <custCode> to re-login after registered,");
            WriteLine("'props' to show the current properties,");
            WriteLine("'answeryesno' to answer a yes/no survey, 'answerspec' to answer a survey with specified text,");
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

        static void LoadConfig()
        {
            persId = int.Parse(ConfigurationManager.AppSettings["PersId"]);
            custCode = ConfigurationManager.AppSettings["CustCode"];
            depId = int.Parse(ConfigurationManager.AppSettings["DepId"]);
            depName = ConfigurationManager.AppSettings["DepName"];
        }

        static void SaveConfig()
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfg.AppSettings.Settings["PersId"].Value = persId.ToString();
            cfg.AppSettings.Settings["CustCode"].Value = custCode;
            cfg.AppSettings.Settings["DepId"].Value = depId.ToString();
            cfg.AppSettings.Settings["DepName"].Value = depName;
            cfg.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
