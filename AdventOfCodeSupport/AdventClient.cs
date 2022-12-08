using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace AdventOfCodeSupport;

internal class AdventClient
{
    public record AdventAnswers(string? Part1, string? Part2);
    
    private static HttpClient? _client;

    private readonly Exception _badClient =
        new("Cannot download inputs/submit answers, user secret \"session\" has not been set.");

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

        var version = new ProductInfoHeaderValue("AdventOfCodeSupport", "2.0.0-alpha.3");
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

    public async Task<AdventAnswers> DownloadAnswersAsync(IAoC day)
    {
        if (_client is null) throw _badClient;
        var regex = new Regex(@"Your puzzle answer was <code>(.+)<\/code>");
        Console.WriteLine($"Downloading answers {day.Year}-{day.Day}...");
        var result = await _client.GetAsync($"{day.Year}/day/{day.Day}");
        if (!result.IsSuccessStatusCode)
            throw new Exception($"Downloading answers {day.Year}-{day.Day} failed. {result.ReasonPhrase}");
        var html = await result.Content.ReadAsStringAsync();
        await Task.Delay(1000); // Rate limit.
        var matches = regex.Matches(html);
        return matches.Count switch
        {
            1 => new AdventAnswers(matches[0].Groups[1].Value, null),
            2 => new AdventAnswers(matches[0].Groups[1].Value, matches[1].Groups[1].Value),
            _ => new AdventAnswers(null, null)
        };
    }

    public async Task<bool> SubmitAnswerAsync(IAoC day, int part, string submission)
    {
        if (_client is null) throw _badClient;
        Console.WriteLine($"Submit Part {part} answer? (y/n):\n{submission}");
        var choice = Console.ReadLine()?.Trim().ToLower();
        if (choice != "y") return false;
        HttpContent content = new StringContent($"level={part}&answer={submission}");
        content.Headers.Clear();
        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
        var result = await _client.PostAsync($"{day.Year}/day/{day.Day}/answer", content);
        if (!result.IsSuccessStatusCode)
            throw new Exception($"Submission failed. {result.ReasonPhrase}");
        var html = await result.Content.ReadAsStringAsync();
        var regexGoldStar = new Regex("gold star");
        var regexIncorrect = new Regex("not the right answer");
        var regexTooLow = new Regex("answer is too low");
        var regexTooHigh = new Regex("answer is too high");
        var waitBefore = new Regex("wait.+before");
        if (regexGoldStar.IsMatch(html))
        {
            Console.WriteLine("You got a star!");
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
        
        await Task.Delay(2000); // Rate limit.
        return false;
    }
}
