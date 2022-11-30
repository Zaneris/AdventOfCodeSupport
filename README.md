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

public class Day01 : AoCBase
{
    public Day01() : base(2022, 1) { }

    protected override void InternalPart1()
    {
        // Part 1 solution here.
    }

    protected override void InternalPart2()
    {
        // Part 2 solution here.
    }
}
```
* The call to the base constructor has an optional 3rd parameter to disable the input for the day.
* Create a `new Solutions()` at your entry point.
* Select your day from the `Solutions`, for example:
```csharp
using AdventOfCodeSupport;

var solutions = new Solutions();
var today = solutions
    .Where(x => x.Year == 2022)
    .OrderByDescending(x => x.Day)
    .First();
```
* Run your solution parts with `today.Part1().Part2()`.
* Or benchmark them with `today.Benchmark()`, benchmarking requires running in Release.
