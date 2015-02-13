using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Exceptions;
using AwfulForumsLibrary.Manager;
using AwfulForumsLibrary.Tools;
using AuthenticationManager = AwfulForumsLibrary.Manager.AuthenticationManager;

namespace Awful_Bookmarks_Console
{
    class Program
    {
        private static Timer aTimer;
        private static List<ForumThreadEntity> _bookmarkList = new List<ForumThreadEntity>();
        private static List<long> _selectedThreadids = new List<long>();
        private static AuthenticationManager _authManager = new AuthenticationManager();
        private static ThreadManager _threadManager = new ThreadManager();
        static void Main(string[] args)
        {
            Task t = MainAsync(args);
            t.Wait();
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Checking login state...");
            var localStorageManager = new LocalStorageManager();
            CookieContainer cookieTest = await localStorageManager.LoadCookie("SACookie2.txt");

            if (cookieTest.Count <= 0)
            {
                var result = await Login();
                if (!result)
                {
                    Console.Write("Failed to Login");
                    return;
                }
            }
            Console.WriteLine("Logged in!");
            Console.WriteLine("Getting Bookmarks...");
            await GetBookmarks();
            if (!_bookmarkList.Any())
            {
                Console.WriteLine("You have no bookmarks! Get some first!");
                return;
            }
            PrintBookmarkList();
            Console.WriteLine("Enter the bookmark numbers you want to be notified of, seperated by commas.");
            var list = Console.ReadLine();
            var intList = SelectBookmarks(list);
            SelectThreadIds(intList);
            if (!_selectedThreadids.Any())
            {
                Console.WriteLine("You did not select any bookmarks!");
                return;
            }
            aTimer = new Timer(10000);
            aTimer.Elapsed += OnTimedEvent;
            // Set the Interval to 2 seconds (2000 milliseconds).
            aTimer.Interval = 300000;
            aTimer.Enabled = true;

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        static async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            await GetNotifications();
        }

        static async Task GetNotifications()
        {
            await GetBookmarks();
            var selectedList = _bookmarkList.Where(node => _selectedThreadids.Contains(node.ThreadId));
            foreach (var thread in selectedList.Where(thread => thread.RepliesSinceLastOpened > 0))
            {
                var stringFormat = thread.RepliesSinceLastOpened == 1 ? "\"{0}\" has {1} unread reply." : "\"{0}\" has {1} unread replies.";
                Console.WriteLine(stringFormat, thread.Name, thread.RepliesSinceLastOpened);
            }
        }

        static void SelectThreadIds(IEnumerable<int> intList)
        {
            foreach (var realNumber in intList.Select(item => item - 1).
                Where(realNumber => realNumber >= 0 
                && realNumber <= _bookmarkList.Count))
            {
                _selectedThreadids.Add(_bookmarkList[realNumber].ThreadId);
            }
        }

        static List<int> SelectBookmarks(string list)
        {
            var intList = new List<int>();
            var parsedList = list.Split(',');
            foreach (var item in parsedList)
            {
                try
                {
                    intList.Add(Convert.ToInt32(item));
                }
                catch (Exception)
                {
                    // Ignore. The user inputed something dumb and it's a dumb app and who cares.
                }
            }
            return intList;
        } 

        static void PrintBookmarkList()
        {
            for (var i = 0; i < _bookmarkList.Count; i++)
            {
                var realNumber = i + 1;
                Console.WriteLine("{0}: {1}", realNumber, _bookmarkList[i].Name);
            }
        }

        static async Task GetBookmarks()
        {
            _bookmarkList = new List<ForumThreadEntity>();
            var forum = new ForumEntity()
            {
                Name = "Bookmarks",
                IsBookmarks = true,
                IsSubforum = false,
                Location = Constants.UserCp
            };
            var pageNumber = 1;
            var hasItems = false;
            while (!hasItems)
            {
                var bookmarks = await _threadManager.GetBookmarksAsync(forum, pageNumber);
                _bookmarkList.AddRange(bookmarks);
                if (bookmarks.Any())
                {
                    hasItems = true;
                }
                else
                {
                    pageNumber++;
                }
            }
        }

        static async Task<bool> Login()
        {
            Console.Write("Enter your username: ");
            var username = Console.ReadLine();
            if (string.IsNullOrEmpty(username))
                return false;
            Console.Write("Enter your password: ");
            var password = GetPassword();
            if (string.IsNullOrEmpty(password.ToString()))
            {
                return false;
            }
            Console.WriteLine("Logging in...");
            try
            {
                return await _authManager.Authenticate(username, password.ToString());
            }
            catch (LoginFailedException ex)
            {
                return false;
            }
        }

        static StringBuilder GetPassword()
        {
            StringBuilder pwd = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length <= 0) continue;
                    pwd.Remove(pwd.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else
                {
                    pwd.Append(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }

    }
}
