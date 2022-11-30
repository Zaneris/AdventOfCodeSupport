namespace AdventOfCodeSupport;

public interface IAoC
{
    public int Year { get; }
    public int Day { get; }
    public IAoC Part1();
    public IAoC Part2();
    public void Benchmark();
}
