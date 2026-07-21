using System;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class HoldNote : NoteBase
{
    public override NoteType Type => (Flags.W ? NoteType.DOUBLE : NoteType.HOLD);
    
    private readonly SolidColorBrush _holdBrush =
        App.Current.Resources["SingleNote"] as SolidColorBrush;
    private readonly SolidColorBrush _doubleBrush =
        App.Current.Resources["DoubleNote"] as SolidColorBrush;

    private readonly SolidColorBrush _holdTailBrush;
    private readonly SolidColorBrush _doubleTailBrush;

    public HoldNote(NoteFlags? startingFlags = null) : base(startingFlags)
    {
        _holdTailBrush = new SolidColorBrush(_holdBrush.Color)
        {
            Opacity = 0.6
        };
        _doubleTailBrush = new SolidColorBrush(_doubleBrush.Color)
        {
            Opacity = 0.6
        };
    }

    public override void Render(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var startY = NoteViewer.TimeToScreenCoords(Time);
        var endY = NoteViewer.TimeToScreenCoords(EndTime);

        if ((startY < -50 && endY < -50) ||
            (startY > NoteViewer.ViewerHeight + 50 && endY > NoteViewer.ViewerHeight + 50))
        {
            return;
        }
        
        dc.DrawRectangle(Type == NoteType.HOLD ? _holdTailBrush : _doubleTailBrush, null,
                         new Rect(x - 16, startY, 32, endY - startY));
        dc.DrawRectangle(Type == NoteType.HOLD ? _holdBrush : _doubleBrush,
                         new Pen(_outlineBrush, 4),  new Rect(x - 40, startY - 12, 80, 24));
        
        if (selected)
        {
            dc.DrawRectangle(null, new Pen(_selectedBrush, 4),
                             new Rect(x - 40, startY - 12, 80, 24));
        }
        
        // overriding the flags hides the letter for a double note and does nothing to a hold note
        RenderFlags(dc, x, startY, new NoteFlags(Flags.C, Flags.F, false));
    }

    public override void RenderPreview(DrawingContext dc)
    {
        if (EndTime < Chart.CurrentTime || Time > Chart.CurrentTime + 1000)
        {
            return;
        }
        
        if (Type == NoteType.HOLD)
        {
            RenderHoldPreview(dc);
        }
        else
        {
            RenderDoublePreview(dc);
        }
    }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        switch (Type)
        {
            case NoteType.HOLD:
                if (Time > rangeStart && Time <= rangeEnd && Config.HitSounds.HoldStart)
                {
                    return (long)(Time - rangeStart);
                }
                if (EndTime > rangeStart && EndTime <= rangeEnd && Config.HitSounds.HoldEnd)
                {
                    return (long)(EndTime - rangeStart);
                }
                break;
            case NoteType.DOUBLE:
                if (Time > rangeStart && Time <= rangeEnd && Config.HitSounds.DoubleStart)
                {
                    return (long)(Time - rangeStart);
                }
                if (EndTime > rangeStart && EndTime <= rangeEnd && Config.HitSounds.DoubleEnd)
                {
                    return (long)(EndTime - rangeStart);
                }
                break;
        }
        
        return null;
    }
    
    private void RenderHoldPreview(DrawingContext dc)
    {
        var startX = GamePreview.TimeToScreenCoords(Time < Chart.CurrentTime ?
                                                        Chart.CurrentTime : Time);
        
        var endX = GamePreview.TimeToScreenCoords(EndTime);
        var y = Lane == NoteLane.TOP ? GamePreview.TopLaneY : GamePreview.BottomLaneY;
        
        dc.DrawLine(new Pen(_holdTailBrush, 20), new Point(startX, y), new Point(endX, y));
        dc.DrawEllipse(_holdBrush, new Pen(_outlineBrush, 6), new Point(startX, y), 30, 30);
    }
    
    private void RenderDoublePreview(DrawingContext dc)
    {
        var startX = GamePreview.TimeToScreenCoords(Time < Chart.CurrentTime ?
                                                        Chart.CurrentTime : Time);
        var startY = Lane == NoteLane.TOP ? GamePreview.TopLaneY : GamePreview.BottomLaneY;
        var endY = Lane == NoteLane.TOP ? GamePreview.BottomLaneY : GamePreview.TopLaneY;
        var noteY = Utils.Map(Math.Clamp(Chart.CurrentTime, Time, EndTime), Time, EndTime,
                              startY, endY);

        if (Config.EnhancedPreview)
        {
            var endX = GamePreview.TimeToScreenCoords(EndTime);
            dc.DrawEllipse(_doubleTailBrush, null, new Point(endX, endY), 20, 20);
        }
        dc.DrawEllipse(_doubleBrush, new Pen(_outlineBrush, 6), new Point(startX, noteY), 30, 30);
    }
}