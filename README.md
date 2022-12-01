# AdventOfCodeSupport
This package provides support to simplify the process of storing each day's puzzle within a single solution. Each day is automatically registered to a central location, with built-in support for BenchmarkDotNet.
## Usage
* Begin by adding this NuGet package to your project `AdventOfCodeSupport`.
* Add a folder to your project for the current year i.e. `2022`.
* Add a subfolder to the year called `Inputs`.
* Place each day's input into that folder named by day with 2 digits `01.txt`.
* Create a class in the `2022` folder called `Day01.cs`
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
var day3 = solutions.GetDay(2022, 3);
var day4 = solutions.First(x => x.Year == 2022 && x.Day == 4);
```
* Run your solution parts with `today.Part1().Part2()`.
* Or benchmark them with `today.Benchmark()`, benchmarking requires running in Release.
