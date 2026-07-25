using System.Text.RegularExpressions;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace RetroPad.UI.Controls;

public class JsonColorizer : DocumentColorizingTransformer
{
    private static readonly SolidColorBrush KeyBrush = new(Color.FromRgb(0x9C, 0xDC, 0xFE));       // light blue
    private static readonly SolidColorBrush StringBrush = new(Color.FromRgb(0xCE, 0x91, 0x78));     // orange
    private static readonly SolidColorBrush NumberBrush = new(Color.FromRgb(0xB5, 0xCE, 0xA8));     // light green
    private static readonly SolidColorBrush KeywordBrush = new(Color.FromRgb(0x56, 0x9C, 0xD6));    // blue
    private static readonly SolidColorBrush PunctuationBrush = new(Color.FromRgb(0x80, 0x80, 0x80)); // gray
    private static readonly SolidColorBrush CommentBrush = new(Color.FromRgb(0x6A, 0x99, 0x55));    // green

    private static readonly Regex KeyPattern = new(@"""[^""\\]*(?:\\.[^""\\]*)*""(?=\s*:)", RegexOptions.Compiled);
    private static readonly Regex StringPattern = new(@"""[^""\\]*(?:\\.[^""\\]*)*""", RegexOptions.Compiled);
    private static readonly Regex NumberPattern = new(@"\b-?\d+\.?\d*(?:[eE][+-]?\d+)?\b", RegexOptions.Compiled);
    private static readonly Regex KeywordPattern = new(@"\b(true|false|null)\b", RegexOptions.Compiled);
    private static readonly Regex PunctuationPattern = new(@"[{}\[\]:,]", RegexOptions.Compiled);

    protected override void ColorizeLine(DocumentLine line)
    {
        var text = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(text)) return;

        var lineStart = line.Offset;

        // 1. Keys (highest priority)
        foreach (Match m in KeyPattern.Matches(text))
        {
            ChangeLinePart(lineStart + m.Index, lineStart + m.Index + m.Length,
                el => el.TextRunProperties.SetForegroundBrush(KeyBrush));
        }

        // 2. Keywords (true/false/null)
        foreach (Match m in KeywordPattern.Matches(text))
        {
            ChangeLinePart(lineStart + m.Index, lineStart + m.Index + m.Length,
                el => el.TextRunProperties.SetForegroundBrush(KeywordBrush));
        }

        // 3. Numbers
        foreach (Match m in NumberPattern.Matches(text))
        {
            // Don't color numbers that are inside strings
            if (IsInsideString(text, m.Index)) continue;
            ChangeLinePart(lineStart + m.Index, lineStart + m.Index + m.Length,
                el => el.TextRunProperties.SetForegroundBrush(NumberBrush));
        }

        // 4. Punctuation
        foreach (Match m in PunctuationPattern.Matches(text))
        {
            if (IsInsideString(text, m.Index)) continue;
            ChangeLinePart(lineStart + m.Index, lineStart + m.Index + m.Length,
                el => el.TextRunProperties.SetForegroundBrush(PunctuationBrush));
        }

        // 5. String values (after keys — keys take priority)
        foreach (Match m in StringPattern.Matches(text))
        {
            // Skip if already colored as key
            if (IsKeyAt(text, m.Index)) continue;
            ChangeLinePart(lineStart + m.Index, lineStart + m.Index + m.Length,
                el => el.TextRunProperties.SetForegroundBrush(StringBrush));
        }
    }

    private static bool IsInsideString(string text, int index)
    {
        bool inString = false;
        for (int i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '"' && (i == 0 || text[i - 1] != '\\'))
                inString = !inString;
        }
        return inString;
    }

    private static bool IsKeyAt(string text, int index)
    {
        // Check if this string is followed by : (key pattern)
        if (index >= text.Length || text[index] != '"') return false;
        int end = text.IndexOf('"', index + 1);
        if (end < 0) return false;
        // Skip escaped quotes
        while (end > 0 && text[end - 1] == '\\')
            end = text.IndexOf('"', end + 1);
        if (end < 0) return false;
        // Check for : after the closing quote
        for (int i = end + 1; i < text.Length; i++)
        {
            if (text[i] == ' ') continue;
            return text[i] == ':';
        }
        return false;
    }
}
