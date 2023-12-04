using System.Text;
using CommunityToolkit.HighPerformance;

namespace AdventOfCodeSupport;

/// <summary>
/// Block of the input file.
/// </summary>
public class InputBlock
{
    private string? _text;

    /// <summary>
    /// The raw text of this block.
    /// </summary>
    public string Text => _text ??= Encoding.UTF8.GetString(Bytes);

    /// <summary>
    /// Array of raw <see cref="byte"/> for the input file.
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/> for the input file.
    /// </summary>
    public ReadOnlySpan<byte> Span => Bytes;

    /// <summary>
    /// <see cref="ReadOnlySpan2D{T}"/> of <see cref="byte"/> for the input file.
    /// </summary>
    public ReadOnlySpan2D<byte> Span2D
    {
        get
        {
            var width = Span.IndexOf((byte)'\n');
            return new ReadOnlySpan2D<byte>(Bytes, 0, (Span.Length + 1) / (width + 1), width, 1);
        }
    }

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
            _inputBlocks = Text
                .Replace("\r", "")
                .Trim('\n')
                .Split("\n\n")
                .Select(x => new InputBlock(Encoding.UTF8.GetBytes(x)))
                .ToArray();
            return _inputBlocks;
        }
    }

    /// <summary>
    /// Used by package.
    /// </summary>
    /// <param name="bytes">Raw bytes for block.</param>
    internal InputBlock(byte[] bytes)
    {
        if (bytes is [0xEF, 0xBB, 0xBF, ..])
        {
            Bytes = new byte[bytes.Length - 3];
            Array.Copy(bytes, 3, Bytes, 0, Bytes.Length);
        } else Bytes = bytes;
    }
}
