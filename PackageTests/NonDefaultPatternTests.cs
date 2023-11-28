namespace PackageTests;

/// <summary>
/// Each test manually sets expected pattern
/// </summary>
public class NonDefaultPatternTests
{
    [Fact]
    public void InputTest_NonDefaultPattern_InputLoads()
    {
        var day = new AdventSolutions("Inputs/yyyydd.txt").GetDay(2022, 5);
        day.Part1();
        Assert.Contains("RootInputsFolder", day.GetBag()["Text"]);
    }

    [Fact]
    public void InputTest_NonDefaultPattern_InputException()
    {
        var day = new AdventSolutions("Yearyyyy/dd.txt").GetDay(2022, 6);
        Assert.IsType<Day6PatternTest>(day);
        var exception = Record.Exception(() => day.Part1());
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("input", exception.Message);
    }

    [Fact]
    public void InputPattern_SetWithoutYear_Throws()
    {
        var exception = Record.Exception(() => new AdventSolutions("dd.txt"));
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("yyyy", exception.Message);
    }

    [Fact]
    public void InputPattern_SetWithoutDay_Throws()
    {
        var exception = Record.Exception(() => new AdventSolutions("yyyy.txt"));
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("dd", exception.Message);
    }

    [Fact]
    public void InputPattern_SetWithDayAndYear_DoesNotThrow()
    {
        var exception = Record.Exception(() => new AdventSolutions("yyyydd.txt"));
        Assert.Null(exception);
    }

    [Fact]
    public void InputPattern_SetWithoutFile_Throws()
    {
        var exception = Record.Exception(() => new AdventSolutions("yyyydd/Input/"));
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("filename", exception.Message);
    }

    [Fact]
    public void ClassPattern_SetWithoutYear_Throws()
    {
        var exception = Record.Exception(() => new AdventSolutions(classNamePattern: "Daydd.cs"));
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("yyyy", exception.Message);
    }

    [Fact]
    public void ClassPattern_SetWithoutDay_Throws()
    {
        var exception = Record.Exception(() => new AdventSolutions(classNamePattern: "yyyy.cs"));
        Assert.NotNull(exception);
        Assert.IsType<Exception>(exception);
        Assert.Contains("dd", exception.Message);
    }

    [Fact]
    public void ClassPattern_SetWithDayAndYear_DoesNotThrow()
    {
        var exception = Record.Exception(() => new AdventSolutions(classNamePattern: "yyyydd.cs"));
        Assert.NotNull(exception); // Not null due to pattern not matching class names
        Assert.DoesNotContain("yyyy", exception.Message);
        Assert.DoesNotContain("dd", exception.Message);
    }
}
