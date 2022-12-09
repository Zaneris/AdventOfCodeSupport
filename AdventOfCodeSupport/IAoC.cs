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
    /// Check Part1() answer against the confirmed submitted answer on AoC.
    /// Must have set user secret session cookie.
    /// </summary>
    /// <returns>Whether or not each part is correct, or null if unable to verify.</returns>
    public Task<bool?> CheckPart1Async();

    /// <summary>
    /// Check Part2() answer against the confirmed submitted answer on AoC.
    /// Must have set user secret session cookie.
    /// </summary>
    /// <returns>Whether or not each part is correct, or null if unable to verify.</returns>
    public Task<bool?> CheckPart2Async();

    /// <summary>
    /// Download day's input from AoC.
    /// Must have set user secret session cookie.
    /// </summary>
    public Task DownloadInputAsync();

    /// <summary>
    /// Checks if the day part already has a submitted answer, and if not, asks user if they'd
    /// like to submit their Part1() result. Must have set user secret session cookie.
    /// </summary>
    /// <returns>True/false for correct answer submitted, null for already submitted.</returns>
    public Task<bool?> SubmitPart1Async();

    /// <summary>
    /// Checks if the day part already has a submitted answer, and if not, asks user if they'd
    /// like to submit their Part2() result. Must have set user secret session cookie.
    /// </summary>
    /// <returns>True/false for correct answer submitted, null for already submitted.</returns>
    public Task<bool?> SubmitPart2Async();

    /// <summary>
    /// Benchmark part 1 and 2 of this day with BenchmarkDotNet.
    /// </summary>
    /// <param name="config">Optional BenchmarkDotNet config.</param>
    public void Benchmark(IConfig? config = null);
}
