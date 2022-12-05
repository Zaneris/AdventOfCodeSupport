using System.Collections;
using System.Net.Http.Headers;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Configuration;

namespace AdventOfCodeSupport;

/// <summary>
/// Automatically collects all solution classes derived from AdventBase.
/// </summary>
public class AdventSolutions : IEnumerable<IAoC>
{
    private readonly List<IAoC> _list = new();
    private readonly IConfiguration _config;

    /// <summary>
    /// Create a new automatically generated collection of AoC solutions.
    /// </summary>
    public AdventSolutions()
    {
        var baseType = typeof(AdventBase);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => baseType.IsAssignableFrom(p) && !p.IsAbstract);
        foreach (var type in types)
        {
            var newInstance = (AdventBase)Activator.CreateInstance(type)!;
            _list.Add(newInstance);
        }
        var builder = new ConfigurationBuilder();
        builder.AddUserSecrets(Assembly.GetEntryAssembly()!);
        _config = builder.Build();
    }

    /// <summary>
    /// Enumerator for AoC solution days collected.
    /// </summary>
    /// <returns>Enumerator for AoC solution days collected.</returns>
    public IEnumerator<IAoC> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Get a solution day with the provided year and day.
    /// </summary>
    /// <param name="year">Year.</param>
    /// <param name="day">Day.</param>
    /// <returns>Matching solution.</returns>
    /// <exception cref="Exception">Not found.</exception>
    public IAoC GetDay(int year, int day)
    {
        var result = _list.FirstOrDefault(x => x.Year == year && x.Day == day);
        if (result is null) throw new Exception(@$"Solution for Year: {year}, Day {day} was not found.
Please ensure constructor calls : base({year}, {day}).");
        return result;
    }

    /// <summary>
    /// Get the most recent AoC solution from the collection.
    /// If year is not specified, the most recent day of the most
    /// recent year is returned.
    /// </summary>
    /// <param name="year">Optional year.</param>
    /// <returns>Most recent solution.</returns>
    /// <exception cref="Exception">Collection empty.</exception>
    public IAoC GetMostRecentDay(int? year = null)
    {
        var result = year is null
            ? _list
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Day)
                .FirstOrDefault()
            : _list
                .Where(x => x.Year == year)
                .MaxBy(x => x.Day);
        if (result is null) throw new Exception(@"No solutions found.
Please ensure constructor calls : base(year, day).");
        return result;
    }

    /// <summary>
    /// Benchmark all solutions.
    /// </summary>
    /// <param name="year">Optional year.</param>
    /// <param name="config">Optional BenchmarkDotNet config.</param>
    public void BenchmarkAll(int? year = null, IConfig? config = null)
    {
        var types = year is null
            ? _list.Select(x => x.GetType()).ToArray()
            : _list.Where(x => x.Year == year).Select(x => x.GetType()).ToArray();
        var summaries = BenchmarkRunner.Run(types, config);
        Console.WriteLine($"Results saved to: {summaries[0].ResultsDirectoryPath}");
    }

    /// <summary>
    /// Download any inputs not already downloaded.
    /// Rate limited to 1 per second.
    /// </summary>
    /// <exception cref="Exception">Secret not set or request failure.</exception>
    public async Task DownloadInputsAsync()
    {
        var cookie = _config["session"];
        if (string.IsNullOrWhiteSpace(cookie))
            throw new Exception("Cannot download inputs, user secret \"session\" has not been set.");

        using var handler = new HttpClientHandler { UseCookies = false };
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://www.adventofcode.com/") };

        var version = new ProductInfoHeaderValue("AdventOfCodeSupport", "1.4.0");
        var comment = new ProductInfoHeaderValue("(+nuget.org/packages/AdventOfCodeSupport by @Zaneris)");
        client.DefaultRequestHeaders.UserAgent.Add(version);
        client.DefaultRequestHeaders.UserAgent.Add(comment);
        client.DefaultRequestHeaders.Add("cookie", $"session={cookie}");

        foreach (var day in _list)
        {
            var path = $"../../../{day.Year}/Inputs/{day.Day.ToString("D2")}.txt";
            if (File.Exists(path)) continue;
            Console.WriteLine($"Downloading input {day.Year}-{day.Day}...");
            Directory.CreateDirectory($"../../../{day.Year}/Inputs/");
            var result = await client.GetAsync($"{day.Year}/day/{day.Day}/input");
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Input download {day.Year}-{day.Day} failed. {result.ReasonPhrase}");
            }

            var text = await result.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(path, text);
            await Task.Delay(1000); // Rate limit input downloads.
        }
    }
}
