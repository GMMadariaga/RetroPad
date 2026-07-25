using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RetroPad.Infrastructure.Config;
using RetroPad.Infrastructure.Persistence;
using RetroPad.Infrastructure.Syntax;
using RetroPad.Infrastructure.Formatting;
using RetroPad.Application.Services;
using RetroPad.UI.Syntax;
using RetroPad.UI.ViewModels;

namespace RetroPad.UI.Views;

public partial class MainWindow : Window
{
    private MainViewModel _viewModel = null!;
    private AvalonEditSyntaxService _syntaxService = null!;
    private readonly Dictionary<TabViewModel, ICSharpCode.AvalonEdit.TextEditor> _editors = new();

    public MainWindow()
    {
        InitializeComponent();
        try
        {
            var iconUri = new Uri("pack://application:,,,/retropad.png", UriKind.Absolute);
            Icon = new System.Windows.Media.Imaging.BitmapImage(iconUri);
        }
        catch { }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var configStore = new JsonConfigStore();
        var docRepo = new FileDocumentRepository();
        var sessionStore = new FileSessionStore();
        var languageDetector = new LanguageDetector();
        var formatter = new CompositeCodeFormatter();

        var documentService = new DocumentService(docRepo);
        var sessionService = new SessionService(sessionStore, docRepo);
        var formattingService = new FormattingService(formatter);
        var configService = new ConfigService(configStore);

        _syntaxService = new AvalonEditSyntaxService(languageDetector);
        RetroEditorTheme.Apply();

        _viewModel = new MainViewModel(
            documentService,
            sessionService,
            formattingService,
            configService,
            languageDetector);

        DataContext = _viewModel;
        await _viewModel.InitializeAsync();

        if (_viewModel.SelectedTab is not null)
            ShowEditor(_viewModel.SelectedTab);
    }

    private async void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Save cursor positions before closing
        foreach (var (tab, editor) in _editors)
        {
            tab.Document.CursorOffset = editor.CaretOffset;
            editor.TextArea.Caret.PositionChanged -= Editor_CaretPositionChanged;
            editor.TextChanged -= OnEditorTextChanged;
        }

