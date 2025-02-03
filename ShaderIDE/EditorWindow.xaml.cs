using Microsoft.Win32;
using OpenTK.Wpf;
using ShaderIDE.Data;
using ShaderIDE.Render;
using System;
using System.IO;
using System.Windows;

namespace ShaderIDE;

public partial class EditorWindow : Window
{
    public EditorWindow(Preferences preferences, ColorSchemeManager colorSchemeManager)
    {
        _preferences = preferences;
        ColorSchemeManager = colorSchemeManager;
        InitializeComponent();
        OpenTkControl.Start(new GLWpfControlSettings
        {
            MajorVersion = 3,
            MinorVersion = 3
        });
        _canvas = new RenderCanvas();

        LoadFileContent();

        Closing += EditorWindow_Closing;
        KeyDown += EditorWindow_KeyDown;
    }

    private void LoadFileContent()
    {
        fragmentShaderTextBox.Text = File.Exists(_preferences.LastFilePath)
                ? File.ReadAllText(_preferences.LastFilePath)
                : fragmentShaderTextBox.Text = Shader.DefaultFragmentShader;
        _modified = false;
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

    private void New_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmCanOverwriteTextBox())
            return;

        fragmentShaderTextBox.Text = Shader.DefaultFragmentShader;
        _modified = false;
        _preferences.LastFilePath = string.Empty;
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
        _preferences.LastFilePath = dialog.FileName;
        _preferences.LastDirectory = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
        _modified = false;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveFile();
    }

    private bool SaveFile()
    {
        SaveFileDialog dialog = new SaveFileDialog();
        PrepareFileDialog(dialog);
        if ((dialog?.ShowDialog()) != true)
            return false;

        File.WriteAllText(dialog.FileName, fragmentShaderTextBox.Text);
        _preferences.LastFilePath = dialog.FileName;
        _preferences.LastDirectory = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
        _modified = false;
        return true;
    }

    private void PrepareFileDialog(FileDialog dialog)
    {
        dialog.Filter = "OpenGL Files|*.glsl;*.vert;*.tesc;*.tese;*.geom;*.frag;*.comp|All Files|*.*";
        dialog.DefaultExt = "glsl";
        if (dialog is OpenFileDialog openFileDialog)
        {
            openFileDialog.Multiselect = false;
        }
        if (Directory.Exists(_preferences.LastDirectory))
        {
            dialog.InitialDirectory = _preferences.LastDirectory;
        }
    }

    private void EditorWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_modified)
            return;

        var result = MessageBox.Show("Do you want to save your progress?", "You are about to close the application.", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            e.Cancel = !SaveFile();
            return;
        }

        e.Cancel = result == MessageBoxResult.Cancel;
    }


    private void fragmentShaderTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _modified = true;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _canvas.Dispose();
    }

    public ColorSchemeManager ColorSchemeManager { get; }
    
    private readonly Preferences _preferences;
    
    private readonly RenderCanvas _canvas;

    private bool _modified = false;
}