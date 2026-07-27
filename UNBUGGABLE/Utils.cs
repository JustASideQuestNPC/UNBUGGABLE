using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Media;

namespace UNBUGGABLE;

public static class Utils
{
    public static void DrawOutlinedText(this DrawingContext dc, FormattedText text, Point origin,
        IBrush fill, Pen outline)
    {
        var path = text.BuildGeometry(origin);
        if (path == null)
        {
            return;
        }
        
        dc.DrawGeometry(fill, outline, path);
    }
    
    public static void DrawArc(this DrawingContext dc, SolidColorBrush? brush, Pen? pen,
        Point origin, double xRadius, double yRadius, double startAngle, double endAngle)
    {
        startAngle *= Math.PI / 180;
        endAngle *= Math.PI / 180;
        
        var start = new Point(Math.Cos(startAngle) * xRadius,
                              Math.Sin(startAngle) * yRadius);
        var end = new Point(Math.Cos(endAngle) * xRadius, Math.Sin(endAngle) * yRadius);
        
        var geo = new StreamGeometry();
        using (var c = geo.Open())
        {
            c.BeginFigure(start, false);
            c.ArcTo(end, new Size(xRadius * 2, yRadius * 2), 0, endAngle - startAngle > Math.PI,
                    SweepDirection.Clockwise, true);
            c.EndFigure(false);
        }
        geo.Transform = new TranslateTransform(origin.X, origin.Y);
        dc.DrawGeometry(brush, pen, geo);
    }

    /// <summary>
    /// Returns whether this number is within 1 of another number.
    /// </summary>
    public static bool SoftEquals(this double a, double b)
    {
        return Math.Abs(a - b) < 1;
    }
    
    /// <summary>
    /// Returns whether this number is note within 1 of another number.
    /// </summary>
    public static bool SoftNotEquals(this double a, double b)
    {
        return Math.Abs(a - b) > 1;
    }
    
    /// <summary>
    /// Returns whether a point is inside this rectangle.
    /// </summary>
    public static bool ContainsPoint(this Rect rect, Point point) =>
        rect.Left <= point.X && point.X <= rect.Right &&
        rect.Top  <= point.Y && point.Y <= rect.Bottom;
    
    public static double Map(double input, double inputStart, double inputEnd, double outputStart,
        double outputEnd) =>
        outputStart + ((outputEnd - outputStart) / (inputEnd - inputStart)) * (input - inputStart);

    public static string GetReadableKeybindString(string keybind)
    {
        var split = keybind.Split('+').ToList();
        var primaryKey = split[^1] switch
        {
            "d0" => "0",
            "d1" => "1",
            "d2" => "2",
            "d3" => "3",
            "d4" => "4",
            "d5" => "5",
            "d6" => "6",
            "d7" => "7",
            "d8" => "8",
            "d9" => "9",
            "oem3" => "`",
            "oemMinus" => "-",
            "oemPlus" => "+",
            "oem4" => "[",
            "oemCloseBrackets" => "]",
            "oemPipe" => "\\",
            "return" => "enter",
            "oemSemicolon" => ";",
            "oemQuotes" => "'",
            "oemComma" => ",",
            "oemPeriod" => ".",
            "oemQuestion" => "/",
            _ => split[^1]
        };
        
        return split.Count > 1 ? string.Join("+", split[..^1]) + "+" + primaryKey : primaryKey;
    }
}