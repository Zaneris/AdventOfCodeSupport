using System.Collections;
using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Configuration;

namespace AdventOfCodeSupport;

/// <summary>
/// Automatically collects all solution classes derived from AdventBase.
/// </summary>
public class AdventSolutions : IEnumerable<AdventBase>
{
    private readonly List<AdventBase> _list = [];

    internal IConfiguration Config { get; }

    /// <summary>
    /// The input pattern being used to load and save input files. Set with <see cref="ConfigureInputPattern"/>
    /// </summary>
    public static string InputPattern { get; private set; } = "yyyy/Inputs/dd.txt";

    /// <summary>
    /// Pattern used to pull day and year from class names, if <c>null</c> the default pattern of 4 digit year in the
    /// namespace and 2 digit day in the class name will be used. Set with <see cref="ConfigureClassNamePattern"/>
    /// </summary>
    public static string? ClassNamePattern { get; private set; } = null;

    /// <summary>
    /// Configure the pattern used for class names. <c>yyyy</c> is used to represent the 4 digit
    /// year, and <c>dd</c> the 2 digit day. If the class name does not contain the 4 digit year, it must be set in
    /// the namespace and this method can be ignored.
    /// </summary>
    /// <example>
    /// Here is an example for a class named 202301.cs
    /// <code>
    /// AdventSolutions.ConfigureClassNamePattern("yyyydd.cs");
    /// </code>
    /// </example>
    /// <param name="pattern">Must contain yyyy and dd. <c>null</c> will reset to the default.</param>
    /// <exception cref="Exception">Pattern must contain yyyy and dd</exception>
    public static void ConfigureClassNamePattern(string? pattern)
    {
        if (pattern is null)
        {
            ClassNamePattern = null;
            return;
        }
        pattern = pattern.Replace(".cs", "");
        if (!pattern.Contains("yyyy"))
            throw new Exception("Input pattern must contain yyyy to represent 4 digit year.");
        if (!pattern.Contains("dd"))
            throw new Exception("Input pattern must contain dd to represent 2 digit day.");
        pattern = pattern.Replace('\\', '/');
        if (pattern.Contains('/'))
            throw new Exception("Directory path is not required.");
        ClassNamePattern = pattern;
    }

    /// <summary>
    /// Configure the pattern used for saving and loading input files. <c>yyyy</c> is used to represent the 4 digit
    /// year, and <c>dd</c> the 2 digit day.
    /// </summary>
    /// <example>
    /// Here is an example that would load the 2023 input file for day 1 in file path 2023/Inputs/01.txt
    /// <code>
    /// solutions.ConfigureInputPattern("yyyy/Inputs/dd.txt");
    /// </code>
    /// </example>
    /// <param name="pattern">Must contain yyyy and dd</param>
    /// <returns>Self: <see cref="AdventSolutions"/></returns>
    /// <exception cref="Exception">Pattern must contain yyyy and dd</exception>
    public static void ConfigureInputPattern(string pattern)
    {
        if (!pattern.Contains("yyyy"))
            throw new Exception("Input pattern must contain yyyy to represent 4 digit year.");
        if (!pattern.Contains("dd"))
            throw new Exception("Input pattern must contain dd to represent 2 digit day.");
        pattern = pattern.Replace('\\', '/');
        if (pattern.EndsWith('/'))
            throw new Exception("Pattern must end with a filename, not a directory.");
        while (pattern.StartsWith('/'))
        {
            pattern = pattern.Substring(1, pattern.Length - 1);
        }

        InputPattern = pattern;
    }

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
        Config = builder.Build();
    }

    /// <summary>
    /// Enumerator for AoC solution days collected.
    /// </summary>
    /// <returns>Enumerator for AoC solution days collected.</returns>
    public IEnumerator<AdventBase> GetEnumerator()
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
    public AdventBase GetDay(int year, int day)
    {
        var result = _list.FirstOrDefault(x => x.Year == year && x.Day == day);
        if (result is null) throw new Exception($"""
                                                 Solution for Year: {year}, Day {day} was not found.
                                                 Please ensure constructor calls : base({year}, {day}).
                                                 """);
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
    public AdventBase GetMostRecentDay(int? year = null)
    {
        var result = year is null
            ? _list
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Day)
                .FirstOrDefault()
            : _list
                .Where(x => x.Year == year)
                .MaxBy(x => x.Day);
        if (result is null) throw new Exception("""
                                                No solutions found.
                                                Please ensure constructor calls : base(year, day).
                                                """);
        return result;
    }

    /// <summary>
    /// Benchmark all solutions.
    /// </summary>
    /// <param name="year">Optional year.</param>
    /// <param name="config">Optional <see cref="BenchmarkDotNet"/> config.</param>
    public void BenchmarkAll(int? year = null, IConfig? config = null)
    {
        var types = year is null
            ? _list.Select(x => x.GetType()).ToArray()
            : _list.Where(x => x.Year == year).Select(x => x.GetType()).ToArray();
        var summaries = BenchmarkRunner.Run(types, config);
        Console.WriteLine($"Results saved to: {summaries[0].ResultsDirectoryPath}");
    }
}
