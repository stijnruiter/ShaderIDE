using System;
using System.Windows;

namespace ShaderIDE;

internal class Program
{
    [STAThread]
    public static void Main()
    {
        var app = new Application();
        app.MainWindow = new EditorWindow();
        app.MainWindow.Show();
        app.Run();
    }
}
