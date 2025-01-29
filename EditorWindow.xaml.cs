using OpenTK.Wpf;
using ShaderIDE.Render;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ShaderIDE;

public partial class EditorWindow : Window
{
    public EditorWindow()
    {
        InitializeComponent();
        OpenTkControl.Start(new GLWpfControlSettings
        {
            MajorVersion = 3,
            MinorVersion = 3
        });
        RichTextBoxFragmentShader.AppendText(Shader.DefaultFragmentShader);
        _canvas = new RenderCanvas();
    }

    private void OpenTkControl_OnRender(TimeSpan delta)
    {
        _canvas.Clear();
        _canvas.Draw(delta);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var content = new TextRange(RichTextBoxFragmentShader.Document.ContentStart, RichTextBoxFragmentShader.Document.ContentEnd);
        _canvas.UpdateShader(Shader.DefaultVertexShader, content.Text);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _canvas.Dispose();
    }

    private void RichTextBoxFragmentShader_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            RichTextBoxFragmentShader.TextChanged -= RichTextBoxFragmentShader_TextChanged;
            RichTextBoxFragmentShader.BeginChange();
            ApplySyntaxHighlighting(RichTextBoxFragmentShader);
        }
        finally
        {
            RichTextBoxFragmentShader.EndChange();
            RichTextBoxFragmentShader.TextChanged += RichTextBoxFragmentShader_TextChanged;
        }
    }

    private void ApplySyntaxHighlighting(RichTextBox richTextBox)
    {
        TextRange textBoxRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
        textBoxRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));

        var text = textBoxRange.Text;
        int offset = 0;
        string token = string.Empty;
        TextPointer startPointer = textBoxRange.Start;
        for (var index = 0; index < text.Length; index++)
        {
            char nextChar = text[index];
            if (char.IsLetterOrDigit(nextChar))
            {
                token += nextChar;
                continue;
            }

            if (GetTokenColor(token) is { } color && GetTextRangeOfToken(startPointer, offset, token.Length) is { } tokenRange)
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

    private SolidColorBrush? GetTokenColor(string token)
    {
        if (KeyWords.Any(s => s.Equals(token, StringComparison.InvariantCultureIgnoreCase)))
            return new SolidColorBrush(Colors.Blue);
        
        if (DataTypes.Any(s => s.Equals(token, StringComparison.InvariantCultureIgnoreCase)))
            return new SolidColorBrush(Colors.Red);
        
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


    private readonly RenderCanvas _canvas;
    private static string[] KeyWords = { "void", "out"};
    private static string[] DataTypes = { "vec3", "vec4", "int"};
}