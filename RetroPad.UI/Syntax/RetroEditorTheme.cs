namespace RetroPad.UI.Syntax;

using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Media;

public static class RetroEditorTheme
{
    // Foreground colors (VS Code Dark+ retro palette)
    private static readonly Color CommentColor = Color.FromRgb(0x6A, 0x99, 0x55);
    private static readonly Color StringColor = Color.FromRgb(0xCE, 0x91, 0x78);
    private static readonly Color NumberColor = Color.FromRgb(0xB5, 0xCE, 0xA8);
    private static readonly Color KeywordColor = Color.FromRgb(0x56, 0x9C, 0xD6);
    private static readonly Color TypeColor = Color.FromRgb(0x4E, 0xC9, 0xB0);
    private static readonly Color AttributeColor = Color.FromRgb(0xD7, 0xBA, 0x7D);

    public static void Apply()
    {
        var visited = new HashSet<HighlightingRuleSet>();
        foreach (var def in HighlightingManager.Instance.HighlightingDefinitions)
            ApplyColorScheme(def, visited);
    }

    private static void ApplyColorScheme(IHighlightingDefinition def, HashSet<HighlightingRuleSet> visited)
    {
        // Theme each named color reference declared in the XSHD (<Color name="..."/>)
        foreach (var color in def.NamedHighlightingColors)
            ApplyByColorName(color);

        ApplyRuleSet(def.MainRuleSet, visited);
    }

    private static void ApplyRuleSet(HighlightingRuleSet? ruleSet, HashSet<HighlightingRuleSet> visited)
    {
        if (ruleSet is null || !visited.Add(ruleSet)) return;

        // Color each rule's Color (the actual token color)
        foreach (var rule in ruleSet.Rules)
        {
            if (rule.Color is not null)
                ApplyByColorName(rule.Color);
        }

        // Color spans (string/comment literals)
        foreach (var span in ruleSet.Spans)
        {
            if (span.StartColor is not null) ApplyByColorName(span.StartColor);
            if (span.EndColor is not null) ApplyByColorName(span.EndColor);
            if (span.SpanColor is not null) ApplyByColorName(span.SpanColor);
            if (span.RuleSet is not null) ApplyRuleSet(span.RuleSet, visited);
        }
    }

    private static void ApplyByColorName(HighlightingColor color)
    {
        var name = color.Name ?? string.Empty;
        var lower = name.ToLowerInvariant();

        Color c;
        if (lower.Contains("comment") || lower.Contains("xmldoc"))
            c = CommentColor;
        else if (lower.Contains("string") || lower.Contains("char") || lower.Contains("cdata"))
            c = StringColor;
        else if (lower.Contains("number") || lower.Contains("digit"))
            c = NumberColor;
        else if (lower.Contains("type") || lower.Contains("class"))
            c = TypeColor;
        else if (lower.Contains("attribute") || lower.Contains("property") || lower.Contains("selector"))
            c = AttributeColor;
        else if (lower.Contains("keyword") || lower.Contains("reserved"))
            c = KeywordColor;
        else
            return; // don't override default/unknown colors

        color.Foreground = new SimpleHighlightingBrush(c);
        color.FontStyle = null;
        color.FontWeight = null;
    }
}
