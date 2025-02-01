using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ShaderIDE.Controls;

public class SyntaxTextBox : RichTextBox
{
    public SyntaxTextBox()
    {
        AcceptsTab = true;
        AcceptsReturn = true;
        Document.LineHeight = 1;
        FontFamily = new FontFamily("Consolas");
        TextChanged += SyntaxTextBox_TextChanged;
        SetBinding(BackgroundProperty, new Binding($"{nameof(ColorScheme)}.{nameof(ColorScheme.Background)}") { Source = this });
    }

    public static readonly DependencyProperty ColorSchemeProperty =
        DependencyProperty.Register(nameof(ColorScheme), typeof(ColorScheme),
        typeof(SyntaxTextBox), new UIPropertyMetadata(default, new PropertyChangedCallback(colorSchemeChanged)));

    private static void colorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SyntaxTextBox)d).ApplySyntaxHighlighting();
    }

    public ColorScheme ColorScheme
    {
        get => (ColorScheme)GetValue(ColorSchemeProperty);
        set => SetValue(ColorSchemeProperty, value);
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
        }
        finally
        {
            EndChange();
            TextChanged += SyntaxTextBox_TextChanged;
        }
    }

    private void ResetStyle(TextRange range)
    {
        range.ApplyPropertyValue(TextElement.ForegroundProperty, ColorScheme.Foreground);
        range.ApplyPropertyValue(TextElement.BackgroundProperty, null);
        range.ApplyPropertyValue(TextElement.FontFamilyProperty, FontFamily);
        range.ApplyPropertyValue(TextElement.FontSizeProperty, FontSize);
    }

    private void ApplySyntaxHighlighting()
    {
        TextRange textBoxRange = TextRange;
        ResetStyle(textBoxRange);

        var text = textBoxRange.Text;
        TextPointer startPointer = textBoxRange.Start;
        int offset = 0;
        string token = string.Empty;

        bool goToEol = false;

        for (var index = 0; index < text.Length; index++)
        {
            char nextChar = text[index];
            if (char.IsLetterOrDigit(nextChar) || nextChar == '_' || nextChar == '#')
            {
                token += nextChar;
                continue;
            }

            if (nextChar == '/' && text.Length > index + 1 && text[index + 1] == '/')
            {
                goToEol = true;
                token += nextChar;
                continue;
            }

            if (goToEol && !Environment.NewLine.Contains(nextChar))
            {
                token += nextChar;
                continue;
            }

            var tokenType = GetTokenType(token);
            if (tokenType != TokenType.None && GetTextRangeOfToken(startPointer, offset, token.Length) is { } tokenRange)
            {
                tokenRange.ApplyPropertyValue(TextElement.ForegroundProperty, ColorScheme.GetColor(tokenType));
                startPointer = tokenRange.Start;
                offset = 0;
            }

            offset += token.Length;
            token = string.Empty;

            if (!Environment.NewLine.Contains(nextChar))
            {
                offset++;
            } 
            else
            {
                goToEol = false;
            }
        }
    }

    private static TokenType GetTokenType(string token)
    {
        if (token.StartsWith("//") || token.StartsWith("#"))
            return TokenType.Comments;

        foreach(var (type, values) in SyntaxMapping.OpenGL.Tokens)
        {
            if (values.Contains(token))
                return type;
        }
        return TokenType.None;
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

    public static bool IsFunctionKey(this Key key)
    {
        int keyValue = (int)key;
        return keyValue >= (int)Key.F1 && keyValue <= (int)Key.F24;
    }
}
