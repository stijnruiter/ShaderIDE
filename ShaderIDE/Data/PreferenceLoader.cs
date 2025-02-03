using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ShaderIDE.Data;

public static class PreferenceLoader
{
    public static Preferences Load()
    {
        if (!File.Exists(FullPath))
        {
            var preferences = new Preferences();
            Save(preferences);
            return preferences;
        }

        return JsonSerializer.Deserialize<Preferences>(File.ReadAllText(FullPath)) ?? throw new Exception("Unable to parse preferences");
    }

    public static void Save(Preferences preferences)
    {
        File.WriteAllText(FullPath, JsonSerializer.Serialize(preferences));
    }

    public const string PreferenceFileName = "preferences.conf";

    public static readonly string FullPath = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PreferenceFileName);
}
