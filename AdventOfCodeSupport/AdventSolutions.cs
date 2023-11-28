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

    internal string InputPattern { get; private set; } = "yyyy/Inputs/dd.txt";
    internal string? ClassNamePattern { get; private set; } = null;

    /// <summary>
    /// Create a new automatically generated collection of AoC solutions.
    /// Configure the pattern used for saving and loading input files. <c>yyyy</c> is used to represent the 4 digit
    /// year, and <c>dd</c> the 2 digit day, you can also configure the pattern for class names.
    /// </summary>
    /// <param name="inputPattern">Must contain yyyy and dd</param>
    /// <param name="classNamePattern">Must contain yyyy and dd</param>
    /// <exception cref="Exception">Patterns must contain yyyy and dd</exception>
    public AdventSolutions(string inputPattern = "yyyy/Inputs/dd.txt", string? classNamePattern = null)
    {
        ConfigureInputPattern(inputPattern);
        ConfigureClassNamePattern(classNamePattern);
        var baseType = typeof(AdventBase);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => baseType.IsAssignableFrom(p) && !p.IsAbstract);
        var collectExceptions = new List<string>();
        foreach (var type in types)
        {
            try
            {
                var newInstance = (AdventBase)Activator.CreateInstance(type)!;
                newInstance.LoadYearDay(this);
                _list.Add(newInstance);
            }
            catch (Exception ex)
            {
                collectExceptions.Add(ex.Message);
            }
        }

        if (collectExceptions.Count > 0)
            throw new Exception(string.Join('\n', collectExceptions));
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

    private void ConfigureClassNamePattern(string? pattern)
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

    private void ConfigureInputPattern(string pattern)
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
}
