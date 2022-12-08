using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace AdventOfCodeSupport;

internal class AdventClient
{
    public class AdventAnswers
    {
        public string? Part1 { get; set; }
        public string? Part2 { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    private static HttpClient? _client;

    private readonly Exception _badClient =
        new("Cannot download inputs/submit answers, user secret \"session\" has not been set, check readme.");

    private static Dictionary<string, AdventAnswers>? _savedAnswers;
    private static Dictionary<string, AdventAnswers> SavedAnswers
    {
        get
        {
            if (_savedAnswers is not null) return _savedAnswers;
            if (!File.Exists("../../../Saved.json"))
            {
                _savedAnswers = new Dictionary<string, AdventAnswers>();
                return _savedAnswers;
            }

            var text = File.ReadAllText("../../../Saved.json");
            _savedAnswers = JsonSerializer.Deserialize<Dictionary<string, AdventAnswers>>(text);
            return _savedAnswers ??= new Dictionary<string, AdventAnswers>();
        }
    }

    public AdventClient()
    {
        if (_client is not null) return;
        var builder = new ConfigurationBuilder();
        builder.AddUserSecrets(Assembly.GetEntryAssembly()!);
        var config = builder.Build();
        var cookie = config["session"];
        if (string.IsNullOrWhiteSpace(cookie)) throw _badClient;

        var handler = new HttpClientHandler { UseCookies = false };
        _client = new HttpClient(handler) { BaseAddress = new Uri("https://adventofcode.com/") };

        var version = new ProductInfoHeaderValue("AdventOfCodeSupport", "2.0.0-alpha.4");
        var comment = new ProductInfoHeaderValue("(+nuget.org/packages/AdventOfCodeSupport by @Zaneris)");
        _client.DefaultRequestHeaders.UserAgent.Add(version);
        _client.DefaultRequestHeaders.UserAgent.Add(comment);
        _client.DefaultRequestHeaders.Add("cookie", $"session={cookie}");
    }

    public async Task DownloadInputAsync(IAoC day)
    {
        if (_client is null) throw _badClient;
        var path = $"../../../{day.Year}/Inputs/{day.Day:D2}.txt";
        if (File.Exists(path)) return;
        Console.WriteLine($"Downloading input {day.Year}-{day.Day}...");
        Directory.CreateDirectory($"../../../{day.Year}/Inputs/");
        var result = await _client.GetAsync($"{day.Year}/day/{day.Day}/input");
        if (!result.IsSuccessStatusCode)
            throw new Exception($"Input download {day.Year}-{day.Day} failed. {result.ReasonPhrase}");

        var text = await result.Content.ReadAsStringAsync();
        await File.WriteAllTextAsync(path, text);
        await Task.Delay(1000); // Rate limit input downloads.
    }

    private static async Task SaveAnswersAsync()
    {
        var text = JsonSerializer.Serialize(SavedAnswers);
        await File.WriteAllTextAsync("../../../Saved.json", text);
    }

    public async Task<AdventAnswers> DownloadAnswersAsync(IAoC day, string? testHtml)
    {
        if (SavedAnswers.TryGetValue($"{day.Year}-{day.Day}", out var saved))
        {
            if (saved.Part2 is not null) return saved;
            var diff = DateTime.UtcNow - saved.Timestamp;

            // Rate limit checking for new answers to every 15 minutes.
            if (diff <= new TimeSpan(0, 15, 0)) return saved;
        }
        if (_client is null) throw _badClient;
        var regex = new Regex(@"Your puzzle answer was <code>(.+)<\/code>");
        Console.WriteLine($"Downloading your already submitted answers {day.Year}-{day.Day}...");
        var result = await _client.GetAsync($"{day.Year}/day/{day.Day}");
        if (!result.IsSuccessStatusCode)
            throw new Exception($"Downloading answers {day.Year}-{day.Day} failed. {result.ReasonPhrase}");
        var html = await result.Content.ReadAsStringAsync();
        if (testHtml is null) await Task.Delay(1000); // Rate limit.
        var matches = regex.Matches(html);
        var answer = matches.Count switch
        {
            1 => new AdventAnswers
            {
                Part1 = matches[0].Groups[1].Value
            },
            2 => new AdventAnswers
            {
                Part1 = matches[0].Groups[1].Value,
                Part2 = matches[1].Groups[1].Value
            },
            _ => new AdventAnswers()
        };
        SavedAnswers[$"{day.Year}-{day.Day}"] = answer;
        await SaveAnswersAsync();
        return answer;
    }

    public async Task<bool> SubmitAnswerAsync(IAoC day, int part, string submission, string? testHtml)
    {
        if (_client is null && testHtml is null) throw _badClient;
        var saved = SavedAnswers[$"{day.Year}-{day.Day}"];
        Console.WriteLine($"Submit Part {part} answer? (y/n):\n{submission}");
        var choice = Console.ReadLine()?.Trim().ToLower();
        if (choice != "y") return false;

        string html;
        if (testHtml is null)
        {
            HttpContent content = new StringContent($"level={part}&answer={submission}");
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            var result = await _client!.PostAsync($"{day.Year}/day/{day.Day}/answer", content);
            if (!result.IsSuccessStatusCode)
                throw new Exception($"Submission failed. {result.ReasonPhrase}");
            html = await result.Content.ReadAsStringAsync();
        }
        else html = testHtml;

        var regexGoldStar = new Regex("gold star");
        var regexIncorrect = new Regex("not the right answer");
        var regexTooLow = new Regex("answer is too low");
        var regexTooHigh = new Regex("answer is too high");
        var waitBefore = new Regex("wait.+before");
        if (regexGoldStar.IsMatch(html))
        {
            Console.WriteLine("You got a star!");
            if (part == 1) saved.Part1 = submission;
            else saved.Part2 = submission;
            saved.Timestamp = DateTime.UtcNow;
            await SaveAnswersAsync();
            return true;
        }
        if (regexIncorrect.IsMatch(html))
            Console.WriteLine("That's not the right answer.");
        if (regexTooHigh.IsMatch(html))
            Console.WriteLine("Your answer is too high.");
        if (regexTooLow.IsMatch(html))
            Console.WriteLine("Your answer is too low.");
        var match = waitBefore.Match(html);
        if (match.Success)
            Console.WriteLine($"Please {match.Captures[0].Value} submitting again.");

        if (testHtml is null) await Task.Delay(2000); // Rate limit.
        return false;
    }
}
