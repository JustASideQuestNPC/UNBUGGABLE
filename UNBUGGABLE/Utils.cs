using System;
using System.Collections.Generic;
using System.IO;
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

    /// <summary>
    /// Returns whether a point is inside a polygon.
    /// </summary>
    public static bool PointInPolygon(List<Point> vertices, Point offset, Point point)
    {
        // Raycasting algorithm based on https://wrfranklin.org/Research/Short_Notes/pnpoly.html
        // This looks complicated but it's relatively simple:
        // 1. Draw a line out from the test point (the direction doesn't matter, but I use a
        //    horizontal one here because the math is easier).
        // 2. Check whether that line intersects each edge of the polygon.
        // 3. If the line intersects an odd number of edges, the point is inside the polygon.
        var inside = false;
        for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
        {
            Point v1 = vertices[i], v2 = vertices[j];
            double x1 = v1.X + offset.X, y1 = v1.Y + offset.Y, x2 = v2.X + offset.X,
                   y2 = v2.Y + offset.Y;
            if (((y1 > point.Y) != (y2 > point.Y)) &&
                point.X < (x2 - x1) * (point.Y - y1) / (y2 - y1) + x1)
            {
                inside = !inside;
            }
        }
        return inside;
    }
    
    public static double Map(double input, double inputStart, double inputEnd, double outputStart,
        double outputEnd) =>
        outputStart + ((outputEnd - outputStart) / (inputEnd - inputStart)) * (input - inputStart);
}