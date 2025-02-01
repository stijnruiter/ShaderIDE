namespace ShaderIDE.Data;

public class Preferences
{
    public string ColorScheme { get; set; } = ColorSchemeManager.Default.Name;
    public string LastFilePath { get; set; } = string.Empty;
    public string LastDirectory { get; set; } = string.Empty;
}
