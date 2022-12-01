using AdventOfCodeSupport;
using PackageTests._2022;

namespace PackageTests;

public class PackageTests
{
    private readonly AdventSolutions _solutions;

    public PackageTests()
    {
        _solutions = new AdventSolutions();
    }

    public static IEnumerable<object[]> YearDays = new List<object[]>
    {
        new object[] { 2022, 4 },
        new object[] { 2022, 12 },
        new object[] { 2021, 25 }
    };

    [Theory]
    [MemberData(nameof(YearDays))]
    public void DayTest_GetSpecificDay_ReturnsCorrectDay(int year, int day)
    {
        var result = _solutions.GetDay(year, day);
        Assert.True(result.Year == year && result.Day == day);
    }

    [Fact]
    public void DayTest_MostRecentDayNoYearSpecified_ReturnsLatestDayOfLatestYear()
    {
        var mostRecent = _solutions.GetMostRecentDay();
        Assert.True(mostRecent.Year == 2022 && mostRecent.Day == 12);
    }

    [Fact]
    public void DayTest_MostRecentDayWithYear_ReturnsLatestDayOfSpecifiedYear()
    {
        var mostRecent = _solutions.GetMostRecentDay(2021);
        Assert.True(mostRecent.Year == 2021 && mostRecent.Day == 25);
    }

    [Fact]
    public void InputTest_AllText_TextLoaded()
    {
        var day = _solutions.GetDay(2022, 4);
        day.Part1();
        Assert.Contains("Test", day.Bag["Text"]);
    }

    [Fact]
    public void InputTest_AllLines_LinesLoaded()
    {
        var day = _solutions.GetDay(2022, 4);
        day.Part2();
        Assert.Equal("2", day.Bag["Lines"]);
    }

    [Fact]
    public void NoCollection_ConstructNewDay_ValidYearDay()
    {
        var day = new TestDay12();
        Assert.True(day.Year == 2022 && day.Day == 12);
    }
}
