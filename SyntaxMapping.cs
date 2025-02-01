using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ShaderIDE;

enum TokenType
{
    None,
    IntrinsicMethod,
    DataType,
    SpecialVariable,
    Keyword
}

internal class SyntaxMapping
{
    public static readonly SyntaxMapping OpenGL = Load("ShaderIDE.glsl_mapping.txt");

    public static SyntaxMapping Load(string embeddedName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(embeddedName);
        if (stream is null)
        {
            Debug.Fail($"EmbeddedResource '{embeddedName}' not found.");
            return new SyntaxMapping();
        }
        return Load(stream);
    }

    public static SyntaxMapping Load(Stream stream)
    {
        TokenType type = TokenType.None;
        using var sr = new StreamReader(stream);

        var dictionary = new Dictionary<TokenType, List<string>>();
        foreach (var tokenType in Enum.GetValues<TokenType>())
        {
            dictionary[tokenType] = [];
        }

        while (sr.ReadLine() is { } line)
        {
            if (line.Length <= 1)
                continue;

            if (line.StartsWith("#"))
            {
                if (Enum.TryParse<TokenType>(line[1..].Trim(), out var nextType))
                {
                    type = nextType;
                    continue;
                }
                continue;
            }

            dictionary[type].AddRange(line.Split(' ').Select(token => token.Trim()));
        }

        return new SyntaxMapping(dictionary);
    }

    private SyntaxMapping()
    {
        var dictionary = new Dictionary<TokenType, List<string>>();
        foreach (var tokenType in Enum.GetValues<TokenType>())
        {
            dictionary[tokenType] = [];
        }
        Tokens = dictionary;
    }

    private SyntaxMapping(IReadOnlyDictionary<TokenType, List<string>> tokens)
    {
        Tokens = tokens;
    }

    public readonly IReadOnlyDictionary<TokenType, List<string>> Tokens;
}
