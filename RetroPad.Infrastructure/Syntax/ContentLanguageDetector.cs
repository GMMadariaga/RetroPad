namespace RetroPad.Infrastructure.Syntax;

using System.Text.RegularExpressions;

public static class ContentLanguageDetector
{
    private static readonly (string Language, Func<string, int> Score)[] Detectors =
    [
        ("JSON", ScoreJson),
        ("XML", ScoreXml),
        ("HTML", ScoreHtml),
        ("CSS", ScoreCss),
        ("SQL", ScoreSql),
        ("C#", ScoreCSharp),
        ("JavaScript", ScoreJavaScript),
        ("Python", ScorePython),
        ("Bash", ScoreBash),
    ];

    public static string Detect(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "PlainText";

        var best = "PlainText";
        var bestScore = 0;

        foreach (var (lang, scorer) in Detectors)
        {
            var score = scorer(content);
            if (score > bestScore)
            {
                bestScore = score;
                best = lang;
            }
        }

        return bestScore >= 2 ? best : "PlainText";
    }

    private static int ScoreJson(string s)
    {
        var score = 0;
        var trimmed = s.TrimStart();
        if (trimmed.StartsWith('{') || trimmed.StartsWith('[')) score += 2;
        if (Regex.IsMatch(s, @"""[a-zA-Z_]\w*""\s*:")) score += 3;
        if (Regex.IsMatch(s, @":\s*""[^""\\]*(\\.[^""\\]*)*""")) score += 1;
        if (Regex.IsMatch(s, @":\s*(true|false|null|\d+(\.\d+)?)\s*[,}\]]")) score += 1;
        return score;
    }

    private static int ScoreXml(string s)
    {
        var score = 0;
        if (s.TrimStart().StartsWith("<?xml")) score += 5;
        if (Regex.IsMatch(s, @"<[a-zA-Z][\w\-]*(\s+[a-zA-Z][\w\-]*=""[^""]*"")*\s*/?>")) score += 2;
        if (Regex.IsMatch(s, @"</[a-zA-Z][\w\-]*>")) score += 1;
        if (Regex.IsMatch(s, @"<!--.*?-->")) score += 1;
        return score;
    }

    private static int ScoreHtml(string s)
    {
        var score = 0;
        if (Regex.IsMatch(s, @"<(?:html|head|body|div|span|p|a|img|table|form|input|script|style)\b", RegexOptions.IgnoreCase)) score += 3;
        if (Regex.IsMatch(s, @"<!DOCTYPE\s+html", RegexOptions.IgnoreCase)) score += 4;
        if (Regex.IsMatch(s, @"class=""[^""]*""", RegexOptions.IgnoreCase)) score += 1;
        if (Regex.IsMatch(s, @"<(?:br|hr|img|input|meta|link)\b", RegexOptions.IgnoreCase)) score += 1;
        return score;
    }

    private static int ScoreCss(string s)
    {
        var score = 0;
        if (Regex.IsMatch(s, @"[.#]?[a-zA-Z][\w\-]*\s*\{")) score += 2;
        if (Regex.IsMatch(s, @"[a-z\-]+\s*:\s*[^;]+;")) score += 3;
        if (Regex.IsMatch(s, @"@(?:media|import|keyframes|font-face)\b")) score += 2;
        if (Regex.IsMatch(s, @"(?:margin|padding|display|color|background|font-size)\s*:")) score += 1;
        return score;
    }

    private static int ScoreSql(string s)
    {
        var score = 0;
        var upper = s.ToUpperInvariant();
        if (Regex.IsMatch(upper, @"\bSELECT\s+.+\s+FROM\b")) score += 4;
        if (Regex.IsMatch(upper, @"\b(?:INSERT\s+INTO|UPDATE\s+\w+\s+SET|DELETE\s+FROM)\b")) score += 3;
        if (Regex.IsMatch(upper, @"\bCREATE\s+(?:TABLE|INDEX|VIEW|DATABASE)\b")) score += 3;
        if (Regex.IsMatch(upper, @"\bWHERE\s+")) score += 1;
        if (Regex.IsMatch(upper, @"\b(?:JOIN|LEFT\s+JOIN|INNER\s+JOIN)\b")) score += 1;
        if (Regex.IsMatch(upper, @";\s*$")) score += 1;
        return score;
    }

    private static int ScoreCSharp(string s)
    {
        var score = 0;
        if (Regex.IsMatch(s, @"\busing\s+[A-Z][\w.]+\s*;")) score += 2;
        if (Regex.IsMatch(s, @"\bnamespace\s+[A-Z][\w.]+")) score += 3;
        if (Regex.IsMatch(s, @"\bclass\s+[A-Z]\w+")) score += 2;
        if (Regex.IsMatch(s, @"\b(?:public|private|protected|internal)\s+")) score += 1;
        if (Regex.IsMatch(s, @"\bvoid\s+\w+\s*\(")) score += 1;
        if (Regex.IsMatch(s, @"=>")) score += 1;
        return score;
    }

    private static int ScoreJavaScript(string s)
    {
        var score = 0;
        if (Regex.IsMatch(s, @"\b(?:const|let|var)\s+\w+\s*=")) score += 2;
        if (Regex.IsMatch(s, @"=>")) score += 2;
        if (Regex.IsMatch(s, @"\bfunction\s+\w+\s*\(")) score += 2;
        if (Regex.IsMatch(s, @"\b(?:import|export)\s+(?:\{|default|[A-Z])")) score += 2;
        if (Regex.IsMatch(s, @"\bconsole\.log\s*\(")) score += 1;
        if (Regex.IsMatch(s, @"(?:===|!==)")) score += 1;
        if (Regex.IsMatch(s, @"`[^`]*`")) score += 1; // template literals
        return score;
    }

    private static int ScorePython(string s)
    {
        var score = 0;
        if (Regex.IsMatch(s, @"^\s*def\s+\w+\s*\(", RegexOptions.Multiline)) score += 3;
        if (Regex.IsMatch(s, @"^\s*class\s+\w+", RegexOptions.Multiline)) score += 2;
        if (Regex.IsMatch(s, @"^\s*import\s+\w+", RegexOptions.Multiline)) score += 2;
        if (Regex.IsMatch(s, @"^\s*from\s+\w+\s+import\b", RegexOptions.Multiline)) score += 2;
        if (Regex.IsMatch(s, @"if\s+__name__\s*==\s*""__main__""")) score += 4;
        if (Regex.IsMatch(s, @"print\s*\(")) score += 1;
        if (Regex.IsMatch(s, @"^\s*#.*$", RegexOptions.Multiline)) score += 1;
        return score;
    }

    private static int ScoreBash(string s)
    {
        var score = 0;
        if (s.TrimStart().StartsWith("#!/")) score += 3;
        if (Regex.IsMatch(s, @"^\s*#!/bin/(?:ba)?sh", RegexOptions.Multiline)) score += 5;
        if (Regex.IsMatch(s, @"^\s*(?:if|then|else|fi|for|while|do|done|case|esac)\b", RegexOptions.Multiline)) score += 2;
        if (Regex.IsMatch(s, @"^\s*\w+=\S+", RegexOptions.Multiline)) score += 1;
        if (Regex.IsMatch(s, @"^\s*(?:echo|grep|sed|awk|cat|mkdir|rm|cd|ls)\b", RegexOptions.Multiline)) score += 1;
        if (Regex.IsMatch(s, @"^\s*\$\w+", RegexOptions.Multiline)) score += 1;
        return score;
    }
}
