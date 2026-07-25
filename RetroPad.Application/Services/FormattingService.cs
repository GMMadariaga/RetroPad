namespace RetroPad.Application.Services;

using RetroPad.Core.Ports;

public class FormattingService
{
    private readonly ICodeFormatter _formatter;

    public FormattingService(ICodeFormatter formatter)
    {
        _formatter = formatter;
    }

    public string Format(string content, string language)
    {
        return _formatter.Format(content, language);
    }

    public bool CanFormat(string language)
    {
        return _formatter.SupportsLanguage(language);
    }
}
