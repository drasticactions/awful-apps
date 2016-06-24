using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwfulRedux.Core.Managers;
using AwfulRedux.Core.Models.Posts;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;

namespace sa_post_collector
{
    class Program
    {
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            string username;
            string password;
            List<string> info = File.ReadAllLines("users.txt").ToList();
            if (info.Any())
            {
                username = info[0];
                password = info[1];
            }
            else
            {
                Console.WriteLine("Enter your Something Awful Username: ");
                username = Console.ReadLine();
                Console.WriteLine("Enter your SA Password: ");
                password = ReadPassword();
            }
            Console.WriteLine("Logging in...");
            var authenticationManager = new AuthenticationManager();
            var result = await authenticationManager.AuthenticateAsync(username, password);
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error logging in: {result.Error}");
                return;
            }

            var webManager = new WebManager(result.AuthenticationCookie);
            var postManager = new PostManager(webManager);

            string threadidstring;
            string useridstring;

            if (info.Any())
            {
                threadidstring = info[2];
                useridstring = info[3];
            }
            else
            {
                Console.WriteLine("Enter the SA Thread ID: ");
                threadidstring = Console.ReadLine();
                Console.WriteLine("Enter the SA User ID: ");
                useridstring = Console.ReadLine();
            }


            long threadId = 0;
            try
            {
                threadId = Convert.ToInt64(threadidstring);
            }
            catch (Exception)
            {
                Console.WriteLine("Error: Bad Thread ID");
                return;
            }


            int userid = 0;
            try
            {
                userid = Convert.ToInt32(useridstring);
            }
            catch (Exception)
            {
                Console.WriteLine("Error: Bad Thread ID");
                return;
            }

            // TODO: Make a real "GetThreadInfo" method, this is _lame_.
            var threadInfo =
                await
                    postManager.GetUsersPostsInThreadAsync(
                        $"https://forums.somethingawful.com/showthread.php?threadid={threadId}", userid, 1, false, false, true);
            var posts = JsonConvert.DeserializeObject<ThreadPosts>(threadInfo.ResultJson);
            var postername = posts.Posts.FirstOrDefault()?.User.Username;
            var totalPages = posts.ForumThread.TotalPages;
            Console.WriteLine($"Getting posts for {postername} in {posts.ForumThread.Name}. Total Pages: {totalPages}");
            for (var i = 1; i <= totalPages; i++)
            {
                Console.WriteLine($"Loading page {i}");
                var postsJson2 =
                await
                    postManager.GetUsersPostsInThreadAsync(
                        $"https://forums.somethingawful.com/showthread.php?threadid={threadId}", userid, i, false, false, true);
                var posts2 = JsonConvert.DeserializeObject<ThreadPosts>(postsJson2.ResultJson);
                var postElements = posts2.Posts.Select(node => node.PostElements);
                var innerTextPosts = postElements.Select(node => node.InnerText);
                File.AppendAllLines($"{postername} - {Regex.Replace(posts.ForumThread.Name, "[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]", "_")}.txt", innerTextPosts);
            }
            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        // From http://stackoverflow.com/questions/29201697/hide-replace-when-typing-a-password-c
        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
    }
}
