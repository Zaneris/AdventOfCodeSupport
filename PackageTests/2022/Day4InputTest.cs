using AdventOfCodeSupport;

namespace PackageTests._2022;

public class Day4InputTest : AdventBase
{
    protected override object InternalPart1()
    {
        Bag["Text"] = Input.Text;
        Bag["Lines"] = Input.Lines.Length.ToString();
        Bag["FirstLine"] = Input.Lines[0];
        Bag["Blocks"] = Input.Blocks.Length.ToString();
        Bag["Block1Text"] = Input.Blocks[0].Text;
        Bag["Block1Lines"] = Input.Blocks[0].Lines.Length.ToString();
        if (Input.Blocks.Length > 1)
        {
            Bag["Block2Lines"] = Input.Blocks[1].Lines.Length.ToString();
        }

        return "Test";
    }

    protected override object InternalPart2()
    {
        return 42;
    }
}
