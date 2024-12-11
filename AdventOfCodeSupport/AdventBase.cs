using System.Text;
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
public abstract partial class AdventBase
{
    private AdventSolutions? _adventSolutions;
    private InputBlock? _input;
    private Dictionary<string, string>? _bag;
    private string? _part1;
    private string? _part2;
    private string? _checkedPart1;
    private string? _checkedPart2;
    private string? _testHtmlSubmit;
    private string? _testHtmlLookup;
    private bool _downloadedAnswers;
    private bool _onLoad;
    private bool _useSampleInput;
    private bool _samplePart2;

    private static string? _projectRoot;
    internal static string ProjectRoot
    {
        get
        {
            if (_projectRoot is not null) return _projectRoot;
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directoryInfo != null)
            {
                var files = directoryInfo.GetFiles("*.csproj", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    if (files[0].Name.StartsWith("BenchmarkDotNet"))
                    {
                        Console.SetOut(TextWriter.Null);
                    }
                    else
                    {
                        _projectRoot = Path.GetDirectoryName(files[0].FullName);
                        break;
                    }
                }

                directoryInfo = directoryInfo.Parent;
            }

            if (_projectRoot is null)
                throw new DirectoryNotFoundException("Can't find project root.");
            return _projectRoot;
        }
    }

    /// <summary>
    /// Year of solution.
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Day of solution i.e. 1-25.
    /// </summary>
    public int Day { get; private set; }

    /// <summary>
    /// Answer for part 1. Will run <see cref="Part1"/> to calculate the answer if not already done.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Part1"/> method fails to set answer.</exception>
    public string Part1Answer
    {
        get
        {
            if (_part1 == null)
            {
                Part1();
            }

            return _part1 ?? throw new InvalidOperationException("Part1 answer was not set after running Part1().");
        }
    }

    /// <summary>
    /// Answer for part 2. Will run <see cref="Part2"/> to calculate the answer if not already done.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Part2"/> method fails to set answer.</exception>
    public string Part2Answer
    {
        get
        {
            if (_part2 == null)
            {
                Part2();
            }

            return _part2 ?? throw new InvalidOperationException("Part2 answer was not set after running Part2().");
        }
    }

    /// <summary>
    /// Can be used for things like unit testing to pass information
    /// back to the test.
    /// </summary>
    public Dictionary<string, string> Bag
    {
        get { return _bag ??= new Dictionary<string, string>(); }
    }

    internal string ClassName { get; private set; } = null!;

    /// <summary>
    /// The entire text of the loaded input file.
    /// </summary>
    protected InputBlock Input
    {
        get
        {
            if (!_useSampleInput && _input is not null) return _input;
            var inputPattern = _adventSolutions?.InputPattern ?? "yyyy/Inputs/dd.txt";
            inputPattern = inputPattern.Replace("yyyy", $"{Year}");
            if (_useSampleInput)
            {
                if (_samplePart2)
                {
                    var test = inputPattern.Replace("dd", $"Sample{Day:D2}P2");
                    if (File.Exists(Path.Combine(ProjectRoot, test)))
                    {
                        inputPattern = test;
                    }
                    else
                    {
                        inputPattern = inputPattern.Replace("dd", $"Sample{Day:D2}");
                    }
                }
                else
                {
                    inputPattern = inputPattern.Replace("dd", $"Sample{Day:D2}");
                }
            }
            else
            {
                inputPattern = inputPattern.Replace("dd", $"{Day:D2}");
            }
            try
            {
                Console.WriteLine($"{ProjectRoot}/{inputPattern}");
                _input = new InputBlock(File.ReadAllBytes(Path.Combine(ProjectRoot, inputPattern)));
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                throw new Exception($"""
                                     Input not found for {Year} - Day {Day:D2}
                                     Please ensure input is saved to "Project Root/{inputPattern}"
                                     If no input for the day, disable in the constructor with : base({Year}, {Day}, false)
                                     """);
            }
            return _input;
        }
    }

    /// <summary>
    /// Registers self with AdventSolutions.
    /// </summary>
    internal void LoadYearDay(AdventSolutions? adventSolutions)
    {
        _adventSolutions = adventSolutions;
        var type = GetType();
        if (type.ToString().StartsWith("BenchmarkDotNet"))
        {
            type = type.BaseType;
        }

        var split = type!.FullName!.Split('.');
        ClassName = split[^1];
        if (_adventSolutions?.ClassNamePattern is null)
        {
            var dayMatches = DayPattern().Matches(split[^1]);
            var yearMatches = YearPattern().Matches(split[^2]);
            if (dayMatches.Count != 1 || dayMatches[0].Groups.Count != 2
                                      || yearMatches.Count != 1 || yearMatches[0].Groups.Count != 2)
            {
                throw new Exception($"Unable to automatically parse year/day from class: {type}");
            }
            Day = int.Parse(dayMatches[0].Groups[1].Value);
            Year = int.Parse(yearMatches[0].Groups[1].Value);
            return;
        }

        var classPattern = _adventSolutions.ClassNamePattern;
        classPattern = classPattern.Replace("yyyy", "(\\d{4})");
        classPattern = classPattern.Replace("dd", "0?(\\d{1,2})");
        var matches = Regex.Matches(split[^1], classPattern);
        if (matches.Count != 1 || matches[0].Groups.Count != 3)
            throw new Exception($"Unable to automatically parse year/day from class: {type}");
        var group1 = int.Parse(matches[0].Groups[1].Value);
        var group2 = int.Parse(matches[0].Groups[2].Value);
        Day = group1 <= 31 ? group1 : group2;
        Year = group1 > 31 ? group1 : group2;
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
    /// Called before Part1/2 runs.
    /// </summary>
    protected virtual void InternalOnLoad() { }

    /// <summary>
    /// Just used to benchmark the InternalOnLoad() method.
    /// </summary>
    [Benchmark]
    public void OnLoad()
    {
        if (Year == 0) LoadYearDay(null);
        InternalOnLoad();
    }

    /// <summary>
    /// Execute Part 1 of solution.
    /// </summary>
    /// <returns>Solution itself.</returns>
    [Benchmark]
    public AdventBase Part1()
    {
        if (Year == 0) LoadYearDay(null);
        if (!_onLoad)
        {
            InternalOnLoad();
            _onLoad = true;
        }
        _part1 = InternalPart1().ToString();
        Console.WriteLine($"Part 1: {_part1}");
        return this;
    }

    /// <summary>
    /// Execute Part 2 of solution.
    /// </summary>
    /// <returns>Solution itself.</returns>
    [Benchmark]
    public AdventBase Part2()
    {
        if (Year == 0) LoadYearDay(null);
        if (!_onLoad)
        {
            InternalOnLoad();
            _onLoad = true;
        }
        _part2 = InternalPart2().ToString();
        Console.WriteLine($"Part 2: {_part2}");
        return this;
    }

    /// <summary>
    /// Runs <see cref="InternalPart1"/> and compares the returned value to <c>expectedResult</c>, throws if this fails.
    /// Uses Input file pattern <c>Sample01.txt SampleO2.txt</c> in folder with regular Inputs.
    /// </summary>
    /// <param name="expectedResult">Return of <see cref="InternalPart1"/> is compared to the provided parameter</param>
    /// <returns><see cref="AdventBase"/></returns>
    /// <exception cref="Exception">Throws if Part 1 does not match the <c>expectedResult</c></exception>
    public AdventBase Part1Sample(object expectedResult)
    {
        _useSampleInput = true;
        Part1();
        if (expectedResult.ToString() != _part1)
            throw new Exception("Part 1 does not match expected result.");
        return this;
    }

    /// <summary>
    /// Runs <see cref="InternalPart2"/> and compares the returned value to <c>expectedResult</c>, throws if this fails.
    /// Uses Input file pattern <c>Sample01P2.txt SampleO2P2.txt</c> in folder with regular Inputs. If <c>P2</c> is not
    /// found, <c>Sample01.txt SampleO2.txt</c> is used.
    /// </summary>
    /// <param name="expectedResult">Return of <see cref="InternalPart2"/> is compared to the provided parameter</param>
    /// <returns><see cref="AdventBase"/></returns>
    /// <exception cref="Exception">Throws if Part 2 does not match the <c>expectedResult</c></exception>
    public AdventBase Part2Sample(object expectedResult)
    {
        _useSampleInput = true;
        _samplePart2 = true;
        Part2();
        if (expectedResult.ToString() != _part2)
            throw new Exception("Part 2 does not match expected result.");
        _samplePart2 = false;
        return this;
    }

    /// <summary>
    /// Check Part1() answer against the confirmed submitted answer on AoC.
    /// Must have set user secret session cookie.
    /// </summary>
    /// <returns>Whether or not each part is correct, or null if unable to verify.</returns>
    public async Task<bool?> CheckPart1Async()
    {
        if (!_downloadedAnswers) await DownloadAnswers();
        if (_part1 is null) Part1();
        return CheckAnswer(1, _checkedPart1, _part1);
    }

    /// <summary>
    /// Check Part2() answer against the confirmed submitted answer on AoC.
    /// Must have set user secret session cookie.
    /// </summary>
    /// <returns>Whether or not each part is correct, or null if unable to verify.</returns>
    public async Task<bool?> CheckPart2Async()
    {
        if (!_downloadedAnswers) await DownloadAnswers();
        if (_part2 is null) Part2();
        return CheckAnswer(2, _checkedPart2, _part2);
    }

    private async Task DownloadAnswers()
    {
        var client = new AdventClient(_adventSolutions!, this);
        var answers = await client.DownloadAnswersAsync(this, _testHtmlLookup);
        _checkedPart1 = answers.Part1;
        _checkedPart2 = answers.Part2;
        _downloadedAnswers = true;
    }

    private static bool? CheckAnswer(int part, string? verified, string? result)
    {
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

    /// <summary>
    /// If not already downloaded, downloads the day's input.
    /// Must have set user secret session cookie.
    /// </summary>
    public async Task DownloadInputAsync()
    {
        var client = new AdventClient(_adventSolutions!, this);
        await client.DownloadInputAsync(this);
    }

    /// <summary>
    /// Checks if the day part already has a submitted answer, and if not, asks user if they'd
    /// like to submit their Part1() result. Must have set user secret session cookie.
    /// </summary>
    /// <returns>True/false for correct answer submitted, null for already submitted.</returns>
    public async Task<bool?> SubmitPart1Async()
    {
        if (!_downloadedAnswers) await DownloadAnswers();
        if (_checkedPart1 is not null)
        {
            Console.WriteLine("Correct part 1 answer has already been submitted.");
            return null;
        }
        if (_part1 is null) Part1();
        if (_part1 is null) throw new Exception("Cannot submit a null answer for Part 1");
        var client = new AdventClient(_adventSolutions!, this);
        return await client.SubmitAnswerAsync(this, 1, _part1, _testHtmlSubmit);
    }

    /// <summary>
    /// Checks if the day part already has a submitted answer, and if not, asks user if they'd
    /// like to submit their Part2() result. Must have set user secret session cookie.
    /// </summary>
    /// <returns>True/false for correct answer submitted, null for already submitted.</returns>
    public async Task<bool?> SubmitPart2Async()
    {
        if (!_downloadedAnswers) await DownloadAnswers();
        if (_checkedPart2 is not null)
        {
            Console.WriteLine("Correct part 2 answer has already been submitted.");
            return null;
        }
        if (_part2 is null) Part2();
        if (_part2 is null) throw new Exception("Cannot submit a null answer for Part 2");
        var client = new AdventClient(_adventSolutions!, this);
        return await client.SubmitAnswerAsync(this, 2, _part2, _testHtmlSubmit);
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
        _input = input is null ? null : new InputBlock(Encoding.UTF8.GetBytes(input));
    }

    /// <summary>
    /// Set a custom HTML lookup/submission result to be used by <see cref="CheckPart1Async"/>
    /// and <see cref="SubmitPart1Async"/> instead of the downloaded pages.
    /// </summary>
    /// <param name="htmlSubmitResult">The custom HTML submission result to test with.</param>
    /// <param name="htmlLookupResult">The custom HTML already submitted answer result to test with.</param>
    public void SetTestHtmlResults(string htmlSubmitResult, string htmlLookupResult)
    {
        _testHtmlLookup = htmlLookupResult;
        _testHtmlSubmit = htmlSubmitResult;
    }

    [GeneratedRegex(@"0?(\d{1,2})")]
    private static partial Regex DayPattern();
    [GeneratedRegex(@"(\d{4})")]
    private static partial Regex YearPattern();
}
