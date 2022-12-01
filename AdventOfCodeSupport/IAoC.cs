namespace AdventOfCodeSupport;

/// <summary>
/// Interface for AoC day solutions.
/// </summary>
public interface IAoC
{
    /// <summary>
    /// AoC year.
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// AoC day.
    /// </summary>
    public int Day { get; }

    /// <summary>
    /// Day's part 1 solution.
    /// </summary>
    /// <returns>Self for chaining.</returns>
    public IAoC Part1();

    /// <summary>
    /// Day's part 2 solution.
    /// </summary>
    /// <returns>Self for chaining.</returns>
    public IAoC Part2();

    /// <summary>
    /// Benchmark part 1 and 2 of this day with BenchmarkDotNet.
    /// </summary>
    public void Benchmark();
}
