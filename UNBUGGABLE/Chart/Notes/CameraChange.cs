using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class CameraChange : NoteBase
{
    private static readonly List<Point> Vertices =
    [
        new(-33.412, -10.920),
        new(-20.500, -5.973),
        new(-20.500, -14.000),
        new(20.500, -14.000),
        new(20.500, 14.000),
        new(-20.500, 14.000),
        new(-20.500, 5.973),
        new(-33.412, 10.920)
    ];
    
    public override NoteType Type =>
        (Flags.C ? NoteType.CAMERA_INSTANT : Flags.W ? NoteType.CAMERA_WIDE :
            NoteType.CAMERA_SWAP);
    
    public override NoteLane Lane => NoteLane.CAMERA;
    
    private readonly SolidColorBrush _fillBrush =
        App.Current.Resources["CameraChange"] as SolidColorBrush;
    
    private readonly Geometry _shape = new PolylineGeometry(Vertices, true);

    public override void Render(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);

        if (y < -50 || y > NoteViewer.ViewerHeight + 50)
        {
            return;
        }

        var shape = _shape.Clone();
        shape.Transform = new TranslateTransform(x, y);
        
        dc.DrawGeometry(_fillBrush, new Pen(_outlineBrush, 4), shape);
        if (selected)
        {
            dc.DrawGeometry(null, new Pen(_selectedBrush, 4), shape);
        }
        RenderFlags(dc, x, y);
    }

    public override void RenderPreview(DrawingContext dc) { }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time > rangeStart && Time <= rangeEnd && Config.HitSounds.CameraChange)
        {
            return (long)(Time - rangeStart);
        }

        return null;
    }

    public override bool MouseOver()
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);
        return Utils.PointInPolygon(Vertices, new Point(x, y), ChartBuilder.MousePosition);
    }
}