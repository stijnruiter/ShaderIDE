using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ShaderIDE.Controls;

public class SyntaxTextBox : RichTextBox
{
    public SyntaxTextBox()
    {
        SuggestionBox = new Popup()
        {
            PlacementTarget = this,
            Placement = PlacementMode.Bottom,
            StaysOpen = false,
            Child = SuggestionItemList,
            Focusable = false
        };

        AcceptsTab = true;
        AcceptsReturn = true;
        Document.LineHeight = 1;
        FontFamily = new FontFamily("Consolas");
        TextChanged += SyntaxTextBox_TextChanged;
        PreviewKeyUp += SyntaxTextBox_KeyUp;
        KeyUp += SyntaxTextBox_KeyUp;
        PreviewKeyDown += SyntaxTextBox_PreviewKeyDown;
        SuggestionItemList.KeyDown += SuggestionItemList_KeyDown;
    }

    public TextRange TextRange => new(Document.ContentStart, Document.ContentEnd);

    public string Text
    {
        get => TextRange.Text;
        set => TextRange.Text = value;
    }

    private void SyntaxTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            TextChanged -= SyntaxTextBox_TextChanged;
            BeginChange();
            ApplySyntaxHighlighting();
            UpdateSuggestionBoxPosition();
        }
        finally
        {
            EndChange();
            TextChanged += SyntaxTextBox_TextChanged;
        }
    }

    private void SyntaxTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if ((e.Key == Key.Space && e.ModifierPressed(ModifierKeys.Control)) ||
            (e.Key == Key.LeftCtrl && e.KeyboardDevice.IsKeyDown(Key.Space)) ||
            e.Key.IsAlphaNumeric())
        {
            ShowSuggestionBox();
            return;
        }

        if (!SuggestionBox.IsOpen)
            return;

        switch (e.Key)
        {
            case Key.Up:
                SuggestionItemList.SelectedIndex = (SuggestionItemList.Items.Count + SuggestionItemList.SelectedIndex - 1) % SuggestionItemList.Items.Count;
                e.Handled = true;
                return;
            case Key.Down:
                SuggestionItemList.SelectedIndex = (SuggestionItemList.SelectedIndex + 1) % SuggestionItemList.Items.Count;
                e.Handled = true;
                return;
            case Key.Left:
            case Key.Right:
            case Key.Escape:
                ClosePopup();
                return;
            case Key.Enter:
            case Key.Tab:
                ApplySuggestion((string)(SuggestionItemList.SelectedItem ?? SuggestionItemList.Items[0]));
                e.Handled = true;
                return;
        };
    }


    private void SuggestionItemList_KeyDown(object sender, KeyEventArgs e)
    {
        if (!SuggestionBox.IsOpen)
            return;

        switch(e.Key)
        {
            case Key.Tab:
            case Key.Enter:
                ApplySuggestion((string)SuggestionItemList.SelectedItem);
                ClosePopup();
                e.Handled = true;
                return;
        }
    }

    private void SyntaxTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!SuggestionBox.IsOpen)
            return;

        switch (e.Key)
        {
            case Key.Enter:
            case Key.Tab:
            case Key.Up:
            case Key.Down:
                e.Handled = true;
                return;
        }
    }

    private void ApplySuggestion(string suggestion)
    {
        ClosePopup();

        var range = GetWordRange(CaretPosition);
        if (range is null)
            return;
           
        range.Text = suggestion;
        CaretPosition = range.End;
        Focus();
    }


    private void ApplySyntaxHighlighting()
    {
        TextRange textBoxRange = TextRange;
        textBoxRange.ApplyPropertyValue(TextElement.ForegroundProperty, _colorBlack);

        var text = textBoxRange.Text;
        TextPointer startPointer = textBoxRange.Start;
        int offset = 0;
        string token = string.Empty;

        for (var index = 0; index < text.Length; index++)
        {
            char nextChar = text[index];
            if (char.IsLetterOrDigit(nextChar))
            {
                token += nextChar;
                continue;
            }

            if (GetTokenForegroundColor(token) is { } color && GetTextRangeOfToken(startPointer, offset, token.Length) is { } tokenRange)
            {
                tokenRange.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                startPointer = tokenRange.Start;
                offset = 0;
            }

            offset += token.Length;
            token = string.Empty;

            if (!Environment.NewLine.Contains(nextChar))
            {
                offset++;
            }
        }
    }

    private static SolidColorBrush? GetTokenForegroundColor(string token)
    {
        if (_keyWords.Any(s => s.Equals(token, StringComparison.InvariantCultureIgnoreCase)))
            return _colorBlue;

        if (_dataTypes.Any(s => s.Equals(token, StringComparison.InvariantCultureIgnoreCase)))
            return _colorRed;

        return null;
    }

    private static TextRange GetTextRangeOfToken(TextPointer startPointer, int offset, int characterCount)
    {
        var start = GetTextPositionAtOffset(startPointer, offset);
        if (start is null)
        {
            Debug.Fail("Start TextPointer not found.");
            return null;
        }
        var end = GetTextPositionAtOffset(start, characterCount);
        if (start is null)
        {
            Debug.Fail("End TextPointer not found.");
            return null;
        }
        return new TextRange(start, end);
    }
    
    private static TextPointer? GetTextPositionAtOffset(TextPointer position, int characterCount)
    {
        while (position != null)
        {
            if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                int count = position.GetTextRunLength(LogicalDirection.Forward);
                if (characterCount <= count)
                {
                    return position.GetPositionAtOffset(characterCount);
                }

                characterCount -= count;
            }

            TextPointer nextContextPosition = position.GetNextContextPosition(LogicalDirection.Forward);
            if (nextContextPosition == null)
                return position;

            position = nextContextPosition;
        }

        return position;
    }

    public void ClosePopup()
    {
        SuggestionBox.IsOpen = false;
        SuggestionItemList.SelectedIndex = 0;
    }

    public void ShowSuggestionBox()
    {
        UpdateSuggestionBoxPosition();
        SuggestionBox.IsOpen = !SuggestionBox.PlacementRectangle.IsEmpty;
    }

    public void UpdateSuggestionBoxPosition()
    {
        SuggestionBox.PlacementRectangle = ComputePopupPlacement(CaretPosition);
    }

    private static Rect ComputePopupPlacement(TextPointer caret) 
        => GetWordRange(caret)?.Start?.GetCharacterRect(LogicalDirection.Backward) ?? Rect.Empty;

    private static TextRange? GetWordRange(TextPointer caret)
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
        while(true)
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

    private readonly Popup SuggestionBox;
    private readonly ListBox SuggestionItemList = new ListBox()
    {
        Foreground = Brushes.Black,
        ItemsSource = _keyWords.Concat(_dataTypes),
        Focusable = false
    };

    private static readonly SolidColorBrush _colorBlack = new(Colors.Black);
    private static readonly SolidColorBrush _colorBlue = new(Colors.Blue);
    private static readonly SolidColorBrush _colorRed = new(Colors.Red);

    private static readonly string[] _keyWords = { "void", "out" };
    private static readonly string[] _dataTypes = { "vec3", "vec4", "int" };
}

public static class KeyEventExtension
{
    public static bool ModifierPressed(this KeyboardEventArgs args, ModifierKeys modifierKey) 
        => (args.KeyboardDevice.Modifiers & modifierKey) == modifierKey;

    public static LogicalDirection Reversed(this LogicalDirection direction)
        => direction == LogicalDirection.Backward ? LogicalDirection.Forward : LogicalDirection.Backward;

    public static bool IsAlphaNumeric(this Key key)
    {
        int keyValue = (int)key;
        return ((keyValue >= (int)Key.D0 && keyValue <= (int)Key.Z) || 
            (keyValue >= (int)Key.NumPad0 && keyValue <= (int)Key.NumPad9)); 
    }
}
