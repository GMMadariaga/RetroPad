namespace RetroPad.Core.Ports;

public interface ICodeFormatter
{
    string Format(string content, string language);
    bool SupportsLanguage(string language);
}
