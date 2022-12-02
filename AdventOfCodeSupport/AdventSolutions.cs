using System.Collections;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace AdventOfCodeSupport;

/// <summary>
/// Automatically collects all solution classes derived from AdventBase.
/// </summary>
public class AdventSolutions : IEnumerable<IAoC>
{
    private readonly List<IAoC> _list = new();

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
}
