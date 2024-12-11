namespace PackageTests._2022;

public class Day7SpanTest : AdventBase
{
    protected override object InternalPart1()
    {
        var span = Input.Span2D;
        Bag["Cols"] = span.Width.ToString();
        Bag["Rows"] = span.Height.ToString();
        Bag["Last"] = ((char)span[5, 9]).ToString();
        return 0;
    }

    protected override object InternalPart2()
    {
        throw new NotImplementedException();
    }
}
