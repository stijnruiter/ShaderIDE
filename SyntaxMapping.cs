using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShaderIDE;

enum TokenType
{
    None,
    Comments,
    IntrinsicMethod,
    DataType,
    SpecialVariable,
    Keyword
}

internal class SyntaxMapping
{
    public static readonly SyntaxMapping OpenGL = Load("ShaderIDE.Resources.glsl_mapping.txt");

    public static SyntaxMapping Load(string embeddedName) 
        => Load(EmbeddedLoader.GetStream(embeddedName));

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

            dictionary[type].AddRange(line.Split(' ')
                                          .Select(token => token.Trim())
                                          .Where(token => !string.IsNullOrWhiteSpace(token)));
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
