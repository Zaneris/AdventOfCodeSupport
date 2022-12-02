using AdventOfCodeSupport;

namespace SampleProject._2022;

public class Day01 : AdventBase
{
    protected override void InternalPart1()
    {
        var total = InputLines.Sum(int.Parse);
        Bag["Part1"] = total.ToString(); // Pass to unit test.
        Console.WriteLine(total);
    }

    protected override void InternalPart2()
    {
        Bag["Part2"] = InputText.Length.ToString();
        Console.WriteLine("Part 2");
    }
}