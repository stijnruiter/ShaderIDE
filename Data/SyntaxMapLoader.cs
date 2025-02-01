using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShaderIDE.Data;

public enum TokenType
{
    None,
    Comments,
    IntrinsicMethod,
    DataType,
    SpecialVariable,
    Keyword
}

internal class SyntaxMapLoader
{
    public static readonly SyntaxMapLoader OpenGL = Load("ShaderIDE.Resources.glsl_mapping.txt");

    public static SyntaxMapLoader Load(string embeddedName)
        => Load(EmbeddedLoader.GetStream(embeddedName));

    public static SyntaxMapLoader Load(Stream stream)
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

            dictionary[type].AddRange(line.Split(' ')
                                          .Select(token => token.Trim())
                                          .Where(token => !string.IsNullOrWhiteSpace(token)));
        }

        return new SyntaxMapLoader(dictionary);
    }

    private SyntaxMapLoader()
    {
        var dictionary = new Dictionary<TokenType, List<string>>();
        foreach (var tokenType in Enum.GetValues<TokenType>())
        {
            dictionary[tokenType] = [];
        }
        Tokens = dictionary;
    }

    private SyntaxMapLoader(IReadOnlyDictionary<TokenType, List<string>> tokens)
    {
        Tokens = tokens;
    }

    public readonly IReadOnlyDictionary<TokenType, List<string>> Tokens;
}
