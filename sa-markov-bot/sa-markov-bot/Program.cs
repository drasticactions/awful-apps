using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkovSharp.TokenisationStrategies;
using Newtonsoft.Json;
using System.IO;

namespace sa_markov_bot
{
    class Program
    {
        static void Main(string[] args)
        {
           MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var path = args.FirstOrDefault();
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Enter File Path");
                path = Console.ReadLine();
            }
            var lines = File.ReadAllLines(path);
            var markov = new StringMarkov();
            markov.Learn(lines);
            var newLines = markov.Walk(10).ToList();
            for (var i = 0; i < newLines.Count(); i++)
            {
                Console.WriteLine(newLines[i]);
            }
            Console.ReadLine();
        }
    }
}
