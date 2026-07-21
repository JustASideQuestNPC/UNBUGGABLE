using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class SingleNote : NoteBase
{
    private static readonly List<Point> SpikeVertices =
    [
        new(-30, -16),
        new( 30,   0),
        new(-30,  16)
    ];
    
    private static readonly List<Point> SpikePreviewVertices =
    [
        new(-35, 30),
        new( 0, -30),
        new( 35, 30)
    ];
    
    public override NoteType Type => (Flags.W ? NoteType.SPIKE : NoteType.SINGLE);
    
    private readonly Geometry _spikeShape = new PolylineGeometry(SpikeVertices, true);
    private readonly Geometry _spikePreviewShape = new PolylineGeometry(SpikePreviewVertices, true);
    
    private readonly SolidColorBrush _singleBrush =
        App.Current.Resources["SingleNote"] as SolidColorBrush;
    private readonly SolidColorBrush _spikeBrush =
        App.Current.Resources["Spike"] as SolidColorBrush;

    public override void Render(DrawingContext dc, bool selected)
    {
        if (Type == NoteType.SINGLE)
        {
            RenderSingle(dc, selected);
        }
        else
        {
            RenderSpike(dc, selected);
        }
    }

    public override void RenderPreview(DrawingContext dc)
    {
        if (Time < Chart.CurrentTime || Time > Chart.CurrentTime + 1000)
        {
            return;
        }
        
        if (Type == NoteType.SINGLE)
        {
            RenderSinglePreview(dc);
        }
        else
        {
            RenderSpikePreview(dc);
        }
    }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        var shouldPlay = Type switch
        {
            NoteType.SINGLE => Time > rangeStart && Time <= rangeEnd && Config.HitSounds.Single,
            NoteType.SPIKE => Time > rangeStart && Time <= rangeEnd && Config.HitSounds.Spike,
            _ => false
        };

        if (shouldPlay)
        {
            return (long)(Time - rangeStart);
        }

        return null;
    }

    public override bool MouseOver()
    {
        if (Type == NoteType.SINGLE)
        {
            return base.MouseOver();
        }
        
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);
        return Utils.PointInPolygon(SpikeVertices, new Point(x, y), ChartBuilder.MousePosition);
    }
    
    private void RenderSingle(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);

        if (y < -50 || y > NoteViewer.ViewerHeight + 50)
        {
            return;
        }
        
        dc.DrawRectangle(_singleBrush, new Pen(_outlineBrush, 4), new Rect(x - 40, y - 12, 80, 24));
        if (selected)
        {
            dc.DrawRectangle(null, new Pen(_selectedBrush, 4), new Rect(x - 40, y - 12, 80, 24));
        }
        
        RenderFlags(dc, x, y);
    }

    private void RenderSinglePreview(DrawingContext dc)
    {
        dc.DrawEllipse(_singleBrush, new Pen(_outlineBrush, 6),
                       new Point(GamePreview.TimeToScreenCoords(Time),
                                 Lane == NoteLane.TOP ? GamePreview.TopLaneY :
                                     GamePreview.BottomLaneY),
                       30, 30);
    }
    
    private void RenderSpike(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);

        if (y < -50 || y > NoteViewer.ViewerHeight + 50)
        {
            return;
        }
        
        var shape = _spikeShape.Clone();
        var offset = (Lane == NoteLane.TOP) ? 7 : -7;
        var transform = new TransformGroup();
        if (Lane == NoteLane.BOTTOM)
        {
            transform.Children.Add(new RotateTransform(180));
        }
        transform.Children.Add(new TranslateTransform(x + offset, y));
        shape.Transform = transform;
        
        dc.DrawGeometry(_spikeBrush, new Pen(_outlineBrush, 4), shape);
        if (selected)
        {
            dc.DrawGeometry(null, new Pen(_selectedBrush, 4), shape);
        }
        
        RenderFlags(dc, x, y, new NoteFlags(Flags.C, Flags.F, false));
    }

    private void RenderSpikePreview(DrawingContext dc)
    {
        var y = Lane == NoteLane.TOP ? GamePreview.TopLaneY : GamePreview.BottomLaneY;
        var shape = _spikePreviewShape.Clone();
        var offset = (Lane == NoteLane.TOP) ? 7 : -7;
        var transform = new TransformGroup();
        if (Lane == NoteLane.TOP)
        {
            transform.Children.Add(new RotateTransform(180));
        }
        transform.Children.Add(new TranslateTransform(GamePreview.TimeToScreenCoords(Time),
                                                      y + offset));
        shape.Transform = transform;
        
        dc.DrawGeometry(_spikeBrush, new Pen(_outlineBrush, 6), shape);
    }
}