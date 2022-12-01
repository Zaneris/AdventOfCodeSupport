namespace AdventOfCodeSupport.Testing;

/// <summary>
/// Extensions to assist with unit testing.
/// </summary>
public static class TestExtensions
{
    /// <summary>
    /// Set a custom input to be used by InputText and InputLines instead of the
    /// automatically loaded day's input file.
    /// </summary>
    /// <param name="adventBase">Extending interface.</param>
    /// <param name="input">The custom input to test with.</param>
    public static void SetTestInput(this IAoC adventBase, string? input)
    {
        ((AdventBase)adventBase).SetTestInput(input);
    }

    /// <summary>
    /// Versatile bag, assists with unit testing to pass information back to the test.
    /// </summary>
    /// <param name="adventBase">Extending interface.</param>
    public static Dictionary<string, string> GetBag(this IAoC adventBase)
    {
        return ((AdventBase)adventBase).Bag;
    }
}