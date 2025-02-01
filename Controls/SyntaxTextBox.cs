using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
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

    private void ApplySyntaxHighlighting()
    {
        TextRange textBoxRange = TextRange;
        textBoxRange.ApplyPropertyValue(TextElement.ForegroundProperty, ColorScheme[TokenType.None]);

        var text = textBoxRange.Text;
        TextPointer startPointer = textBoxRange.Start;
        int offset = 0;
        string token = string.Empty;

        for (var index = 0; index < text.Length; index++)
        {
            char nextChar = text[index];
            if (char.IsLetterOrDigit(nextChar) || nextChar == '_')
            {
                token += nextChar;
                continue;
            }

            var tokenType = GetTokenType(token);
            if (tokenType != TokenType.None && GetTextRangeOfToken(startPointer, offset, token.Length) is { } tokenRange)
            {
                tokenRange.ApplyPropertyValue(TextElement.ForegroundProperty, ColorScheme[tokenType]);
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

    private static TokenType GetTokenType(string token)
    {
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

    internal static readonly IReadOnlyDictionary<TokenType, SolidColorBrush> ColorScheme = new Dictionary<TokenType, SolidColorBrush>
    {
        {TokenType.None, new SolidColorBrush(Colors.Black) },
        {TokenType.Keyword, new SolidColorBrush(Colors.Blue) },
        {TokenType.DataType, new SolidColorBrush(Colors.Brown) },
        {TokenType.SpecialVariable, new SolidColorBrush(Colors.YellowGreen) },
        {TokenType.IntrinsicMethod, new SolidColorBrush(Colors.Red) },
    };


    private static readonly SolidColorBrush _color = new(Colors.Black);
    private static readonly SolidColorBrush _colorBlue = new(Colors.Blue);
    private static readonly SolidColorBrush _colorGray = new(Colors.Blue);
    private static readonly SolidColorBrush _colorGren = new(Colors.Green);
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
