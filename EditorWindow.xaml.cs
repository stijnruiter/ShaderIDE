using Microsoft.Win32;
using OpenTK.Wpf;
using ShaderIDE.Render;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
        _modified = false;
        _canvas = new RenderCanvas();

        KeyDown += EditorWindow_KeyDown;
    }

    private void EditorWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.F5)
        {
            _canvas.UpdateShader(Shader.DefaultVertexShader, fragmentShaderTextBox.Text);
        }
    }

    private void OpenTkControl_OnRender(TimeSpan delta)
    {
        _canvas.Clear();
        _canvas.Draw(delta);
    }

    private void Compile_Click(object sender, RoutedEventArgs e)
    {
        _canvas.UpdateShader(Shader.DefaultVertexShader, fragmentShaderTextBox.Text);
    }

    private bool ConfirmCanOverwriteTextBox()
    {
        if (_modified)
        {
            var result = MessageBox.Show("Any unsaved changes will be lost.", "Are you sure you want to continue?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return false;
        }
        return true;
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmCanOverwriteTextBox())
            return;

        fragmentShaderTextBox.Text = Shader.DefaultFragmentShader;
        _modified = false;
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmCanOverwriteTextBox())
            return;

        OpenFileDialog dialog = new OpenFileDialog();
        PrepareFileDialog(dialog);

        if (dialog?.ShowDialog() != true)
            return;
        fragmentShaderTextBox.Text = File.ReadAllText(dialog.FileName);
        _modified = false;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new SaveFileDialog();
        PrepareFileDialog(dialog);
        if ((dialog?.ShowDialog()) != true)
            return;
        
        File.WriteAllText(dialog.FileName, fragmentShaderTextBox.Text);
        _modified = false;
    }

    public void PrepareFileDialog(FileDialog dialog)
    {
        dialog.Filter = "OpenGL Files|*.glsl;*.vert;*.tesc;*.tese;*.geom;*.frag;*.comp|All Files|*.*";
        dialog.DefaultExt = "glsl";
        if (dialog is OpenFileDialog openFileDialog)
        {
            openFileDialog.Multiselect = false;
        }
    }

    private void fragmentShaderTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _modified = true;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _canvas.Dispose();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if ((sender as ComboBox)?.SelectedItem is not ColorScheme scheme)
            return;

        ColorSchemeManager.SetScheme(scheme);
    }


    private readonly RenderCanvas _canvas;
    private bool _modified = false;

}