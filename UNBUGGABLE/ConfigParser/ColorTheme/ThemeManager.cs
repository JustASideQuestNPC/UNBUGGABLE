using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Media;

namespace UNBUGGABLE.Resources;

public static class ThemeManager
{
    private static Dictionary<string, Dictionary<string, Color>> _themeColors = new();
    private static Dictionary<string, Dictionary<string, double>> _themeDoubles = new();

    public static void AddTheme(string themeName, ColorTheme theme)
    {
        Trace.WriteLine($"Adding theme {themeName}");
        Dictionary<string, Color> colors = new();
        Dictionary<string, double> doubles = new();
        
        // i should probably be doing this with reflection but whatever
        
    }

    public static void ApplyTheme(string themeName)
    {
        
    }
}