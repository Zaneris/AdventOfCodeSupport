using AdventOfCodeSupport;

namespace AltNamingTests;

public class NamingTests
{
    [Fact]
    public void ClassPattern_SetWithDayAndYear_SingularDay()
    {
        var solutions = new AdventSolutions(classNamePattern: "YearyyyyDaydd.cs");
        var day = solutions.GetDay(2023, 9);
        Assert.True(day is { Year: 2023, Day: 9 });
        Assert.IsType<Year2023Day09>(day);
        Assert.Single(solutions);
    }
}
