using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
            },
            SelectedIndex = 0
        };

        Placement = PlacementMode.Bottom;
        StaysOpen = false;
        Child = List;
        Focusable = false;

        List.PreviewKeyDown += List_PreviewKeyDown;
    }

    public ListBox List { get; }

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
            oldTextBox.PreviewKeyDown -= suggestionBox.ParentTarget_KeyDown;
            oldTextBox.PreviewKeyUp -= suggestionBox.ParentTarget_KeyUp;
        }
        if (e.NewValue is RichTextBox newTextBox)
        {
            newTextBox.TextChanged += suggestionBox.ParentTarget_TextChanged;
            newTextBox.PreviewKeyDown += suggestionBox.ParentTarget_KeyDown;
            newTextBox.PreviewKeyUp += suggestionBox.ParentTarget_KeyUp;

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

    private void ParentTarget_KeyDown(object sender, KeyEventArgs e)
    {
        if ((e.Key == Key.Space && e.ModifierPressed(ModifierKeys.Control)) ||
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
                ApplySuggestion();
                e.Handled = true;
                return;
        };
    }

    private void ParentTarget_KeyUp(object sender, KeyEventArgs e)
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

    public void ApplySuggestion()
    {
        if (List.Items.Count == 0 || List.SelectedIndex < 0)
            ApplySuggestion(string.Empty);

        ApplySuggestion((string)List.SelectedValue);
    }

    public void ApplySuggestion(string suggestion)
    {
        if (TargetTextBox is null)
            return;

        var range = GetWordAtTextPosition(TargetTextBox.CaretPosition);
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
        var currentWordStartPosition = GetWordAtTextPosition(TargetTextBox?.CaretPosition)?.Start;
        PlacementRectangle = currentWordStartPosition?.GetCharacterRect(LogicalDirection.Backward) ?? Rect.Empty;
    }

    public void Cancel()
    {
        _isNavigating = false;
        IsOpen = false;
        List.SelectedIndex = 0;
        TargetTextBox?.Focus();
    }

    private static TextRange? GetWordAtTextPosition(TextPointer? caret)
    {
        if (caret is null)
            return null;

        var leftPointer = GetTextPositionOfLastAlphaNumeric(caret, LogicalDirection.Backward);
        var rightPointer = GetTextPositionOfLastAlphaNumeric(caret, LogicalDirection.Forward);
        return new TextRange(leftPointer, rightPointer);
    }

    private static TextPointer GetTextPositionOfLastAlphaNumeric(TextPointer caret, LogicalDirection direction)
    {
        if (caret.GetLineStartPosition((int)direction) is not { } lineEnd)
            return caret;
        Debug.Assert(caret.IsAtInsertionPosition);
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
        var regexDirection = direction == LogicalDirection.Forward ? RegexOptions.None : RegexOptions.RightToLeft;
        var match = Regex.Match(wordRange.Text, "[^a-zA-Z0-9]", regexDirection);
        return match.Success
            ? wordRange.Start.GetPositionAtOffset(match.Index + (direction == LogicalDirection.Backward ? 1 : 0)) ?? wordEdge
            : wordEdge;
    }
    
    private bool _isNavigating = false;
}
