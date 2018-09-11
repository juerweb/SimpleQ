using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Mobile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MobileTest
{
    class Program
    {
        static List<Answer> predefinedAnswers = new List<Answer>
        {
            new Answer { AnsId = 1, AnsDesc = "Yes", TypeId = 1},
            new Answer { AnsId = 2, AnsDesc = "No", TypeId = 1},
            new Answer { AnsId = 3, AnsDesc = "Green", TypeId = 2},
            new Answer { AnsId = 4, AnsDesc = "Yellow", TypeId = 2},
            new Answer { AnsId = 5, AnsDesc = "Red", TypeId = 2},
            new Answer { AnsId = 6, AnsDesc = "OneWord", TypeId = 3},
            new Answer { AnsId = 7, AnsDesc = "SpecifiedText", TypeId = 4},
        };

        static List<AnswerType> predefinedAnswerTypes = new List<AnswerType>
        {
            new AnswerType {TypeId = 1, TypeDesc = "YesNo"},
            new AnswerType {TypeId = 2, TypeDesc = "TrafficLight"},
            new AnswerType {TypeId = 3, TypeDesc = "OneWord"},
            new AnswerType {TypeId = 4, TypeDesc = "SpecifiedText"},
        };

        static int persId;
        static string custCode;
        static int depId;
        static string depName;

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
                        if (input == "register")
                        {
                            Register();
                        }
                        else if (input == "unregister")
                        {
                           Unregister();
                        }
                        else if (input == "props")
                        {
                            WriteLine($"PersId: {persId}, CustCode: {custCode}, DepId: {depId}, DepName: {depName}");
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
            WriteLine("Registration code:");
            string regCode = ReadLine();

            using (WebClient client = new WebClient())
            {
                string json = client.DownloadString($"http://localhost:55445/api/mobile/register?regCode={Uri.EscapeDataString(regCode)}&deviceId=null");
                RegistrationData reg = JsonSerializer.Create().Deserialize<RegistrationData>(new JsonTextReader(new StringReader(json)));

                persId = reg.PersId;
                custCode = reg.CustCode;
                depId = reg.DepId;
                depName = reg.DepName;
            }
            WriteLine($"PersId: {persId}, CustCode: {custCode}, DepId: {depId}, DepName: {depName}");
        }

        static void Unregister()
        {
            using (WebClient client = new WebClient())
            {
                WriteLine(client.DownloadString($"http://localhost:55445/api/mobile/register?persId={persId}&custCode={Uri.EscapeDataString(custCode)}"));

                persId = 0;
                custCode = null;
                depId = 0;
                depName = null;
            }

            WriteLine("Successfully unregistered");
        }

        static void AnswerYesNo()
        {

        }

        static void Cls()
        {
            Console.Clear();
        }

        static void Help()
        {
            WriteLine("Type 'register' to sign up, 'unregister' to unregister,");
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
    }
}
