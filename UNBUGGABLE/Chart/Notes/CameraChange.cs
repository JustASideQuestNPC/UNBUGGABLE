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

    private static SolidColorBrush FillBrush;
    private static SolidColorBrush OutlineBrush;
    private static double OutlineThickness;
    private static SolidColorBrush SelectedOutlineBrush;
    private static SolidColorBrush SelectedFillBrush;
    private static double SelectedOutlineThickness;
    
    public override NoteType Type =>
        (Flags.C ? NoteType.CAMERA_INSTANT : Flags.W ? NoteType.CAMERA_WIDE :
            NoteType.CAMERA_SWAP);
    
    public override NoteLane Lane => NoteLane.CAMERA;
    
    private readonly Geometry _shape = new PolylineGeometry(Vertices, true);

    public static void UpdateStyles()
    {
        FillBrush = (SolidColorBrush)App.Current.Resources["Notes.Camera.FillColor"];
        OutlineBrush = (SolidColorBrush)App.Current.Resources["Notes.Camera.OutlineColor"];
        // thickness is always the same on all sides
        OutlineThickness = ((Thickness)App.Current.Resources["Notes.Camera.OutlineThickness"]).Top;
        SelectedFillBrush =
            (SolidColorBrush)App.Current.Resources["Notes.Camera.Selected.FillColor"];
        SelectedOutlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.Camera.Selected.OutlineColor"];
        SelectedOutlineThickness =
            ((Thickness)App.Current.Resources["Notes.Camera.Selected.OutlineThickness"]).Top;
    }

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
        
        var pen = selected ?
            new Pen(SelectedOutlineBrush, SelectedOutlineThickness) :
            new Pen(OutlineBrush, OutlineThickness);
        dc.DrawGeometry(selected ? SelectedFillBrush : FillBrush, pen, shape);
        
        RenderFlags(dc, x, y);
        
        RenderDebugTime(dc, x, y);
    }

    public override void RenderPreview(DrawingContext dc) { }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time > rangeStart && Time <= rangeEnd && Config.Settings.HitSounds.CameraChange)
        {
            return (long)(Time - rangeStart);
        }

        return null;
    }
    public override string ToString() => $"Camera Change: Type={Type} Time={Time}ms";
}