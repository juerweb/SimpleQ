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
                    if (input == "login")
                    {
                        Login();
                    }
                    else if (input == "register")
                    {

                    }
                    else if (input == "cls")
                    {
                        Console.Clear();
                    }
                    else if (input == "help")
                    {
                        WriteLine("Type 'login' to sign in, 'register' to sign up, 'cls' to clear the screen, 'exit' to close program.");
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

        static void Login()
        {
            WriteLine("E-Mail:");
            string email = ReadLine();
            WriteLine("Password:");
            string pwd = ReadLine();
            WriteLine("CustName:");
            string custName = ReadLine();
            WriteLine("Type 'commit' to login, 'abort' for cancelling");
            string action;
            while ((action = ReadLine()) != "commit" && (action = ReadLine()) != "abort") ;

            if (action == "commit")
            {
                Task<OperationStatus> task = hub.Invoke<OperationStatus>("Login", email, pwd, custName);
                task.Wait();
                WriteLine(task.Result);
            }
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
