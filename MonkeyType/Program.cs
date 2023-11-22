using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

public class UserData
{
    public string Name { get; set; }
    public int WordsPerMinute { get; set; }
    public double Accuracy { get; set; }
}

public static class Leaderboard
{
    private const string LeaderboardFileName = "leaderboard.json";
    private static List<UserData> leaderboard;
    private static readonly object leaderboardLock = new object();

    static Leaderboard()
    {
        leaderboard = LoadLeaderboard();
    }

    public static void AddUser(UserData user)
    {
        lock (leaderboardLock)
        {
            leaderboard.Add(user);
            SaveLeaderboard();
        }
    }

    public static void DisplayLeaderboard()
    {
        lock (leaderboardLock)
        {
            Console.WriteLine("Leaderboard:");
            foreach (var user in leaderboard.OrderBy(u => u.WordsPerMinute))
            {
                Console.WriteLine($"{user.Name}: {user.WordsPerMinute} WPM, Accuracy: {user.Accuracy:F2}%");
            }
        }
    }

    private static void SaveLeaderboard()
    {
        lock (leaderboardLock)
        {
            File.WriteAllText(LeaderboardFileName, JsonConvert.SerializeObject(leaderboard));
        }
    }

    private static List<UserData> LoadLeaderboard()
    {
        if (File.Exists(LeaderboardFileName))
        {
            return JsonConvert.DeserializeObject<List<UserData>>(File.ReadAllText(LeaderboardFileName));
        }
        return new List<UserData>();
    }
}

public class TypingTest
{
    private static readonly List<string> SampleTexts = new List<string>
    {
        "фильм мозайка стойка вид в у но короб.",
        "дом дверь текст машина жизнь ноги вахта.",
        "карл украл у клары коралы клара украла у карла кларнет",
        "Шла саша по шоссе и сосала сушку"
    };

    public static void StartTest(string userName)
    {
        Console.WriteLine($"Welcome, {userName}! Press any key to start typing test.");
        Console.ReadKey(true);
        Console.Clear();

        string textToType = GetRandomText();
        Console.WriteLine($"Type the following text:\n{textToType}");

        var typedText = new StringBuilder();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        ConsoleKeyInfo keyInfo;
        do
        {
            keyInfo = Console.ReadKey(true);
            if (!char.IsControl(keyInfo.KeyChar))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(keyInfo.KeyChar);
                Console.ResetColor();
                typedText.Append(keyInfo.KeyChar);
            }
        } while (keyInfo.Key != ConsoleKey.Enter);

        stopwatch.Stop();
        Console.WriteLine(); // New line after Enter

        CalculateResults(userName, textToType, typedText.ToString(), stopwatch.Elapsed.TotalSeconds);
    }

    private static string GetRandomText()
    {
        Random random = new Random();
        int index = random.Next(SampleTexts.Count);
        return SampleTexts[index];
    }

    private static void CalculateResults(string userName, string originalText, string typedText, double elapsedTime)
    {
        int wordsTyped = typedText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        int wordsPerMinute = (int)(wordsTyped / (elapsedTime / 60));

        Console.WriteLine($"Results for {userName}:");
        Console.WriteLine($"Words per minute: {wordsPerMinute} WPM");

        int correctCharacters = originalText.Where((c, index) => index < typedText.Length && c == typedText[index]).Count();
        double accuracy = (double)correctCharacters / originalText.Length * 100;

        Console.WriteLine($"Accuracy: {accuracy:F2}%");

        Leaderboard.AddUser(new UserData
        {
            Name = userName,
            WordsPerMinute = wordsPerMinute,
            Accuracy = accuracy
        });

        Leaderboard.DisplayLeaderboard();
    }
}

class Program
{
    [STAThread]
    static void Main()
    {
        Console.Write("Enter your name: ");
        string userName = Console.ReadLine();

        TypingTest.StartTest(userName);

        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();
    }
}
