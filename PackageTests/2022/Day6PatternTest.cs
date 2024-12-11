using AdventOfCodeSupport;

namespace PackageTests._2022;

public class Day6PatternTest : AdventBase
{
    protected override object InternalPart1()
    {
        Bag["Text"] = Input.Text;

        return Input.Lines[0];
    }

    protected override object InternalPart2()
    {
        return Input.Lines[0];
    }
}