        if (_viewModel is not null)
            await _viewModel.SaveSessionAsync();
    }

    // ── Title Bar ──────────────────────────────────────────────

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            ToggleMaximize();
        else
            DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    // ── Editor Management ──────────────────────────────────────

    private void TabContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabViewModel newTab)
            ShowEditor(newTab);
    }

    private void ShowEditor(TabViewModel tab)
    {
        if (!_editors.TryGetValue(tab, out var editor))
        {
            editor = CreateEditor(tab);
            _editors[tab] = editor;
        }

        EditorHost.Child = editor;
    }

    private ICSharpCode.AvalonEdit.TextEditor CreateEditor(TabViewModel tab)
    {
        var editor = new ICSharpCode.AvalonEdit.TextEditor
        {
            FontFamily = new System.Windows.Media.FontFamily("Cascadia Mono, Consolas, JetBrains Mono, Courier New"),
            FontSize = 14,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xAA, 0xAA, 0xAA)),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x0A, 0x0A, 0x0A)),
            LineNumbersForeground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x33, 0x33, 0x33)),
            ShowLineNumbers = true,
            WordWrap = false,
            Text = tab.DisplayContent ?? string.Empty,
            SyntaxHighlighting = _syntaxService.GetDefinition(tab.Document.Language)
        };

        editor.Options.ShowBoxForControlCharacters = true;
        editor.Options.EnableHyperlinks = true;
        editor.Options.EnableEmailHyperlinks = true;

        if (tab.Document.CursorOffset > 0 && tab.Document.CursorOffset <= editor.Document.TextLength)
            editor.CaretOffset = tab.Document.CursorOffset;

        editor.TextChanged += OnEditorTextChanged;
        editor.TextArea.Caret.PositionChanged += Editor_CaretPositionChanged;

        // Sync format command (ViewModel changes DisplayContent -> editor must update)
        tab.DisplayContentChanged += OnTabDisplayContentChanged;

        return editor;
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (sender is not ICSharpCode.AvalonEdit.TextEditor editor) return;
        var tab = GetTabForEditor(editor);
        if (tab is not null)
        {
            tab.DisplayContentChanged -= OnTabDisplayContentChanged;
            tab.DisplayContent = editor.Text;
            tab.DisplayContentChanged += OnTabDisplayContentChanged;
        }
    }

    private void OnTabDisplayContentChanged(object? sender, EventArgs e)
    {
        if (sender is not TabViewModel tab) return;
        if (!_editors.TryGetValue(tab, out var editor)) return;

        var content = tab.DisplayContent ?? string.Empty;
        if (editor.Text != content)
        {
            editor.TextChanged -= OnEditorTextChanged;
            editor.Text = content;
            editor.TextChanged += OnEditorTextChanged;
        }
    }

    private TabViewModel? GetTabForEditor(ICSharpCode.AvalonEdit.TextEditor editor)
    {
        foreach (var (tab, ed) in _editors)
        {
            if (ed == editor) return tab;
        }
        return null;
    }

    private void Editor_CaretPositionChanged(object? sender, EventArgs e)
    {
        if (sender is ICSharpCode.AvalonEdit.Editing.Caret caret)
            _viewModel?.UpdateCursorStatus(caret.Line, caret.Column);
    }

    private ICSharpCode.AvalonEdit.TextEditor? GetActiveEditor()
    {
        if (_viewModel?.SelectedTab is null) return null;
        _editors.TryGetValue(_viewModel.SelectedTab, out var editor);
        return editor;
    }

    // ── Menu Actions ───────────────────────────────────────────

    private void Undo_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Undo();
    private void Redo_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Redo();
    private void Cut_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Cut();
    private void Copy_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Copy();
    private void Paste_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.Paste();
    private void SelectAll_Click(object sender, RoutedEventArgs e) => GetActiveEditor()?.SelectAll();

    private void Find_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor is null) return;
        new FindReplaceDialog(editor) { Owner = this }.Show();
    }

    private void Replace_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor is null) return;
        new FindReplaceDialog(editor) { Owner = this }.Show();
    }

    private void GoToLine_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor is null) return;
        new GoToLineDialog(editor) { Owner = this }.ShowDialog();
    }

    private void DuplicateLine_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor is null) return;
        var line = editor.Document.GetLineByOffset(editor.CaretOffset);
        editor.Document.Insert(line.EndOffset, Environment.NewLine + editor.Document.GetText(line));
    }

    private void DeleteLine_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor is null) return;
        editor.Document.Remove(editor.Document.GetLineByOffset(editor.CaretOffset));
    }

    private void MoveLineUp_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor is null) return;
        var line = editor.Document.GetLineByOffset(editor.CaretOffset);
        if (line.LineNumber <= 1) return;
        var prev = editor.Document.GetLineByNumber(line.LineNumber - 1);
        var a = editor.Document.GetText(line);
        var b = editor.Document.GetText(prev);
        editor.Document.Replace(line, b);
        editor.Document.Replace(prev, a);
    }

    private void MoveLineDown_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor is null) return;
        var line = editor.Document.GetLineByOffset(editor.CaretOffset);
        if (line.LineNumber >= editor.Document.LineCount) return;
        var next = editor.Document.GetLineByNumber(line.LineNumber + 1);
        var a = editor.Document.GetText(line);
        var b = editor.Document.GetText(next);
        editor.Document.Replace(line, b);
        editor.Document.Replace(next, a);
    }

    private void CloseTab_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not TabViewModel tab) return;
        if (_editors.TryGetValue(tab, out var editor))
        {
            editor.TextArea.Caret.PositionChanged -= Editor_CaretPositionChanged;
            editor.TextChanged -= OnEditorTextChanged;
            tab.DisplayContentChanged -= OnTabDisplayContentChanged;
            _editors.Remove(tab);
        }
        _viewModel?.CloseTabCommand.Execute(tab);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            Find_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.H && Keyboard.Modifiers == ModifierKeys.Control)
        {
            Replace_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.G && Keyboard.Modifiers == ModifierKeys.Control)
        {
            GoToLine_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
        {
            DuplicateLine_Click(sender, e);
            e.Handled = true;
        }
    }

    private void Language_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem mi || mi.Header is not string lang) return;
        _viewModel?.UpdateLanguage(lang);

        if (_viewModel?.SelectedTab is not null && _editors.TryGetValue(_viewModel.SelectedTab, out var editor))
            editor.SyntaxHighlighting = _syntaxService.GetDefinition(lang);
    }
}
