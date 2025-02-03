using ShaderIDE.Data;
using System;
using System.Windows;

namespace ShaderIDE;

internal class Program
{

    [STAThread]
    public static void Main()
    {
        var preferences = PreferenceLoader.Load();
        var colorSchemeManager = new ColorSchemeManager(preferences);
        try
        {
            var app = new Application();
            app.MainWindow = new EditorWindow(preferences, colorSchemeManager);
            app.MainWindow.Show();
            app.Run();
        }
        finally
        {
            PreferenceLoader.Save(preferences);
        }
    }

}
