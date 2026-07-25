using System.Text.RegularExpressions;
using System.Windows;
using ICSharpCode.AvalonEdit;
using RetroPad.UI.ViewModels;

namespace RetroPad.UI.Views;

public partial class FindReplaceDialog : Window
{
    private readonly FindReplaceViewModel _viewModel;
    private readonly ICSharpCode.AvalonEdit.TextEditor _editor;

    public FindReplaceDialog(ICSharpCode.AvalonEdit.TextEditor editor)
    {
        InitializeComponent();
        _editor = editor;
        _viewModel = new FindReplaceViewModel();
        DataContext = _viewModel;

        SearchBox.Focus();
    }

    private void FindNext_Click(object sender, RoutedEventArgs e)
    {
        Find(reverse: false);
    }

    private void FindPrevious_Click(object sender, RoutedEventArgs e)
    {
        Find(reverse: true);
    }

    private void Find(bool reverse)
    {
        if (string.IsNullOrEmpty(_viewModel.SearchText))
        {
            _viewModel.StatusText = "Enter search text";
            return;
        }

        var startOffset = reverse
            ? _editor.SelectionStart
            : _editor.SelectionStart + _editor.SelectionLength;

        var text = _editor.Text;
        var search = _viewModel.SearchText;

        var comparison = _viewModel.MatchCase
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        int index;
        if (reverse)
        {
            var searchText = text[..Math.Min(startOffset, text.Length)];
            index = searchText.LastIndexOf(search, comparison);
        }
        else
        {
            index = text.IndexOf(search, startOffset, comparison);
            if (index < 0 && startOffset > 0)
                index = text[..startOffset].IndexOf(search, comparison);
        }

        if (_viewModel.WholeWord && index >= 0)
        {
            if (index > 0 && char.IsLetterOrDigit(text[index - 1]))
            {
                Find(reverse);
                return;
            }
            if (index + search.Length < text.Length && char.IsLetterOrDigit(text[index + search.Length]))
            {
                Find(reverse);
                return;
            }
        }

        if (index >= 0)
        {
            _editor.Select(index, search.Length);
            _editor.ScrollToLine(_editor.Document.GetLineByOffset(index).LineNumber);
            _editor.TextArea.Caret.Offset = index;
            _viewModel.StatusText = string.Empty;
        }
        else
        {
            _viewModel.StatusText = "Not found";
        }
    }

    private void Replace_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_viewModel.SearchText)) return;

        var selected = _editor.SelectedText;
        var comparison = _viewModel.MatchCase
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        if (string.Equals(selected, _viewModel.SearchText, comparison))
        {
            var offset = _editor.SelectionStart;
            _editor.Document.Replace(offset, selected.Length, _viewModel.ReplaceText);
            _editor.Select(offset, _viewModel.ReplaceText.Length);
        }

        FindNext_Click(sender, e);
    }

    private void ReplaceAll_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_viewModel.SearchText)) return;

        var text = _editor.Text;
        var search = _viewModel.SearchText;
        var replacement = _viewModel.ReplaceText;

        var comparison = _viewModel.MatchCase
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        int count = 0;
        int index = 0;
        var result = new System.Text.StringBuilder();

        while (index <= text.Length)
        {
            var found = text.IndexOf(search, index, comparison);
            if (found < 0)
            {
                result.Append(text[index..]);
                break;
            }

            if (_viewModel.WholeWord)
            {
                if (found > 0 && char.IsLetterOrDigit(text[found - 1]))
                {
                    result.Append(text[index..(found + search.Length)]);
                    index = found + search.Length;
                    continue;
                }
                if (found + search.Length < text.Length && char.IsLetterOrDigit(text[found + search.Length]))
                {
                    result.Append(text[index..(found + search.Length)]);
                    index = found + search.Length;
                    continue;
                }
            }

            result.Append(text[index..found]);
            result.Append(replacement);
            index = found + search.Length;
            count++;
        }

        _editor.Text = result.ToString();
        _viewModel.StatusText = $"Replaced {count} occurrence(s)";
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
