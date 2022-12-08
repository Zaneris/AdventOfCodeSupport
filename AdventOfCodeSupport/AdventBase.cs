using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace AdventOfCodeSupport;

/// <summary>
/// Registers self with AdventSolutions, and contains built-in
/// benchmarking support for BenchmarkDotNet.
/// </summary>
[MemoryDiagnoser]
public abstract class AdventBase : IAoC
{
    /// <summary>
    /// Year of solution.
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Day of solution i.e. 1-25.
    /// </summary>
    public int Day { get; }

    private InputBlock? _input;

    /// <summary>
    /// The entire text of the loaded input file.
    /// </summary>
    protected InputBlock Input
    {
        get
        {
            if (_input is not null) return _input;
            try
            {
                var directory = new DirectoryInfo("../../../../").Name;
                var isBenchmark = directory.StartsWith("net");
                if (isBenchmark) Console.SetOut(TextWriter.Null);
                var relative = isBenchmark ? "../../../../../../../" : "../../../";
                _input = new InputBlock(File.ReadAllText($"{relative}{Year}/Inputs/{Day.ToString("D2")}.txt"));
            }
            catch (FileNotFoundException)
            {
                throw new Exception(@$"Input not found for {Year} - Day {Day.ToString("D2")}
Please ensure input is saved to ""Project Root/{Year}/Inputs/{Day.ToString("D2")}.txt""
If no input for the day, disable in the constructor with : base({Year}, {Day}, false)");
            }
            return _input;
        }
    }

    private Dictionary<string, string>? _bag;
    
    private string? _part1;
    private string? _part2;

    /// <summary>
    /// Can be used for things like unit testing to pass information
    /// back to the test.
    /// </summary>
    public Dictionary<string, string> Bag
    {
        get { return _bag ??= new Dictionary<string, string>(); }
    }

    /// <summary>
    /// Registers self with AdventSolutions.
    /// </summary>
    public AdventBase()
    {
        var type = GetType();
        if (type.ToString().StartsWith("BenchmarkDotNet"))
        {
            type = type.BaseType;
        }
        var split = type!.FullName!.Split('.');
        var dayMatches = Regex.Matches(split[^1], @"0?(\d+)");
        var yearMatches = Regex.Matches(split[^2], @"^_(\d+)$");
        if (dayMatches.Count != 1 || dayMatches[0].Groups.Count != 2
            || yearMatches.Count != 1 || yearMatches[0].Groups.Count != 2)
        {
            throw new Exception($"Unable to automatically parse year/day from class: {type}");
        }
        Day = int.Parse(dayMatches[0].Groups[1].Value);
        Year = int.Parse(yearMatches[0].Groups[1].Value);
    }

    /// <summary>
    /// Called from Part1().
    /// </summary>
    protected abstract object InternalPart1();

    /// <summary>
    /// Called from Part2().
    /// </summary>
    protected abstract object InternalPart2();

    /// <summary>
    /// Execute Part 1 of solution.
    /// </summary>
    /// <returns>Solution itself.</returns>
    [Benchmark]
    public IAoC Part1()
    {
        _part1 = InternalPart1().ToString();
        Console.WriteLine($"Part 1: {_part1}");
        return this;
    }

    /// <summary>
    /// Execute Part 2 of solution.
    /// </summary>
    /// <returns>Solution itself.</returns>
    [Benchmark]
    public IAoC Part2()
    {
        _part2 = InternalPart2().ToString();
        Console.WriteLine($"Part 2: {_part2}");
        return this;
    }

    /// <summary>
    /// Check answers against the confirmed submitted answer on AoC.
    /// Must have set user secret session cookie.
    /// </summary>
    /// <returns>Whether or not each part is correct, or null if unable to verify.</returns>
    public async Task<(bool? part1Correct, bool? part2Correct)> CheckAnswers()
    {
        var client = new AdventClient();
        var answers = await client.CheckDayAnswers(this);
        var p1 = CheckAnswer(1, answers.Part1, _part1);
        var p2 = CheckAnswer(2, answers.Part2, _part2);
        return (p1, p2);
    }

    private static bool? CheckAnswer(int part, string? verified, string? result)
    {
        if (result is null)
        {
            Console.WriteLine($"Run `day.Part{part}()` before calling `CheckAnswers()`.");
            return null;
        }

        if (verified is null)
        {
            Console.WriteLine($"Part {part} does not have a submitted accepted answer to check against.");
            return null;
        }
        
        Console.WriteLine(verified == result 
            ? $"Correct: Part {part} ({result}) matches accepted answer ({verified})." 
            : $"Incorrect: Part {part} ({result}) does not match ({verified}).");
        return verified == result;
    }

    public async Task DownloadInput()
    {
        var client = new AdventClient();
        await client.DownloadDay(this);
    }

    /// <summary>
    /// Benchmark part 1 and 2 with BenchmarkDotNet.
    /// </summary>
    /// <param name="config">Optional BenchmarkDotNet config.</param>
    public void Benchmark(IConfig? config = null)
    {
        var summary = BenchmarkRunner.Run(GetType(), config);
        Console.WriteLine($"Results saved to: {summary.ResultsDirectoryPath}");
    }

    /// <summary>
    /// Set a custom input to be used by Input property instead of the
    /// automatically loaded day's input file.
    /// </summary>
    /// <param name="input">The custom input to test with.</param>
    public void SetTestInput(string? input)
    {
        _input = input is null ? null : new InputBlock(input);
    }

    private async Task<AdventClient.AdventAnswers> GetSubmittedAnswers()
    {
        var client = new AdventClient();
        return await client.CheckDayAnswers(this);
    }
}
