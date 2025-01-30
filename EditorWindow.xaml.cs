using OpenTK.Wpf;
using ShaderIDE.Render;
using System;
using System.Windows;

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
        fragmentShaderTextBox.Text = Shader.DefaultFragmentShader;
        _canvas = new RenderCanvas();
    }

    private void OpenTkControl_OnRender(TimeSpan delta)
    {
        _canvas.Clear();
        _canvas.Draw(delta);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _canvas.UpdateShader(Shader.DefaultVertexShader, fragmentShaderTextBox.Text);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _canvas.Dispose();
    }

    private readonly RenderCanvas _canvas;
}