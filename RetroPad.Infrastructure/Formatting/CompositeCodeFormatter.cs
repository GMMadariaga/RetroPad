namespace RetroPad.Infrastructure.Formatting;

using RetroPad.Core.Ports;

public class CompositeCodeFormatter : ICodeFormatter
{
    private readonly Dictionary<string, ICodeFormatter> _formatters = new(StringComparer.OrdinalIgnoreCase);

    public CompositeCodeFormatter()
    {
        Register(new JsonFormatter());
        Register(new XmlFormatter());
        Register(new HtmlFormatter());
        Register(new CssFormatter());
        Register(new SqlFormatter());
    }

    private void Register(ILanguageFormatter formatter)
    {
        foreach (var lang in formatter.GetSupportedLanguages())
            _formatters[lang] = formatter;
    }

    public string Format(string content, string language)
    {
        return _formatters.TryGetValue(language, out var formatter)
            ? formatter.Format(content, language)
            : content;
    }

    public bool SupportsLanguage(string language) => _formatters.ContainsKey(language);
}

internal interface ILanguageFormatter : ICodeFormatter
{
    IReadOnlyList<string> GetSupportedLanguages();
}

internal class JsonFormatter : ILanguageFormatter
{
    public IReadOnlyList<string> GetSupportedLanguages() => ["JSON"];

    public string Format(string content, string language)
    {
        try
        {
            var obj = Newtonsoft.Json.Linq.JToken.Parse(content);
            return obj.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        catch
        {
            return content;
        }
    }

    public bool SupportsLanguage(string language) => language.Equals("JSON", StringComparison.OrdinalIgnoreCase);
}

internal class XmlFormatter : ILanguageFormatter
{
    public IReadOnlyList<string> GetSupportedLanguages() => ["XML"];

    public string Format(string content, string language)
    {
        try
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(content);
            using var sw = new StringWriter();
            using var writer = new System.Xml.XmlTextWriter(sw)
            {
                Formatting = System.Xml.Formatting.Indented,
                Indentation = 2
            };
            doc.WriteTo(writer);
            return sw.ToString();
        }
        catch
        {
            return content;
        }
    }

    public bool SupportsLanguage(string language) => language.Equals("XML", StringComparison.OrdinalIgnoreCase);
}

internal class HtmlFormatter : ILanguageFormatter
{
    public IReadOnlyList<string> GetSupportedLanguages() => ["HTML"];

    public string Format(string content, string language)
    {
        try
        {
            var doc = new System.Xml.XmlDocument();
            // Wrap in root if missing html tag
            var wrapped = content.TrimStart();
            if (!wrapped.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) &&
                !wrapped.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
            {
                wrapped = "<html>" + content + "</html>";
            }
            doc.LoadXml(wrapped);
            using var sw = new StringWriter();
            using var writer = new System.Xml.XmlTextWriter(sw)
            {
                Formatting = System.Xml.Formatting.Indented,
                Indentation = 2
            };
            doc.WriteTo(writer);
            return sw.ToString();
        }
        catch
        {
            return content;
        }
    }

    public bool SupportsLanguage(string language) => language.Equals("HTML", StringComparison.OrdinalIgnoreCase);
}

internal class CssFormatter : ILanguageFormatter
{
    public IReadOnlyList<string> GetSupportedLanguages() => ["CSS"];

    public string Format(string content, string language)
    {
        var sb = new System.Text.StringBuilder();
        int indent = 0;

        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (trimmed.StartsWith('}'))
                indent = Math.Max(0, indent - 1);

            sb.AppendLine(new string(' ', indent * 2) + trimmed);

            if (trimmed.EndsWith('{'))
                indent++;
        }

        return sb.ToString().TrimEnd();
    }

    public bool SupportsLanguage(string language) => language.Equals("CSS", StringComparison.OrdinalIgnoreCase);
}

internal class SqlFormatter : ILanguageFormatter
{
    private static readonly string[] Keywords = [
        "SELECT", "FROM", "WHERE", "AND", "OR", "ORDER BY", "GROUP BY",
        "HAVING", "INSERT INTO", "VALUES", "UPDATE", "SET", "DELETE FROM",
        "CREATE TABLE", "ALTER TABLE", "DROP TABLE", "JOIN", "LEFT JOIN",
        "RIGHT JOIN", "INNER JOIN", "OUTER JOIN", "ON", "AS", "UNION",
        "LIMIT", "OFFSET", "DISTINCT", "CASE", "WHEN", "THEN", "ELSE", "END"
    ];

    public IReadOnlyList<string> GetSupportedLanguages() => ["SQL"];

    public string Format(string content, string language)
    {
        var lines = content
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l));

        var sb = new System.Text.StringBuilder();
        int indent = 0;

        foreach (var line in lines)
        {
            var upper = line.ToUpperInvariant();

            if (upper.StartsWith(')') || upper.StartsWith("END"))
                indent = Math.Max(0, indent - 1);

            sb.AppendLine(new string(' ', indent * 2) + line);

            if (upper.EndsWith('(') || upper.EndsWith("BEGIN"))
                indent++;
        }

        return sb.ToString().TrimEnd();
    }

    public bool SupportsLanguage(string language) => language.Equals("SQL", StringComparison.OrdinalIgnoreCase);
}
