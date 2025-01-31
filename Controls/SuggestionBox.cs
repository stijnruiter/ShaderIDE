using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ShaderIDE.Controls;

internal class SuggestionBox : Popup
{
    public SuggestionBox()
    {
        List = new ListBox()
        {
            Foreground = Brushes.Black,
            Focusable = false,
            ItemsSource = new string[] {
                "test",
                "tes2",
                "test3"
            }
        };

        Placement = PlacementMode.Bottom;
        StaysOpen = false;
        Child = List;
        Focusable = false;

        List.PreviewKeyDown += List_PreviewKeyDown;
    }

    [Bindable(true)]
    public RichTextBox TargetTextBox
    {
        get => (RichTextBox)GetValue(TargetTextBoxProperty);
        set => SetValue(TargetTextBoxProperty, value);
    }

    public static readonly DependencyProperty TargetTextBoxProperty = DependencyProperty.Register(
                                                nameof(TargetTextBox), 
                                                typeof(RichTextBox), 
                                                typeof(SuggestionBox), 
                                                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(AttachTextBoxEvents)));

    private static void AttachTextBoxEvents(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var suggestionBox = (SuggestionBox)d;
        if (e.OldValue is RichTextBox oldTextBox)
        {
            oldTextBox.TextChanged -= suggestionBox.ParentTarget_TextChanged;
            oldTextBox.PreviewKeyDown -= suggestionBox.ParentTarget_PreviewKeyDown;
            oldTextBox.PreviewKeyUp -= suggestionBox.ParentTarget_KeyUp;
            oldTextBox.KeyUp -= suggestionBox.ParentTarget_KeyUp;
        }
        if (e.NewValue is RichTextBox newTextBox)
        {
            newTextBox.TextChanged += suggestionBox.ParentTarget_TextChanged;
            newTextBox.PreviewKeyDown += suggestionBox.ParentTarget_PreviewKeyDown;
            newTextBox.PreviewKeyUp += suggestionBox.ParentTarget_KeyUp;
            newTextBox.KeyUp += suggestionBox.ParentTarget_KeyUp;
            suggestionBox.SetValue(PlacementTargetProperty, newTextBox);
        }
    }

    private void List_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsOpen)
            return;

        switch (e.Key)
        {
            case Key.Tab:
            case Key.Enter:
                ApplySuggestion();
                e.Handled = true;
                return;
        }
    }

    private void ParentTarget_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsOpen)
            return;

        switch (e.Key)
        {
            case Key.Tab:
            case Key.Enter when _isNavigating:
            case Key.Up when _isNavigating:
            case Key.Down:
                e.Handled = true;
                return;
        }
    }

    private void ParentTarget_KeyUp(object sender, KeyEventArgs e)
    {
        if ((e.Key == Key.Space && e.ModifierPressed(ModifierKeys.Control)) ||
            (e.Key == Key.LeftCtrl && e.KeyboardDevice.IsKeyDown(Key.Space)) ||
            (e.Key.IsAlphaNumeric() && !e.ModifierPressed(ModifierKeys.Control)))
        {
            Show();
            return;
        }

        if (!IsOpen)
            return;

        switch (e.Key)
        {
            case Key.Up when _isNavigating:
                PreviousSuggestion();
                e.Handled = true;
                return;
            case Key.Down:
                NextSuggestion();
                e.Handled = true;
                return;
            case Key.Up:
            case Key.Left:
            case Key.Right:
            case Key.Escape:
                Cancel();
                return;
            case Key.Enter when _isNavigating:
            case Key.Tab:
                ApplySuggestion((string)(List.SelectedItem ?? string.Empty));
                e.Handled = true;
                return;
        };
    }

    private void ParentTarget_KeyDown(object sender, KeyEventArgs e)
    {
        if (!IsOpen)
            return;

        switch (e.Key)
        {
            case Key.Tab:
            case Key.Enter when _isNavigating:
            case Key.Up when _isNavigating:
            case Key.Down:
                e.Handled = true;
                return;
        }
    }

    private void ParentTarget_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdatePosition();
    }

    private void SuggestionBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!IsOpen)
            return;

        switch (e.Key)
        {
            case Key.Up when _isNavigating:
            case Key.Down:
                e.Handled = true;
                return;
            case Key.Tab:
            case Key.Enter:
                ApplySuggestion((string)List.SelectedItem);
                e.Handled = true;
                return;
        }
    }

    public void ApplySuggestion()
    {
        ApplySuggestion((string)List.SelectedValue ?? string.Empty);
    }


    public void ApplySuggestion(string suggestion)
    {
        if (TargetTextBox is null)
            return;

        var range = GetWordRange(TargetTextBox.CaretPosition);
        if (range is null)
            return;

        range.Text = suggestion;
        TargetTextBox.CaretPosition = range.End;
        Cancel();
    }

    public void NextSuggestion()
    {
        _isNavigating = true;
        List.SelectedIndex = (List.SelectedIndex + 1) % List.Items.Count;
    }

    public void PreviousSuggestion()
    {
        _isNavigating = true;
        List.SelectedIndex = (List.Items.Count + List.SelectedIndex - 1) % List.Items.Count;
    }

    public void Show()
    {
        UpdatePosition();
        IsOpen = !PlacementRectangle.IsEmpty;
    }

    public void UpdatePosition()
    {
        PlacementRectangle = ComputePopupPlacement(TargetTextBox?.CaretPosition);
    }

    public void Cancel()
    {
        _isNavigating = false;
        IsOpen = false;
        List.SelectedIndex = 0;
        TargetTextBox?.Focus();
    }

    private static Rect ComputePopupPlacement(TextPointer? caret)
        => GetWordRange(caret)?.Start?.GetCharacterRect(LogicalDirection.Backward) ?? Rect.Empty;

    private static TextRange? GetWordRange(TextPointer? caret)
    {
        if (caret is null)
            return null;

        var leftPointer = GetTextPointerOfLastAlphaNumeric(caret, LogicalDirection.Backward);
        var rightPointer = GetTextPointerOfLastAlphaNumeric(caret, LogicalDirection.Forward);
        return new TextRange(leftPointer, rightPointer);
    }

    private static TextPointer GetTextPointerOfLastAlphaNumeric(TextPointer caret, LogicalDirection direction)
    {
        if (caret.GetLineStartPosition((int)direction) is not { } lineEnd)
            return caret;

        var wordEdge = caret;
        char[] textbuffer = new char[1];
        while (true)
        {
            if (wordEdge is null || lineEnd.CompareTo(wordEdge) != (int)direction * 2 - 1) // The caret passed the next line
            {
                wordEdge = lineEnd;
                break;
            }

            wordEdge.GetTextInRun(direction, textbuffer, 0, 1);
            if (!char.IsLetterOrDigit(textbuffer[0]) && textbuffer[0] != '\0')
                break;

            wordEdge = wordEdge.GetNextInsertionPosition(direction);
        }

        // For some reason, it sometimes happens that there is still a non-alphanumeric character..
        var wordRange = new TextRange(wordEdge, caret);
        var regexDirection = direction == LogicalDirection.Forward ? RegexOptions.RightToLeft : RegexOptions.None;
        var match = Regex.Match(wordRange.Text, "[^a-zA-Z0-9]", regexDirection);
        return match.Success ? wordRange.Start.GetPositionAtOffset(match.Index) ?? wordEdge : wordEdge;
    }
    
    public ListBox List { get; }
    private bool _isNavigating = false;
}
