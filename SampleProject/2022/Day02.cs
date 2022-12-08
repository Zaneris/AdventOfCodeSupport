using AdventOfCodeSupport;

namespace SampleProject._2022;

public class Day02 : AdventBase
{
    protected override object InternalPart1()
    {
        var max = Input.Lines.Max(int.Parse);
        Bag["Part1"] = max.ToString(); // Pass to unit test.
        return max;
    }

    protected override object InternalPart2()
    {
        Bag["Part2"] = Input.Text.Length.ToString();
        return "Part2";
    }
}
