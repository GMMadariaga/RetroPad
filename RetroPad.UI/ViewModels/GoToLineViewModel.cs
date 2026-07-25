namespace RetroPad.UI.ViewModels;

public class GoToLineViewModel : ViewModelBase
{
    private string _lineNumberText = string.Empty;
    private int _maxLine;
    private string _statusText = string.Empty;

    public string LineNumberText
    {
        get => _lineNumberText;
        set => SetProperty(ref _lineNumberText, value);
    }

    public int MaxLine
    {
        get => _maxLine;
        set
        {
            if (SetProperty(ref _maxLine, value))
                OnPropertyChanged(nameof(MaxLineText));
        }
    }

    public string MaxLineText => $"(1 - {MaxLine})";

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public int? GetLineNumber()
    {
        if (int.TryParse(_lineNumberText, out var line) && line >= 1 && line <= MaxLine)
            return line;
        return null;
    }
}
