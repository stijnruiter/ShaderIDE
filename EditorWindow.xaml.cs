using OpenTK.Wpf;
using ShaderIDE.Render;
using System;
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
        _canvas.UpdateShader(Shader.DefaultVertexShader, TextRange(RichTextBoxFragmentShader).Text);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _canvas.Dispose();
    }

    private readonly RenderCanvas _canvas;

    private bool ignoreTextChanges = false;

    private void RichTextBoxFragmentShader_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ignoreTextChanges)
            return;

        ignoreTextChanges = true;
        RichTextBoxFragmentShader.BeginChange();
        try
        {
            HighlightXml(RichTextBoxFragmentShader);
        }
        finally
        {
            RichTextBoxFragmentShader.EndChange();
            ignoreTextChanges = false;
        }
    }

    private static TextRange TextRange(RichTextBox richTextBox)
    {
        return new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
    }

    public static void HighlightXml(RichTextBox richTextBox)
    {
        var newDocument = richTextBox.Document;
        var range = new TextRange(newDocument.ContentStart, newDocument.ContentEnd);
        range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));

        var position = newDocument.ContentStart;
        while(position != null && position.CompareTo(newDocument.ContentEnd) <= 0)
        {
            if (position.CompareTo(newDocument.ContentEnd) == 0)
                return;

            var textRun = position.GetTextInRun(LogicalDirection.Forward);
            foreach(var search in KeyWords)
            {
                var indexInRun = textRun.IndexOf(search, StringComparison.CurrentCulture);
                if (indexInRun >= 0)
                {
                    var keywordPos = position.GetPositionAtOffset(indexInRun, LogicalDirection.Forward);
                    if (keywordPos != null)
                    {
                        var nextPointer = keywordPos.GetPositionAtOffset(search.Length);
                        var textRange = new TextRange(keywordPos, nextPointer);
                        textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
                    }
                }
            }
            position = position!.GetNextContextPosition(LogicalDirection.Forward);
        }
    }

    private static string[] KeyWords = { "void", "out"};
}