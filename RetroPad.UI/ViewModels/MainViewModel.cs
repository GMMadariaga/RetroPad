using System.Collections.ObjectModel;
using System.IO;
using RetroPad.Application.Services;
using RetroPad.Core.Entities;
using RetroPad.Core.Ports;
using RetroPad.UI.Commands;

namespace RetroPad.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly DocumentService _documentService;
    private readonly SessionService _sessionService;
    private readonly FormattingService _formattingService;
    private readonly ConfigService _configService;
    private readonly ISyntaxHighlighter _syntaxHighlighter;

    private TabViewModel? _selectedTab;
    private string _statusText = "Ready";
    private string _languageStatus = "PlainText";
    private string _cursorStatus = "Ln 1, Col 1";
    private string _windowTitle = "RetroPad";
    private AppConfig _config;

    public MainViewModel(
        DocumentService documentService,
        SessionService sessionService,
        FormattingService formattingService,
        ConfigService configService,
        ISyntaxHighlighter syntaxHighlighter)
    {
        _documentService = documentService;
        _sessionService = sessionService;
        _formattingService = formattingService;
        _configService = configService;
        _syntaxHighlighter = syntaxHighlighter;
        _config = new AppConfig();

        NewTabCommand = RelayCommand.Create(NewTab);
        CloseTabCommand = RelayCommand.Create<object?>(CloseTab);
        OpenFileCommand = RelayCommand.CreateAsync(OpenFileAsync);
        SaveFileCommand = RelayCommand.CreateAsync(SaveFileAsync);
        SaveFileAsCommand = RelayCommand.CreateAsync(SaveFileAsAsync);
        FormatDocumentCommand = RelayCommand.CreateAsync(FormatDocumentAsync);
        ExitCommand = RelayCommand.Create(Exit);

        Tabs = [];
    }

    public ObservableCollection<TabViewModel> Tabs { get; }

    public TabViewModel? SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                UpdateLanguageStatus();
                UpdateWindowTitle();
                OnPropertyChanged(nameof(HasTabs));
            }
        }
    }

    public bool HasTabs => Tabs.Count > 0;

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string LanguageStatus
    {
        get => _languageStatus;
        set => SetProperty(ref _languageStatus, value);
    }

    public string CursorStatus
    {
        get => _cursorStatus;
        set => SetProperty(ref _cursorStatus, value);
    }

    public string WindowTitle
    {
        get => _windowTitle;
        set => SetProperty(ref _windowTitle, value);
    }

    public AppConfig Config => _config;

    public IReadOnlyList<string> AvailableLanguages => _syntaxHighlighter.GetSupportedLanguages();

    public RelayCommand NewTabCommand { get; }
    public RelayCommand CloseTabCommand { get; }
    public RelayCommand OpenFileCommand { get; }
    public RelayCommand SaveFileCommand { get; }
    public RelayCommand SaveFileAsCommand { get; }
    public RelayCommand FormatDocumentCommand { get; }
    public RelayCommand ExitCommand { get; }

    public async Task InitializeAsync()
    {
        _config = await _configService.LoadAsync();
        OnPropertyChanged(nameof(Config));

        if (_config.RememberSession)
        {
            try
            {
                var session = await _sessionService.LoadSessionAsync();
                if (session?.HasTabs == true)
                {
                    foreach (var tab in session.Tabs)
                    {
                        var doc = new Document
                        {
                            Id = tab.DocumentId,
                            FilePath = tab.FilePath,
                            FileName = tab.FileName,
                            Language = tab.Language,
                            CursorOffset = tab.CursorOffset,
                            ScrollOffset = tab.ScrollOffset
                        };

                        if (!string.IsNullOrEmpty(tab.FilePath))
                        {
                            try
                            {
                                doc.Content = await _documentService.OpenAsync(tab.FilePath);
                            }
                            catch
                            {
                                doc.Content = string.Empty;
                                doc.FileName = $"[Not Found] {doc.FileName}";
                            }
                        }
                        else
                        {
                            var temp = await _sessionService.LoadTempContentAsync(tab.DocumentId);
                            doc.Content = temp ?? string.Empty;
                        }

                        Tabs.Add(new TabViewModel(doc));
                    }

                    var activeIndex = session.ActiveTabIndex;
                    if (activeIndex >= 0 && activeIndex < Tabs.Count)
                        SelectedTab = Tabs[activeIndex];

                    StatusText = $"Session restored: {Tabs.Count} tab(s)";
                    return;
                }
            }
            catch
            {
                StatusText = "Could not restore session";
            }
        }

        NewTab();
    }

    private void NewTab()
    {
        var doc = new Document { FileName = $"Untitled {Tabs.Count + 1}" };
        var tab = new TabViewModel(doc);
        Tabs.Add(tab);
        SelectedTab = tab;
        StatusText = "New document";
    }

    private void CloseTab(object? parameter)
    {
        if (parameter is TabViewModel tab)
        {
            var index = Tabs.IndexOf(tab);
            Tabs.Remove(tab);
            if (SelectedTab == tab)
                SelectedTab = Tabs.Count > 0 ? Tabs[Math.Min(index, Tabs.Count - 1)] : null;
            if (Tabs.Count == 0)
                NewTab();
        }
    }

    private async Task OpenFileAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "All Files (*.*)|*.*",
            InitialDirectory = _config.LastDirectory
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var content = await _documentService.OpenAsync(dialog.FileName);
                var doc = new Document
                {
                    FilePath = dialog.FileName,
                    FileName = Path.GetFileName(dialog.FileName),
                    Content = content,
                    Language = _syntaxHighlighter.DetectLanguage(dialog.FileName)
                };

                var existingTab = Tabs.FirstOrDefault(t =>
                    t.Document.HasFilePath &&
                    t.Document.FilePath.Equals(dialog.FileName, StringComparison.OrdinalIgnoreCase));

                if (existingTab is not null)
                {
                    SelectedTab = existingTab;
                    StatusText = $"Switched to: {existingTab.Document.FileName}";
                    return;
                }

                var tab = new TabViewModel(doc);
                Tabs.Add(tab);
                SelectedTab = tab;
                _config.LastDirectory = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                _config.LastLanguage = doc.Language;
                UpdateLanguageStatus();
                StatusText = $"Opened: {doc.FileName}";
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }
    }

    private async Task SaveFileAsync()
    {
        if (SelectedTab is null) return;

        var doc = SelectedTab.Document;
        if (doc.HasFilePath)
        {
            try
            {
                await _documentService.SaveAsync(doc.FilePath, SelectedTab.DisplayContent);
                doc.IsModified = false;
                SelectedTab.RefreshHeader();
                UpdateWindowTitle();
                StatusText = $"Saved: {doc.FileName}";
            }
            catch (Exception ex)
            {
                StatusText = $"Error saving: {ex.Message}";
            }
        }
        else
        {
            await SaveFileAsAsync();
        }
    }

    private async Task SaveFileAsAsync()
    {
        if (SelectedTab is null) return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "All Files (*.*)|*.*",
            FileName = SelectedTab.Document.FileName,
            InitialDirectory = _config.LastDirectory
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _documentService.SaveAsync(dialog.FileName, SelectedTab.DisplayContent);
                SelectedTab.Document.FilePath = dialog.FileName;
                SelectedTab.Document.FileName = Path.GetFileName(dialog.FileName);
                SelectedTab.Document.IsModified = false;
                SelectedTab.RefreshHeader();
                _config.LastDirectory = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                UpdateWindowTitle();
                StatusText = $"Saved: {SelectedTab.Document.FileName}";
            }
            catch (Exception ex)
            {
                StatusText = $"Error saving: {ex.Message}";
            }
        }
    }

    private async Task FormatDocumentAsync()
    {
        if (SelectedTab is null) return;

        var lang = SelectedTab.Document.Language;
        if (!_formattingService.CanFormat(lang))
        {
            StatusText = $"No formatter for {lang}";
            return;
        }

        var formatted = _formattingService.Format(SelectedTab.DisplayContent, lang);
        SelectedTab.DisplayContent = formatted;
        StatusText = $"Formatted: {lang}";
    }

    private void Exit()
    {
        System.Windows.Application.Current.MainWindow?.Close();
    }

    public void UpdateLanguage(string language)
    {
        if (SelectedTab is null) return;
        SelectedTab.Document.Language = language;
        _config.LastLanguage = language;
        UpdateLanguageStatus();
    }

    public void UpdateCursorStatus(int line, int column)
    {
        CursorStatus = $"Ln {line}, Col {column}";
    }

    private void UpdateLanguageStatus()
    {
        LanguageStatus = SelectedTab?.Document.Language ?? "PlainText";
    }

    private void UpdateWindowTitle()
    {
        if (SelectedTab is null)
        {
            WindowTitle = "RetroPad";
            return;
        }

        var prefix = SelectedTab.Document.IsModified ? "* " : "";
        WindowTitle = $"{prefix}{SelectedTab.Document.FileName} — RetroPad";
    }

    public async Task SaveSessionAsync()
    {
        try
        {
            var docs = Tabs.Select(t => t.Document).ToList();
            var activeIndex = SelectedTab is not null ? Tabs.IndexOf(SelectedTab) : 0;
            await _sessionService.SaveSessionAsync(docs, activeIndex);
            await _configService.SaveAsync(_config);
        }
        catch
        {
            // Silently fail on session save — not critical
        }
    }
}
