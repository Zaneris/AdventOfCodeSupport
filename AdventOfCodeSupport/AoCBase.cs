using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AdventOfCodeSupport;

public abstract class AoCBase : IAoC
{
    public int Year { get; }
    public int Day { get; }
    protected string InputText { get; private set; } = null!;

    private readonly bool _hasInput;

    protected AoCBase(int year, int day, bool hasInput = true)
    {
        Year = year;
        Day = day;
        _hasInput = hasInput;
    }

    protected abstract void InternalPart1();
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
            throw new Exception($"""
                Input not found for {Year} - Day {Day.ToString("D2")}
                Please ensure input is saved to "Project Root/{Year}/Inputs/{Day.ToString("D2")}.txt"
                If no input for the day, disable in the constructor with : base({Year}, {Day}, false)
                """);
        }
    }

    [Benchmark]
    public IAoC Part1()
    {
        LoadInput();
        InternalPart1();
        return this;
    }

    [Benchmark]
    public IAoC Part2()
    {
        LoadInput();
        InternalPart2();
        return this;
    }

    public void Benchmark()
    {
        BenchmarkRunner.Run(GetType());
    }
}
