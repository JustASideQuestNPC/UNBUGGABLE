using System;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class HoldNote : NoteBase
{
    private class StyleGroup
    {
        public required SolidColorBrush FillBrush;
        public required SolidColorBrush OutlineBrush;
        public required double OutlineThickness;
        public required SolidColorBrush TailFillBrush;
        public required SolidColorBrush TailOutlineBrush;
        public required double TailOutlineThickness;
        public required SolidColorBrush SelectedFillBrush;
        public required SolidColorBrush SelectedOutlineBrush;
        public required double SelectedOutlineThickness;
        public required SolidColorBrush SelectedTailFillBrush;
        public required SolidColorBrush SelectedTailOutlineBrush;
        public required double SelectedTailOutlineThickness;
    }

    private static StyleGroup _holdStyles;
    private static StyleGroup _doubleStyles;
    
    public override NoteType Type => (Flags.W ? NoteType.DOUBLE : NoteType.HOLD);

    public static void UpdateStyles()
    {
        _holdStyles = new StyleGroup
        {
            FillBrush = (SolidColorBrush)App.Current.Resources["Notes.Hold.FillColor"],
            OutlineBrush = (SolidColorBrush)App.Current.Resources["Notes.Hold.OutlineColor"],
            OutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Hold.OutlineThickness"]).Top,
            TailFillBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Hold.TailColor"],
            TailOutlineBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Hold.TailOutlineColor"],
            TailOutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Hold.TailOutlineThickness"]).Top,

            SelectedFillBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Hold.Selected.FillColor"],
            SelectedOutlineBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Hold.Selected.OutlineColor"],
            SelectedOutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Hold.Selected.OutlineThickness"]).Top,
            SelectedTailFillBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Hold.Selected.TailColor"],
            SelectedTailOutlineBrush =
                (SolidColorBrush)App.Current.Resources[
                    "Notes.Hold.Selected.TailOutlineColor"],
            SelectedTailOutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Hold.Selected.TailOutlineThickness"]).Top,
        };
        _doubleStyles = new StyleGroup
        {
            FillBrush = (SolidColorBrush)App.Current.Resources["Notes.Double.FillColor"],
            OutlineBrush = (SolidColorBrush)App.Current.Resources["Notes.Double.OutlineColor"],
            OutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Double.OutlineThickness"]).Top,
            TailFillBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Double.TailColor"],
            TailOutlineBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Double.TailOutlineColor"],
            TailOutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Double.TailOutlineThickness"]).Top,

            SelectedFillBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Double.Selected.FillColor"],
            SelectedOutlineBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Double.Selected.OutlineColor"],
            SelectedOutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Double.Selected.OutlineThickness"]).Top,
            SelectedTailFillBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Double.Selected.TailColor"],
            SelectedTailOutlineBrush =
                (SolidColorBrush)App.Current.Resources[
                    "Notes.Double.Selected.TailOutlineColor"],
            SelectedTailOutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Double.Selected.TailOutlineThickness"]).Top
        };
    }

    public override bool MouseOverTail()
    {
        if (base.MouseOverTail())
        {
            return true;
        }

        if (Type == NoteType.DOUBLE && Config.Settings.EnhancedPreview)
        {
            return new Rect(
                NoteViewer.GetNoteX(Lane == NoteLane.TOP ? NoteLane.BOTTOM : NoteLane.TOP) - 40,
                NoteViewer.TimeToScreenCoords(EndTime) - 12, 80,
                24).ContainsPoint(ChartBuilder.MousePosition);
        }

        return false;
    }

    public override void Render(DrawingContext dc, bool selected)
    {
        var styles = Type == NoteType.HOLD ? _holdStyles : _doubleStyles;
        
        var x = NoteViewer.GetNoteX(Lane);
        var startY = NoteViewer.TimeToScreenCoords(Time);
        var endY = NoteViewer.TimeToScreenCoords(EndTime);

        if ((startY < -50 && endY < -50) ||
            (startY > NoteViewer.ViewerHeight + 50 && endY > NoteViewer.ViewerHeight + 50))
        {
            return;
        }
        
        var tailPen = selected ?
            new Pen(styles.SelectedTailOutlineBrush, styles.SelectedTailOutlineThickness) : 
            new Pen(styles.TailOutlineBrush, styles.TailOutlineThickness);
        dc.DrawRectangle(selected ? styles.SelectedTailFillBrush : styles.TailFillBrush, tailPen,
                         new Rect(x - 16, startY, 32, endY - startY));

        // also show where doubles will land
        if (Type == NoteType.DOUBLE && Config.Settings.EnhancedPreview)
        {
            var endX = NoteViewer.GetNoteX(Lane == NoteLane.TOP ? NoteLane.BOTTOM : NoteLane.TOP);
            dc.DrawRectangle(styles.TailFillBrush, tailPen, new Rect(endX - 40, endY - 12, 80, 24));
        }
        
        var pen = selected ?
            new Pen(styles.SelectedOutlineBrush, styles.SelectedOutlineThickness) : 
            new Pen(styles.OutlineBrush, styles.OutlineThickness);
        dc.DrawRectangle(selected ? styles.SelectedFillBrush : styles.FillBrush, pen,
                         new Rect(x - 40, startY - 12, 80, 24));
        
        // overriding the flags hides the letter for a double note and does nothing to a hold note
        RenderFlags(dc, x, startY, new NoteFlags(Flags.C, Flags.F, false, Flags.N));
        
        RenderDebugTime(dc, x, startY, endY);
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
                if (EndTime > rangeStart && EndTime <= rangeEnd &&
                    Config.Settings.HitSounds.HoldEnd)
                {
                    return (long)(EndTime - rangeStart);
                }
                break;
            case NoteType.DOUBLE:
                if (Time > rangeStart && Time <= rangeEnd && Config.Settings.HitSounds.DoubleStart)
                {
                    return (long)(Time - rangeStart);
                }
                if (EndTime > rangeStart && EndTime <= rangeEnd &&
                    Config.Settings.HitSounds.DoubleEnd)
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
        
        dc.DrawLine(new Pen(_holdStyles.TailFillBrush, 20), new Point(startX, y),
                    new Point(endX, y));
        dc.DrawEllipse(_holdStyles.FillBrush, new Pen(_holdStyles.OutlineBrush, 6),
                       new Point(startX, y), 30, 30);
    }
    
    private void RenderDoublePreview(DrawingContext dc, double startY)
    {
        var startX = GamePreview.TimeToScreenCoords(Time < Chart.CurrentTimeRaw ?
                                                        Chart.CurrentTimeRaw : Time);
        var endY = -startY;
        var noteY = Utils.MapRanges(Math.Clamp(Chart.CurrentTimeRaw, Time, EndTime), Time, EndTime,
                              startY, endY);
        
        var fillBrush = _doubleStyles.FillBrush;
        var outlineBrush = _doubleStyles.OutlineBrush;
        if (Time < Chart.CurrentTimeRaw)
        {
            fillBrush = _doubleStyles.TailFillBrush;
            outlineBrush = _doubleStyles.TailOutlineBrush;
        }

        if (Config.Settings.EnhancedPreview)
        {
            var endX = GamePreview.TimeToScreenCoords(EndTime);
            dc.DrawEllipse(_doubleStyles.TailFillBrush, null, new Point(endX, endY), 20, 20);
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