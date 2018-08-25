using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

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
                    else if (input == "logout")
                    {
                        Logout();
                    }
                    else if (input == "props")
                    {
                        WriteLine($"{persId} {custCode} {depId} {depName}");
                    }
                    else if (input == "cls")
                    {
                        Console.Clear();
                    }
                    else if (input == "help")
                    {
                        WriteLine("Type 'register' to sign up, 'unregister' to unregister 'login' to sign in, 'logout' to sign out, " +
                            "'props' to show the current properties, 'cls' to clear the screen, 'exit' to close program.");
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
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

        static void Logout()
        {
            hub.Invoke("Logout").Wait();
            WriteLine("Logged out.");
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
