namespace AdventOfCodeSupport;

/// <summary>
/// Block of the input file.
/// </summary>
public class InputBlock
{
    /// <summary>
    /// The raw text of this block.
    /// </summary>
    public string Text { get; }

    private string[]? _lines;

    /// <summary>
    /// This block split on new lines, leading and trailing empty lines removed.
    /// </summary>
    public string[] Lines
    {
        get
        {
            if (_lines is not null) return _lines;
            _lines = Text.Replace("\r", "").Trim('\n').Split('\n');
            return _lines;
        }
    }

    /// <summary>
    /// Used by package.
    /// </summary>
    /// <param name="text">Raw text for block.</param>
    public InputBlock(string text)
    {
        Text = text;
    }
}
