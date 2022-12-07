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

    private InputBlock[]? _inputBlocks;

    /// <summary>
    /// The input file split on double new lines, blocks contain raw text and lines.
    /// Leading and trailing empty lines removed.
    /// </summary>
    public InputBlock[] Blocks
    {
        get
        {
            if (_inputBlocks is not null) return _inputBlocks;
            _inputBlocks = Text.Replace("\r", "").Trim('\n').Split("\n\n").Select(x => new InputBlock(x))
                .ToArray();
            return _inputBlocks;
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
