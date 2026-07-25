using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using RetroPad.UI.ViewModels;

namespace RetroPad.UI.Views;

public partial class GoToLineDialog : Window
{
    private readonly GoToLineViewModel _viewModel;
    private readonly ICSharpCode.AvalonEdit.TextEditor _editor;

    public GoToLineDialog(ICSharpCode.AvalonEdit.TextEditor editor)
    {
        InitializeComponent();
        _editor = editor;
        _viewModel = new GoToLineViewModel
        {
            MaxLine = editor.Document.LineCount
        };
        DataContext = _viewModel;

        LineInput.Focus();
        LineInput.SelectAll();
    }

    public int? SelectedLine { get; private set; }

    private void Go_Click(object sender, RoutedEventArgs e)
    {
        GoToLine();
    }

    private void GoToLine()
    {
        var line = _viewModel.GetLineNumber();
        if (line is null)
        {
            _viewModel.StatusText = "Invalid line number";
            return;
        }

        SelectedLine = line.Value;
        _editor.ScrollToLine(line.Value);
        _editor.TextArea.Caret.Line = line.Value;
        _editor.TextArea.Caret.Column = 1;
        Close();
    }

    private void LineInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            GoToLine();
        else if (e.Key == Key.Escape)
            Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
