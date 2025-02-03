using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ShaderIDE.Data;

public class ColorScheme
{
    public ColorScheme() : this("Default")
    {
        Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
    }

    public ColorScheme(string name)
    {
        Name = name;
        _foregroundColors = new Dictionary<TokenType, SolidColorBrush>();
        foreach (var type in Enum.GetValues<TokenType>())
        {
            _foregroundColors[type] = new SolidColorBrush(Colors.Black);
        }
        Background = new SolidColorBrush(Colors.White);
    }

    public SolidColorBrush GetColor(TokenType type) => _foregroundColors[type];

    public string Name { get; set; } = string.Empty;

    public SolidColorBrush Background { get; set; } = new SolidColorBrush(Colors.White);

    public SolidColorBrush Foreground
    {
        get => _foregroundColors[TokenType.None];
        set => _foregroundColors[TokenType.None] = value;
    }

    public SolidColorBrush Comments
    {
        get => _foregroundColors[TokenType.Comments];
        set => _foregroundColors[TokenType.Comments] = value;
    }

    public SolidColorBrush Keyword
    {
        get => _foregroundColors[TokenType.Keyword];
        set => _foregroundColors[TokenType.Keyword] = value;
    }

    public SolidColorBrush DataType
    {
        get => _foregroundColors[TokenType.DataType];
        set => _foregroundColors[TokenType.DataType] = value;
    }

    public SolidColorBrush SpecialVariable
    {
        get => _foregroundColors[TokenType.SpecialVariable];
        set => _foregroundColors[TokenType.SpecialVariable] = value;
    }

    public SolidColorBrush IntrinsicMethod
    {
        get => _foregroundColors[TokenType.IntrinsicMethod];
        set => _foregroundColors[TokenType.IntrinsicMethod] = value;
    }

    private readonly Dictionary<TokenType, SolidColorBrush> _foregroundColors;
}

