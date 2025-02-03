using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ShaderIDE.Data;

public class ColorSchemeManager : DependencyObject
{
    public ColorSchemeManager(Preferences preferences)
    {
        Preferences = preferences;
        SetScheme();
    }

    public void SetScheme()
    {
        var scheme = ColorSchemes.SingleOrDefault(c => c.Name.Equals(Preferences.ColorScheme, StringComparison.InvariantCultureIgnoreCase));
        if (scheme != null)
        {
            Current = scheme;
        }
    }

    public ObservableCollection<ColorScheme> ColorSchemes { get; set; } = [Default, Dark, Adas, Cosmo];

    public static readonly DependencyProperty CurrentColorSchemeProperty =
        DependencyProperty.Register(nameof(Current), typeof(ColorScheme),
        typeof(ColorSchemeManager), new UIPropertyMetadata(Dark, new PropertyChangedCallback(OnCurrentColorSchemChanged)));


    public ColorScheme Current
    {
        get => (ColorScheme)GetValue(CurrentColorSchemeProperty);
        set => SetValue(CurrentColorSchemeProperty, value);
    }

    public readonly static ColorScheme Cosmo = new("Cosmo")
    {
        //https://www.color-hex.com/color-palette/1055152
        Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
        DataType = new SolidColorBrush(Color.FromRgb(153, 84, 73)),
        Keyword = new SolidColorBrush(Color.FromRgb(136, 254, 214)),
        SpecialVariable = new SolidColorBrush(Color.FromRgb(196, 255, 133)),
        IntrinsicMethod = new SolidColorBrush(Color.FromRgb(229, 130, 252)),
        Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
        Comments = new SolidColorBrush(Color.FromRgb(174, 175, 182))
    };

    public readonly static ColorScheme Adas = new("Adas")
    {
        //https://www.color-hex.com/color-palette/1055154

        Keyword = new SolidColorBrush(Color.FromRgb(212, 161, 44)),
        DataType = new SolidColorBrush(Color.FromRgb(158, 143, 87)),
        Background = new SolidColorBrush(Color.FromRgb(11, 19, 43)),
        Comments = new SolidColorBrush(Color.FromRgb(88, 80, 107)),
        IntrinsicMethod = new SolidColorBrush(Color.FromRgb(166, 124, 82)),
        SpecialVariable = new SolidColorBrush(Color.FromRgb(255, 130, 93)),
        Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
    };

    public readonly static ColorScheme Dark = new("Dark")
    {
        // https://www.color-hex.com/color-palette/1051590
        Comments = new SolidColorBrush(Color.FromRgb(123, 140, 171)),
        DataType = new SolidColorBrush(Color.FromRgb(171, 123, 140)),
        Keyword = new SolidColorBrush(Color.FromRgb(123, 171, 154)),
        SpecialVariable = new SolidColorBrush(Color.FromRgb(171, 123, 164)),
        IntrinsicMethod = new SolidColorBrush(Color.FromRgb(209, 182, 142)),
        Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
        Background = new SolidColorBrush(Color.FromRgb(30, 30, 30))
    };

    public readonly static ColorScheme Default = new();

    public readonly Preferences Preferences;

    private static void OnCurrentColorSchemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var manager = (ColorSchemeManager)d;
        manager.Preferences.ColorScheme = manager.Current.Name;
    }
}

