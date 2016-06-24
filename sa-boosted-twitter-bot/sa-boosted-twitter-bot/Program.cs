using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkovSharp.TokenisationStrategies;
using System.IO;
using System.Text.RegularExpressions;
using CoreTweet;

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
            var apiKeys = File.ReadAllLines("api.txt").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            string apiKey, apiSecret, accessToken, accessTokenSecret = "";
            Tokens tokens = null;
            if (apiKeys.Count > 0)
            {
                apiKey = apiKeys[0];
                apiSecret = apiKeys[1];
                accessToken = apiKeys[2];
                accessTokenSecret = apiKeys[3];
                tokens = CoreTweet.Tokens.Create(apiKey, apiSecret, accessToken, accessTokenSecret);
            }

            var lines = File.ReadAllLines("Boosted.txt");
            var markov = new StringMarkov();
            markov.Learn(lines);
            var result = markov.Walk().First();
            var splitLines = result.Split(new[] {"?", "!"}, StringSplitOptions.None).Where(s => !string.IsNullOrWhiteSpace(s));
            try
            {
                foreach (var line in splitLines)
                {
                    var foo = Twitterize(line.Trim());
                    if (foo.Length > 140)
                    {
                        var charCount = 0;
                        var bar =
                            foo.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                                .GroupBy(w => (charCount += w.Length + 1) / 115)
                                .Select(g => string.Join(" ", g)).ToList();
                        for (var i = 0; i < bar.Count(); i++)
                        {
                            var totalCount = i + 1;
                            bar[i] = bar[i] + $" /{totalCount}";
                            if (tokens != null)
                            {
                                TweetLine(tokens, bar[i]);
                            }
                            Console.WriteLine($"{bar[i]} - {bar[i].Length}");
                        }
                    }
                    else
                    {
                        if (tokens != null)
                        {
                            TweetLine(tokens, foo);
                        }
                        Console.WriteLine($"{foo} - {foo.Length}");
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            if (tokens == null)
            {
                Console.ReadLine();
            }
        }

        static void TweetLine(Tokens tokens, string line)
        {
            Console.WriteLine(line.Count());
            tokens.Statuses.Update(status => line);
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
            Regex r = new Regex(string.Join("|", Rubio.Select(Regex.Escape).ToArray()));
            return r.Replace(line, "@marcorubio");
        }

        static string ReplaceBush(string line)
        {
            Regex r = new Regex(string.Join("|", Bush.Select(Regex.Escape).ToArray()));
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
