using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkovSharp.TokenisationStrategies;
using System.IO;
using System.Text.RegularExpressions;

namespace sa_boosted_twitter_bot
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var lines = File.ReadAllLines("Boosted.txt");
            var markov = new StringMarkov();
            markov.Learn(lines);
            var result = markov.Walk().First();
            var splitLines = result.Split(new[] {"?", "!"}, StringSplitOptions.None).Where(s => !string.IsNullOrWhiteSpace(s));
            foreach (var line in splitLines)
            {
                var foo = Twitterize(line.Trim());
                if (foo.Length > 140)
                {
                    var charCount = 0;
                    var bar =
                        foo.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries)
                            .GroupBy(w => (charCount += w.Length + 1)/135)
                            .Select(g => string.Join(" ", g)).ToList();
                    for (var i = 0; i < bar.Count(); i++)
                    {
                        var totalCount = i + 1;
                        bar[i] = bar[i] + $" /{totalCount}";
                        Console.WriteLine($"{FirstLetterToUpper(bar[i])} - {bar[i].Length}");
                    }
                }
                else
                {
                    Console.WriteLine($"{FirstLetterToUpper(foo)} - {foo.Length}");
                }
            }
            Console.ReadLine();
        }

        static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        static string Twitterize(string line)
        {
            var newline = line.ToLower();
            newline = ReplaceTrump(newline);
            newline = ReplaceClinton(newline);
            newline = ReplaceCruz(newline);
            newline = ReplaceBush(newline);
            newline = ReplaceMarco(newline);
            if (newline.Length <= 75)
            {
                newline = AddHashtags(newline);
            }
            return newline;
        }

        static string AddHashtags(string line)
        {
            // TODO: Add random hashtags
            line = line + " #tcot";
            return line;
        }

        static string ReplaceCruz(string line)
        {
            Regex r = new Regex(string.Join("|", Cruz.Select(Regex.Escape).ToArray()));
            return r.Replace(line, "@tedcruz");
        }

        static string ReplaceMarco(string line)
        {
            Regex r = new Regex(string.Join("|", Cruz.Select(Regex.Escape).ToArray()));
            return r.Replace(line, "@marcorubio");
        }

        static string ReplaceBush(string line)
        {
            Regex r = new Regex(string.Join("|", Cruz.Select(Regex.Escape).ToArray()));
            return r.Replace(line, "@JebBush");
        }

        static string ReplaceClinton(string line)
        {
            Regex r = new Regex(string.Join("|", Clinton.Select(Regex.Escape).ToArray()));
            return r.Replace(line, "@HillaryClinton");
        }

        static string ReplaceTrump(string line)
        {
            Regex r = new Regex(string.Join("|", Trump.Select(Regex.Escape).ToArray()));
            return r.Replace(line, "@realDonaldTrump");
        }

        static string[] Rubio = new[] { "marco" };

        static string[] Bush = new[] { "jeb!", "jeb" };

        static string[] Cruz = new[] { "cruz" };

        static string[] Clinton = new[] { "hillary clinton" };

        static string[] Trump = new[] {"donald j. trump", "donald trump", "donald j trump" };
    }
}
