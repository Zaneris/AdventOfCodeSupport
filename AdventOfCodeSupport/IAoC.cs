using BenchmarkDotNet.Configs;

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
    /// Check answers against the confirmed submitted answer on AoC.
    /// Must have set user secret session cookie.
    /// </summary>
    /// <returns>Whether or not each part is correct, or null if unable to verify.</returns>
    public Task<(bool? part1Correct, bool? part2Correct)> CheckAnswers();

    /// <summary>
    /// Download day's input from AoC.
    /// Must have set user secret session cookie.
    /// </summary>
    public Task DownloadInput();

    /// <summary>
    /// Benchmark part 1 and 2 of this day with BenchmarkDotNet.
    /// </summary>
    /// <param name="config">Optional BenchmarkDotNet config.</param>
    public void Benchmark(IConfig? config = null);
}
