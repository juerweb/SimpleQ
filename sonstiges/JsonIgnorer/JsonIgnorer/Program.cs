using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonIgnorer
{
    class Program
    {
        static void Main(string[] args)
        {
            string input;
            while ((input = ReadLine()) != "exit")
            {
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                else if (input == "inputfile")
                    ProcessFile();
                else
                    AddAttributes(input);
            }
        }

        static void AddAttributes(string path)
        {
            try
            {
                List<string> lines = File.ReadAllLines(path).ToList();
                List<int> indexes = new List<int>();
                int count = 0;

                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains(" virtual "))
                        indexes.Add(i);
                }

                indexes.OrderByDescending(i => i).ToList().ForEach(i =>
                {
                    if (!lines[i - 1].Contains("Ignore]"))
                    {
                        lines.Insert(i, "        [Newtonsoft.Json.JsonIgnore]");
                        lines.Insert(i, "        [System.Web.Script.Serialization.ScriptIgnore(ApplyToOverrides = true)]");
                        count++;
                    }
                });

                File.WriteAllLines(path, lines);
                WriteLine($"{count} JsonIgnore attribute(s) added.");
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
            }
        }

        static void ProcessFile()
        {
            try
            {
                string[] files = File.ReadAllLines("inputfile.txt");
                files.ToList().ForEach(f => AddAttributes(f));
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
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
