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

    /// <summary>
    /// Set a custom HTML lookup/submission result to be used by CheckPartAsync
    /// and SubmitPartAsync instead of the downloaded pages.
    /// </summary>
    /// <param name="adventBase">Extending interface.</param>
    /// <param name="htmlSubmitResult">The custom HTML submission result to test with.</param>
    /// <param name="htmlLookupResult">The custom HTML already submitted answer result to test with.</param>
    public static void SetTestHtmlResults(this IAoC adventBase, string htmlSubmitResult, string htmlLookupResult)
    {
        ((AdventBase)adventBase).SetTestHtmlResults(htmlSubmitResult, htmlLookupResult);
    }
}
