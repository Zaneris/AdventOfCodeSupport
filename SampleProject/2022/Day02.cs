using AdventOfCodeSupport;

namespace SampleProject._2022;

public class Day02 : AdventBase
{
    protected override void InternalPart1()
    {
        var max = InputLines.Max(int.Parse);
        Bag["Part1"] = max.ToString(); // Pass to unit test.
        Console.WriteLine(max);
    }

    protected override void InternalPart2()
    {
        Bag["Part2"] = InputText.Length.ToString();
        Console.WriteLine("Part 2");
    }
}