using AdventOfCodeSupport;

namespace PackageTests._2022;

public class Day4InputTest : AdventBase
{
    protected override void InternalPart1()
    {
        Bag["Text"] = InputText;
    }

    protected override void InternalPart2()
    {
        Bag["Lines"] = InputLines.Length.ToString();
    }
}
