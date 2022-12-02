# AdventOfCodeSupport
This package provides support to simplify the process of storing each day's puzzle within a single solution. Each day is automatically registered to a central location, with built-in support for BenchmarkDotNet.
## Getting Started
* Begin by adding this NuGet package to your project `AdventOfCodeSupport`.
* Add a folder to your project for the current year i.e. `2022`.
* Add a subfolder to the year called `Inputs`.
* Place each day's input into that folder named by day with 2 digits `01.txt`.
* Create a class in the `2022` folder called `Day01.cs`
```text
Project/
├── Program.cs
└── 2022/
    ├── Day01.cs
    ├── Day02.cs
    └── Inputs/
        ├── 01.txt
        └── 02.txt
```
```csharp
using AdventOfCodeSupport;

namespace Foo._2022;

public class Day01 : AdventBase
{
    protected override void InternalPart1()
    {
        // Part 1 solution here.
        Console.WriteLine($"Input characters: {InputText.Length}");
        Console.WriteLine($"Input lines: {InputLines.Length}");
    }

    protected override void InternalPart2()
    {
        // Part 2 solution here.
        Bag["Foo"] = "Bar"; // Pass information to unit tests
    }
}
```
* The properties `InputText` and `InputLines` load from the day's input file automatically.
* Create a `new AdventSolutions()` at your entry point.
* Select your day from the `AdventSolutions`, for example:
```csharp
using AdventOfCodeSupport;

var solutions = new AdventSolutions();
var today = solutions.GetMostRecentDay();
// var day3 = solutions.GetDay(2022, 3);
// var day4 = solutions.First(x => x.Year == 2022 && x.Day == 4);
today.Part1().Part2();
// today.Benchmark();
```
* Run your solution parts with `today.Part1().Part2()`.
* Or benchmark them with `today.Benchmark()`, benchmarking requires running in Release.

## Benchmarking
There's 2 different ways you can benchmark your solutions with this package,
namely, on the derived `AdventBase` day object itself with `.Benchmark()`; or,
on the entire collection of days under `AdventSolutions` with `.BenchmarkAll()`.
Both options accept `IConfig` for `BenchmarkDotNet` configuration, refer to their
documentation for more information. `BenchmarkAll()` also optionally accepts a
year parameter to limit the run to a specific year.

## Download Input Files
To begin you'll need to login to www.adventofcode.com, open the browser dev tools,
then head to Application Tab -> Storage -> Cookies, click the cookie for the site,
then copy the value (long chain of numbers and letters) of the session cookie.

* Open the folder containing your `.csproj` file in terminal.
* Run the command `dotnet user-secrets init`
* Then `dotnet user-secrets set "session" "cookie"` but replace the word `cookie`
with the actual cookie copied from the site.
* After creating your `AdventSolutions` call `.DownloadInputsAsync()`
```csharp
var solutions = new AdventSolutions();
await solutions.DownloadInputsAsync();
```
This only downloads input files that aren't on disk, so if you need to replace
one for whatever reason, you'll need to delete the old file first.

## Unit Testing
Some extension methods such as `SetTestInput` which means `InputText` and 
`InputLines` will use that test input instead of the actual input file,
along with `GetBag()` can help assist you with creating unit tests.
```csharp
protected override void InternalPart1()
{
    // Do some work then...
    Bag["Test"] = InputText; // Pass to unit test.
}
```
```csharp
using AdventOfCodeSupport;
using AdventOfCodeSupport.Testing;

public class SampleTests
{
    private readonly AdventSolutions _solutions;

    public SampleTests()
    {
        _solutions = new AdventSolutions();
    }
    
    [Fact]
    public void InputTest_CustomInput_TextLoaded()
    {
        var day = _solutions.GetDay(2022, 4);
        day.SetTestInput("123");
        day.Part1();
        Assert.StartsWith("123", day.GetBag()["Test"]);
    }
}
```