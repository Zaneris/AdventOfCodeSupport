using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    private static HttpClient? _gptClient;

    private static readonly JsonObject _gptBaseRequest = new()
    {
        ["model"] = "gpt-4-1106-preview",
        ["messages"] = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "system",
                ["content"] = "You are an expert at Advent of Code puzzles, you love to explain things using just code and code comments, english is a burden."
            }
        }
    };

    private const string GPT_HELP_ME = "I'm using C#/.NET to complete Advent of Code puzzles, I've provided the puzzle question for the day along with my attempt at the solution, I've received an incorrect response from Advent of Code for Part {num}, what could be wrong with Part {num}? Answer as briefly as possible, use code with comments.";

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

    public AdventClient(AdventSolutions adventSolutions, AdventBase adventBase)
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

        var version = new ProductInfoHeaderValue("AdventOfCodeSupport", "2.3.0");
        var comment = new ProductInfoHeaderValue("(+nuget.org/packages/AdventOfCodeSupport by @Zaneris)");
        _adventClient.DefaultRequestHeaders.UserAgent.Add(version);
        _adventClient.DefaultRequestHeaders.UserAgent.Add(comment);
        _adventClient.DefaultRequestHeaders.Add("cookie", $"session={cookie.Trim()}");

        // Configure ChatGPT client;
        if (_gptClient is not null) return;
        var secret = config["secret"];
        if (string.IsNullOrWhiteSpace(secret)) return;
        _gptClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/v1/"),
            DefaultRequestHeaders = {{"Authorization", $"Bearer {secret.Trim()}"}}
        };
    }

    public async Task DownloadInputAsync(AdventBase day)
    {
        if (_adventClient is null) throw _badClient;
        var inputPattern = _adventSolutions.InputPattern;
        inputPattern = inputPattern.Replace("yyyy", $"{day.Year}");
        inputPattern = inputPattern.Replace("dd", $"{day.Day:D2}");
        var path = $"../../../{inputPattern}";
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
        await File.WriteAllTextAsync("../../../Saved.json", text);
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

        if (_gptClient is null)
        {
            Console.WriteLine("Set dotnet user-secret \"secret\" with an OpenAI API key secret for auto ChatGPT help.");
            if (testHtml is null) await Task.Delay(2000); // Rate limit.
            return false;
        }

        var actualRequest = _gptBaseRequest.DeepClone();
        var helpMe = GPT_HELP_ME.Replace("{num}", part.ToString());
        var response = await _adventClient!.GetAsync($"{day.Year}/day/{day.Day}");
        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to download puzzle question to pass to ChatGPT");
        var problem = await response.Content.ReadAsStringAsync();
        problem = RegexPuzzleProblem().Match(problem).Value;
        var classText = LocateClassText();
        if (classText is null)
            throw new Exception($"Unable to locate .cs file for {_calledBy.Year}-{_calledBy.Day}");
        actualRequest["messages"]!.AsArray().Add(new JsonObject
        {
            ["role"] = "user",
            ["content"] = $"{helpMe}\n{problem}\n{classText}"
        });
        Console.WriteLine("Incorrect answer, asking ChatGPT for help...");
        response = await _gptClient.PostAsJsonAsync("chat/completions", actualRequest);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to retrieve response from ChatGPT");
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        if (json is not null && json.ContainsKey("choices") && json["choices"]!.AsArray().Count > 0)
        {
            var fix = json["choices"]![0]!["message"]!["content"]!.ToString();
            Console.WriteLine(fix);
        }
        else throw new Exception("Unexpected response from ChatGPT");

        // TODO: Cache puzzle questions

        return false;
    }

    private string? LocateClassText()
    {
        if (_adventSolutions.ClassNamePattern is null)
        {
            var directories = Directory.EnumerateDirectories("../../../", $"*{_calledBy.Year}*", SearchOption.AllDirectories);
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
            var files = Directory.EnumerateFiles("../../../", className, SearchOption.AllDirectories);
            // TODO: Replace all ../../../ with a project root constant
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
