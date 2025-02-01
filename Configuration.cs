using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ShaderIDE;

public class Configuration
{
    public const string FileName = "preferences.conf";
    private static string FullPath => Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FileName);
    
    public Configuration() { }

    public static Configuration Load()
    {
        if (!File.Exists(FullPath))
        {
            var config = new Configuration();
            config.Save();
            return config;
        }

        return JsonSerializer.Deserialize<Configuration>(File.ReadAllText(FullPath)) ?? throw new Exception("Unable to parse config");
    }

    public void Save()
    {
        File.WriteAllText(FullPath, JsonSerializer.Serialize(this));
    }

    public string ColorScheme { get; set; } = ColorSchemeManager.Default.Name;
}
