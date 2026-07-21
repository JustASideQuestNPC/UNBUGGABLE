using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class MarkerDummyNote : NoteBase
{
    public override NoteType Type => NoteType.MARKER_DUMMY;
    public override NoteLane Lane => NoteLane.MARKER;
    
    private int _colorId;
    public int ColorId
    {
        get => _colorId;
        set
        {
            _colorId = value;
            _fillBrush = (SolidColorBrush)App.Current.Resources[$"Marker{value + 1}"];
        }
    }
    
    private readonly Geometry _shape = new PolylineGeometry([
        new Point(-12, -10),
        new Point(  0,   0),
        new Point(-12,  10)
    ], true);
    
    private SolidColorBrush _fillBrush;

    public MarkerDummyNote(double time, int colorId)
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
        
        _shape.Transform = new TranslateTransform(147, y);
        dc.DrawGeometry(_fillBrush, null, _shape);
    }
    
    public override void RenderPreview(DrawingContext dc) { }
    
    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time > rangeStart && Time <= rangeEnd && Config.HitSounds.Marker)
        {
            return (long)(Time - rangeStart);
        }
        
        return null;
    }
    
    public override string ToHitObjectString(bool isFirstNote, bool isStandardFile)
    {
        if (Config.SaveMarkersInLane2 || isStandardFile)
        {
            return $"128,192,{Time},{(isFirstNote ? 1 : 5)},{GetFlagString()},0:0:0:0:";
        }

        return "";
    }
}