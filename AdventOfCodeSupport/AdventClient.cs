using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace AdventOfCodeSupport;

internal partial class AdventClient
{
    private readonly AdventSolutions _adventSolutions;
    private readonly AdventBase _calledBy;

    public class AdventAnswers
    {
        public string? Part1 { get; set; }
        public string? Part2 { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    private static HttpClient? _adventClient;

    private readonly Exception _badClient =
        new("Cannot download inputs/submit answers, user secret \"session\" has not been set, check readme.");

    private static Dictionary<string, AdventAnswers>? _savedAnswers;
    private static Dictionary<string, AdventAnswers> SavedAnswers
    {
        get
        {
            if (_savedAnswers is not null) return _savedAnswers;
            if (!File.Exists(Path.Combine(AdventBase.ProjectRoot, "Saved.json")))
            {
                _savedAnswers = new Dictionary<string, AdventAnswers>();
                return _savedAnswers;
            }

            var text = File.ReadAllText(Path.Combine(AdventBase.ProjectRoot, "Saved.json"));
            _savedAnswers = JsonSerializer.Deserialize<Dictionary<string, AdventAnswers>>(text);
            return _savedAnswers ??= new Dictionary<string, AdventAnswers>();
        }
    }

    internal AdventClient(AdventSolutions adventSolutions, AdventBase adventBase)
    {
        _adventSolutions = adventSolutions;
        _calledBy = adventBase;
        if (_adventClient is not null) return;
        var builder = new ConfigurationBuilder();
        builder.AddUserSecrets(Assembly.GetEntryAssembly()!);
        var config = builder.Build();
        var cookie = config["session"];
        if (string.IsNullOrWhiteSpace(cookie)) return;

        var handler = new HttpClientHandler { UseCookies = false };
        _adventClient = new HttpClient(handler) { BaseAddress = new Uri("https://adventofcode.com/") };

        var version = new ProductInfoHeaderValue("AdventOfCodeSupport", "2.5.1");
        var comment = new ProductInfoHeaderValue("(+nuget.org/packages/AdventOfCodeSupport by @Zaneris)");
        _adventClient.DefaultRequestHeaders.UserAgent.Add(version);
        _adventClient.DefaultRequestHeaders.UserAgent.Add(comment);
        _adventClient.DefaultRequestHeaders.Add("cookie", $"session={cookie.Trim()}");
    }

    public async Task DownloadInputAsync(AdventBase day)
    {
        if (_adventClient is null) throw _badClient;
        var inputPattern = _adventSolutions.InputPattern;
        inputPattern = inputPattern.Replace("yyyy", $"{day.Year}");
        inputPattern = inputPattern.Replace("dd", $"{day.Day:D2}");
        var path = Path.Combine(AdventBase.ProjectRoot, inputPattern);
        if (File.Exists(path)) return;
        Console.WriteLine($"Downloading input {day.Year}-{day.Day}...");
        var directory = Path.GetDirectoryName(path);
        Directory.CreateDirectory($"{directory}");
        var result = await _adventClient.GetAsync($"{day.Year}/day/{day.Day}/input");
        if (!result.IsSuccessStatusCode)
            throw new Exception($"Input download {day.Year}-{day.Day} failed. {result.ReasonPhrase}");

        var text = await result.Content.ReadAsStringAsync();
        await File.WriteAllTextAsync(path, text);
        await Task.Delay(1000); // Rate limit input downloads.
    }

    private static async Task SaveAnswersAsync()
    {
        var text = JsonSerializer.Serialize(SavedAnswers);
        await File.WriteAllTextAsync($"{AdventBase.ProjectRoot}/Saved.json", text);
    }

    public async Task<AdventAnswers> DownloadAnswersAsync(AdventBase day, string? testHtml)
    {
        string html;
        if (testHtml is null) // No need to download if using test HTML.
        {
            if (SavedAnswers.TryGetValue($"{day.Year}-{day.Day}", out var saved))
            {
                if (saved.Part2 is not null) return saved;
                var diff = DateTime.UtcNow - saved.Timestamp;

                // Rate limit checking for new answers to every 15 minutes.
                if (diff <= new TimeSpan(0, 15, 0)) return saved;
            }
            if (_adventClient is null) throw _badClient;
            Console.WriteLine($"Downloading your already submitted answers {day.Year}-{day.Day}...");
            var result = await _adventClient.GetAsync($"{day.Year}/day/{day.Day}");
            if (!result.IsSuccessStatusCode)
                throw new Exception($"Downloading answers {day.Year}-{day.Day} failed. {result.ReasonPhrase}");
            html = await result.Content.ReadAsStringAsync();
            await Task.Delay(1000); // Rate limit.
        }
        else html = testHtml;

        var matches = RegexAnswer().Matches(html);
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
        if (testHtml is null) await SaveAnswersAsync(); // Skip saving during unit tests.
        return answer;
    }

    public async Task<bool> SubmitAnswerAsync(AdventBase day, int part, string submission, string? testHtml)
    {
        var saved = SavedAnswers[$"{day.Year}-{day.Day}"];

        string html;
        if (testHtml is null) // No need to download if using test HTML.
        {
            if (_adventClient is null) throw _badClient;
            Console.WriteLine($"Submit Part {part} answer? (y/n):\n{submission}");
            var choice = Console.ReadLine()?.Trim().ToLower();
            if (choice != "y") return false;
            HttpContent content = new StringContent($"level={part}&answer={submission}");
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            var result = await _adventClient.PostAsync($"{day.Year}/day/{day.Day}/answer", content);
            if (!result.IsSuccessStatusCode)
                throw new Exception($"Submission failed. {result.ReasonPhrase}");
            html = await result.Content.ReadAsStringAsync();
        }
        else html = testHtml;

        if (RegexGoldStar().IsMatch(html))
        {
            Console.WriteLine("You got a star!");
            if (part == 1) saved.Part1 = submission;
            else saved.Part2 = submission;
            saved.Timestamp = DateTime.UtcNow;
            if (testHtml is null) await SaveAnswersAsync();
            return true;
        }
        if (RegexIncorrect().IsMatch(html))
            Console.WriteLine("That's not the right answer.");
        if (RegexTooHigh().IsMatch(html))
            Console.WriteLine("Your answer is too high.");
        if (RegexTooLow().IsMatch(html))
            Console.WriteLine("Your answer is too low.");
        var match = RegexWaitBefore().Match(html);
        if (match.Success)
            Console.WriteLine($"Please {match.Captures[0].Value} submitting again.");

        return false;
    }

    private string? LocateClassText()
    {
        if (_adventSolutions.ClassNamePattern is null)
        {
            var directories = Directory.EnumerateDirectories(AdventBase.ProjectRoot, $"*{_calledBy.Year}*", SearchOption.AllDirectories);
            var directoryPath = directories.FirstOrDefault(); // Take the first match
            if (directoryPath is null) return null;
            var files = Directory.EnumerateFiles(directoryPath, $"{_calledBy.ClassName}.cs", SearchOption.AllDirectories);
            var filePath = files.FirstOrDefault();
            if (filePath is null) return null;
            return File.ReadAllText(filePath);
        }
        else
        {
            var className = _adventSolutions.ClassNamePattern;
            className = className.Replace("yyyy", $"{_calledBy.Year}");
            className = className.Replace("dd", $"{_calledBy.Day:D2}");
            className = $"{className}.cs";
            var files = Directory.EnumerateFiles(AdventBase.ProjectRoot, className, SearchOption.AllDirectories);
            var filePath = files.FirstOrDefault();
            if (filePath is null) return null;
            return File.ReadAllText(filePath);
        }
    }

    [GeneratedRegex("gold star")]
    private static partial Regex RegexGoldStar();
    [GeneratedRegex("not the right answer")]
    private static partial Regex RegexIncorrect();
    [GeneratedRegex("answer is too low")]
    private static partial Regex RegexTooLow();
    [GeneratedRegex("answer is too high")]
    private static partial Regex RegexTooHigh();
    [GeneratedRegex("wait.+before")]
    private static partial Regex RegexWaitBefore();
    [GeneratedRegex(@"Your puzzle answer was <code>(.+)<\/code>")]
    private static partial Regex RegexAnswer();
    [GeneratedRegex("""<article class="day-desc">[\S\s]+<\/article>""")]
    private static partial Regex RegexPuzzleProblem();
}
