using System.IO;
using System.Reflection;

namespace ShaderIDE;

public static class EmbeddedLoader
{
    public static Stream GetStream(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(path);
        if (stream is null)
            throw new FileNotFoundException($"Embedded resource {path} not found. Possible names are {string.Join(", \r\n", assembly.GetManifestResourceNames())}", path);
        
        return stream;
    }

    public static string GetContent(string path)
    {
        using var stream = GetStream(path);
        using var sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }
}
