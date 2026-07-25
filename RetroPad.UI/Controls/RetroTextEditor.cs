using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace RetroPad.UI.Controls;

public class RetroTextEditor : TextEditor
{
    public Action<string>? OnContentPasted { get; set; }

    private int _prePasteLength;

    public void PrepareForPaste()
    {
        _prePasteLength = Document?.TextLength ?? 0;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        // Subscribe to PreviewKeyDown on the TextArea (fires before CommandBinding)
        TextArea.PreviewKeyDown += TextArea_PreviewKeyDown;
    }

    private void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
        {
            _prePasteLength = Document?.TextLength ?? 0;
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);

        if (_prePasteLength > 0)
        {
            var currentLength = Document?.TextLength ?? 0;
            if (currentLength > _prePasteLength + 3) // more than a few chars = paste
            {
                var content = Document?.Text ?? string.Empty;
                OnContentPasted?.Invoke(content);
            }
            _prePasteLength = 0;
        }
    }
}
