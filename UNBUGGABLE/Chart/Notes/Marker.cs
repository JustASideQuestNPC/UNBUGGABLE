using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class MarkerDummyNote : NoteBase
{
    private static List<SolidColorBrush> Brushes = [];
    
    public override NoteType Type => NoteType.MARKER_DUMMY;
    public override NoteLane Lane => NoteLane.MARKER;

    public int ColorId;
    
    private readonly Geometry _shape = new PolylineGeometry([
        new Point(-12, -10),
        new Point(  0,   0),
        new Point(-12,  10)
    ], true);

    public static void UpdateStyles()
    {
        Brushes.Clear();
        for (var i = 0; i < 3; ++i)
        {
            Brushes.Add((SolidColorBrush)App.Current.Resources[$"NoteViewer.Markers.Color{i + 1}"]);
        }
    }

    public MarkerDummyNote(long time, int colorId)
    {
        Time = time;
        ColorId = colorId;
    }
    
    // markers can never be selected
    public override void Render(DrawingContext dc, bool _)
    {
        var y = NoteViewer.TimeToScreenCoords(Time);
        if (y < -50 || y > NoteViewer.ViewerHeight + 50)
        {
            return;
        }
        
        _shape.Transform = new TransformGroup { Children =
        {
            new TranslateTransform(0, y),
            new ScaleTransform((double)App.Current.Resources["NoteViewer.Markers.ArrowScale"],
                               (double)App.Current.Resources["NoteViewer.Markers.ArrowScale"])
        } };
        dc.DrawGeometry(Brushes[ColorId], null, _shape);
    }
    
    public override void RenderPreview(DrawingContext dc) { }
    
    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time < rangeStart || Time > rangeEnd)
        {
            return null;
        }
        
        var offset = (long)(Time - rangeStart);
        if ((ColorId == 0 && Config.Settings.HitSounds.Marker1) ||
            (ColorId == 1 && Config.Settings.HitSounds.Marker2) ||
            (ColorId == 2 && Config.Settings.HitSounds.Marker3))
        {
            return offset;
        }
        
        return null;
    }
    
    public override string ToHitObjectString(bool isFirstNote, bool isStandardFile)
    {
        if (Config.Settings.SaveMarkersInLane2 || isStandardFile)
        {
            return $"128,192,{Time + Chart.Metadata.ChartOffset}," +
                   $"{(isFirstNote ? 1 : 5)},{GetFlagString()},0:0:0:0:";
        }

        return "";
    }

    public override string ToString() => $"Marker: Type={ColorId} Time={Time}ms";
}