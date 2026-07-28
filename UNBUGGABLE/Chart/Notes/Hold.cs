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
        RenderFlags(dc, x, startY, new NoteFlags(Flags.C, Flags.F, false, Flags.N));
        
        RenderDebugTime(dc, x, startY);
    }

    public override void RenderPreview(DrawingContext dc)
    {
        if (EndTime < Chart.CurrentTimeRaw || Time > Chart.CurrentTimeRaw + 1000)
        {
            return;
        }
        
        var y = Flags.N ? TimeToNoiszPreviewY(Time) :
            Lane == NoteLane.TOP ? GamePreview.TopLaneY : GamePreview.BottomLaneY;
        
        if (Type == NoteType.HOLD)
        {
            RenderHoldPreview(dc, y);
        }
        else
        {
            RenderDoublePreview(dc, y);
        }
    }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        switch (Type)
        {
            case NoteType.HOLD:
                if (Time > rangeStart && Time <= rangeEnd && Config.Settings.HitSounds.HoldStart)
                {
                    return (long)(Time - rangeStart);
                }
                if (EndTime > rangeStart && EndTime <= rangeEnd && Config.Settings.HitSounds.HoldEnd)
                {
                    return (long)(EndTime - rangeStart);
                }
                break;
            case NoteType.DOUBLE:
                if (Time > rangeStart && Time <= rangeEnd && Config.Settings.HitSounds.DoubleStart)
                {
                    return (long)(Time - rangeStart);
                }
                if (EndTime > rangeStart && EndTime <= rangeEnd && Config.Settings.HitSounds.DoubleEnd)
                {
                    return (long)(EndTime - rangeStart);
                }
                break;
        }
        
        return null;
    }
    
    private void RenderHoldPreview(DrawingContext dc, double y)
    {
        var startX = GamePreview.TimeToScreenCoords(Time < Chart.CurrentTimeRaw ?
                                                        Chart.CurrentTimeRaw : Time);
        
        var endX = GamePreview.TimeToScreenCoords(EndTime);
        
        dc.DrawLine(new Pen(_holdTailBrush, 20), new Point(startX, y), new Point(endX, y));
        dc.DrawEllipse(_holdBrush, new Pen(_outlineBrush, 6), new Point(startX, y), 30, 30);
    }
    
    private void RenderDoublePreview(DrawingContext dc, double startY)
    {
        var startX = GamePreview.TimeToScreenCoords(Time < Chart.CurrentTimeRaw ?
                                                        Chart.CurrentTimeRaw : Time);
        var endY = -startY;
        var noteY = Utils.MapRanges(Math.Clamp(Chart.CurrentTimeRaw, Time, EndTime), Time, EndTime,
                              startY, endY);
        
        var fillBrush = _doubleBrush;
        var outlineBrush = _outlineBrush;
        if (Time < Chart.CurrentTimeRaw)
        {
            fillBrush = new SolidColorBrush(fillBrush.Color)
            {
                Opacity = Config.Settings.DoublePreviewAlpha
            };
            outlineBrush = new SolidColorBrush(outlineBrush.Color)
            {
                Opacity = Config.Settings.DoublePreviewAlpha
            };
        }

        if (Config.Settings.EnhancedPreview)
        {
            var endX = GamePreview.TimeToScreenCoords(EndTime);
            dc.DrawEllipse(_doubleTailBrush, null, new Point(endX, endY), 20, 20);
        }
        dc.DrawEllipse(fillBrush, new Pen(outlineBrush, 6), new Point(startX, noteY), 30, 30);
    }
    
    private double TimeToNoiszPreviewY(double time)
    {
        var rangeStart = (Lane == NoteLane.TOP ? -GamePreview.TopLaneY : GamePreview.BottomLaneY);
        var y = Math.Clamp(
            (Chart.CurrentTimeRaw - time) / 1000 * (GamePreview.PixelsPerSecond / 4.0) +
            rangeStart, 0, rangeStart);
        return (Lane == NoteLane.TOP ? -y : y);
    }

    public override string ToString()
    {
        return $"{(Type == NoteType.HOLD ? "Hold" : "Double")}: Lane={Lane}, " +
               $"Time={Time}-{EndTime}ms";
    }
}