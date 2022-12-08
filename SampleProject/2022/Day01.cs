using AdventOfCodeSupport;

namespace SampleProject._2022;

public class Day01 : AdventBase
{
    protected override object InternalPart1()
    {
        var total = Input.Lines.Sum(int.Parse);
        Bag["Part1"] = total.ToString(); // Pass to unit test.
        return total;
    }

    protected override object InternalPart2()
    {
        Bag["Part2"] = Input.Blocks[0].Text;
        return "Part2";
    }
}
