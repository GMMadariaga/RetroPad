namespace RetroPad.Core.Ports;

public interface ISyntaxHighlighter
{
    string DetectLanguage(string filePath);
    IReadOnlyList<string> GetSupportedLanguages();
    bool SupportsLanguage(string language);
}
