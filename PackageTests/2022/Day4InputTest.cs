using AdventOfCodeSupport;

namespace PackageTests._2022;

public class Day4InputTest : AdventBase
{
    protected override void InternalPart1()
    {
        Bag["Text"] = InputText;
        Bag["Lines"] = InputLines.Length.ToString();
        Bag["FirstLine"] = InputLines[0];
        Bag["Blocks"] = InputBlocks.Length.ToString();
        Bag["Block1Text"] = InputBlocks[0].Text;
        Bag["Block1Lines"] = InputBlocks[0].Lines.Length.ToString();
        if (InputBlocks.Length > 1)
        {
            Bag["Block2Lines"] = InputBlocks[1].Lines.Length.ToString();
        }
    }

    protected override void InternalPart2()
    {
        throw new NotImplementedException();
    }
}
