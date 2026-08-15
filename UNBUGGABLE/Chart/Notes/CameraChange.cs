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

    private static SolidColorBrush _fillBrush;
    private static SolidColorBrush _outlineBrush;
    private static double _outlineThickness;
    private static SolidColorBrush _selectedOutlineBrush;
    private static SolidColorBrush _selectedFillBrush;
    private static double _selectedOutlineThickness;
    
    public override NoteType Type =>
        (Flags.C && Flags.W ? NoteType.CAMERA_SWAP_AND_ZOOM :
            Flags.C ? NoteType.CAMERA_INSTANT : Flags.W ? NoteType.CAMERA_ZOOM :
            NoteType.CAMERA_SWAP);
    
    public override NoteLane Lane => NoteLane.CAMERA;
    
    private readonly Geometry _shape = new PolylineGeometry(Vertices, true);

    public static void UpdateStyles()
    {
        _fillBrush = (SolidColorBrush)App.Current.Resources["Notes.Camera.FillColor"];
        _outlineBrush = (SolidColorBrush)App.Current.Resources["Notes.Camera.OutlineColor"];
        // thickness is always the same on all sides
        _outlineThickness = ((Thickness)App.Current.Resources["Notes.Camera.OutlineThickness"]).Top;
        _selectedFillBrush =
            (SolidColorBrush)App.Current.Resources["Notes.Camera.Selected.FillColor"];
        _selectedOutlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.Camera.Selected.OutlineColor"];
        _selectedOutlineThickness =
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
            new Pen(_selectedOutlineBrush, _selectedOutlineThickness) :
            new Pen(_outlineBrush, _outlineThickness);
        dc.DrawGeometry(selected ? _selectedFillBrush : _fillBrush, pen, shape);
        
        RenderFlags(dc, x, y);
        
        RenderDebugTime(dc, x, y);
    }

    public override string ToHitObjectString(bool isFirstNote, bool isStandardFile)
    {
        // swap and zoom is actually two camera notes that are stacked on top of each other
        if (Type == NoteType.CAMERA_SWAP_AND_ZOOM)
        {
            var swapNote = new CameraChange
            {
                Time = Time,
                // the swap note doesn't have the clap flag because i'm using that flag as an
                // indicator in the UI
                Flags = new NoteFlags(false, Flags.F, false)
            };
            var zoomNote = new CameraChange
            {
                Time = Time + 1, // offset by 1ms otherwise the game doesn't like it
                Flags = new NoteFlags(false, Flags.F, true)
            };
            return $"{swapNote.ToHitObjectString(isFirstNote, isStandardFile)}\n" +
                   $"{zoomNote.ToHitObjectString(false, isStandardFile)}";
        }
        
        return base.ToHitObjectString(isFirstNote, isStandardFile);
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