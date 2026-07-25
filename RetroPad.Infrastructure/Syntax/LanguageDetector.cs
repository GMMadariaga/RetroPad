namespace RetroPad.Infrastructure.Syntax;

using RetroPad.Core.Ports;

public class LanguageDetector : ISyntaxHighlighter
{
    private static readonly Dictionary<string, string> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".txt"] = "PlainText",
        [".json"] = "JSON",
        [".xml"] = "XML",
        [".html"] = "HTML",
        [".htm"] = "HTML",
        [".css"] = "CSS",
        [".js"] = "JavaScript",
        [".mjs"] = "JavaScript",
        [".ts"] = "TypeScript",
        [".cs"] = "C#",
        [".cpp"] = "C++",
        [".cc"] = "C++",
        [".cxx"] = "C++",
        [".c"] = "C",
        [".h"] = "C",
        [".java"] = "Java",
        [".py"] = "Python",
        [".go"] = "Go",
        [".rs"] = "Rust",
        [".php"] = "PHP",
        [".sql"] = "SQL",
        [".ps1"] = "PowerShell",
        [".psm1"] = "PowerShell",
        [".sh"] = "Bash",
        [".bash"] = "Bash",
        [".md"] = "Markdown",
        [".yaml"] = "YAML",
        [".yml"] = "YAML",
        [".ini"] = "INI",
        [".cfg"] = "INI",
        [".conf"] = "INI",
        ["Dockerfile"] = "Dockerfile",
    };

    private static readonly IReadOnlyList<string> SupportedLanguages =
    [
        "PlainText", "JSON", "XML", "HTML", "CSS", "JavaScript", "TypeScript",
        "C#", "C++", "C", "Java", "Python", "Go", "Rust", "PHP", "SQL",
        "PowerShell", "Bash", "Markdown", "YAML", "INI", "Dockerfile"
    ];

    public string DetectLanguage(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        if (fileName.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase))
            return "Dockerfile";

        var ext = Path.GetExtension(filePath);
        return ExtensionMap.GetValueOrDefault(ext, "PlainText");
    }

    public IReadOnlyList<string> GetSupportedLanguages() => SupportedLanguages;

    public bool SupportsLanguage(string language) => SupportedLanguages.Contains(language);
}
