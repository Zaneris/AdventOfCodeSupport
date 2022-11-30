using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AdventOfCodeSupport;

/// <summary>
/// Registers self with AdventSolutions, and contains built-in
/// benchmarking support for BenchmarkDotNet.
/// </summary>
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
    
    /// <summary>
    /// The text of the loaded input file.
    /// </summary>
    protected string InputText { get; private set; } = null!;

    private readonly bool _hasInput;

    /// <summary>
    /// Registers self with AdventSolutions.
    /// </summary>
    /// <param name="year">Year of this AoC solution.</param>
    /// <param name="day">Day of this AoC solution i.e. 1-25.</param>
    /// <param name="hasInput">Set to false to disable input loading.</param>
    protected AdventBase(int year, int day, bool hasInput = true)
    {
        Year = year;
        Day = day;
        _hasInput = hasInput;
    }

    /// <summary>
    /// Called from Part1().
    /// </summary>
    protected abstract void InternalPart1();
    
    /// <summary>
    /// Called from Part2().
    /// </summary>
    protected abstract void InternalPart2();

    private void LoadInput()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (InputText is not null || !_hasInput) return;
        try
        {
            var directory = new DirectoryInfo("../../../../").Name;
            var isBenchmark = directory.StartsWith("net");
            if (isBenchmark) Console.SetOut(TextWriter.Null);
            var relative = isBenchmark ? "../../../../../../../" : "../../../";
            InputText = File.ReadAllText($"{relative}{Year}/Inputs/{Day.ToString("D2")}.txt");
        }
        catch (FileNotFoundException)
        {
            throw new Exception(@$"Input not found for {Year} - Day {Day.ToString("D2")}
Please ensure input is saved to ""Project Root/{Year}/Inputs/{Day.ToString("D2")}.txt""
If no input for the day, disable in the constructor with : base({Year}, {Day}, false)");
        }
    }

    /// <summary>
    /// Execute Part 1 of solution.
    /// </summary>
    /// <returns>Solution itself.</returns>
    [Benchmark]
    public IAoC Part1()
    {
        LoadInput();
        InternalPart1();
        return this;
    }

    /// <summary>
    /// Execute Part 2 of solution.
    /// </summary>
    /// <returns>Solution itself.</returns>
    [Benchmark]
    public IAoC Part2()
    {
        LoadInput();
        InternalPart2();
        return this;
    }

    /// <summary>
    /// Benchmark part 1 and 2 with BenchmarkDotNet.
    /// </summary>
    public void Benchmark()
    {
        BenchmarkRunner.Run(GetType());
    }
}
