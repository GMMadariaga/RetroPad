using RetroPad.Core.Entities;

namespace RetroPad.UI.ViewModels;

public class TabViewModel : ViewModelBase
{
    private Document _document;
    private string _displayContent = string.Empty;

    public TabViewModel(Document document)
    {
        _document = document;
        _displayContent = document.Content;
    }

    public Document Document
    {
        get => _document;
        set => SetProperty(ref _document, value);
    }

    public string DisplayContent
    {
        get => _displayContent;
        set
        {
            if (SetProperty(ref _displayContent, value))
            {
                _document.Content = value;
                _document.MarkModified();
                OnPropertyChanged(nameof(Header));
                DisplayContentChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? DisplayContentChanged;

    public string Header => _document.DisplayName;

    public void RefreshHeader()
    {
        OnPropertyChanged(nameof(Header));
    }
}
