using System;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class FreestyleNote : NoteBase
{
    public override NoteType Type => NoteType.FREESTYLE;
    
    public override NoteLane Lane => NoteLane.CENTER;
    
    private readonly SolidColorBrush _fillBrush =
        App.Current.Resources["Freestyle"] as SolidColorBrush;
    
    public override void Render(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);

        if (y < -50 || y > NoteViewer.ViewerHeight + 50)
        {
            return;
        }

        var rect = new Rect(x - 40, y - 12, 80, 24);
        if (Chart.GetPreviousNote(this)?.Type == NoteType.FREESTYLE &&
            Config.ShowSubFreestylesInNoteViewer)
        {
            rect = new Rect(x - 24, y - 12, 48, 24);
        }
        
        dc.DrawRectangle(_fillBrush, new Pen(_outlineBrush, 4), rect);
        if (selected)
        {
            dc.DrawRectangle(null, new Pen(_selectedBrush, 4), rect);
        }
        
        RenderFlags(dc, x, y);
    }

    public override void RenderPreview(DrawingContext dc)
    {
        var parentNote = Chart.GetPreviousNote(this);
        var isSubNote = parentNote?.Type == NoteType.FREESTYLE;
        
        if (Time < Chart.CurrentTime || Time > Chart.CurrentTime + 1000)
        {
            return;
        } 
        
        var x = GamePreview.TimeToScreenCoords(Time < Chart.CurrentTime ?
                                                   Chart.CurrentTime : Time);
        if (isSubNote)
        {
            if (parentNote?.Time < Chart.CurrentTime)
            {
                dc.DrawEllipse(_fillBrush, new Pen(_outlineBrush, 6), new Point(
                                   GamePreview.TimeToScreenCoords(Chart.CurrentTime), 0), 30, 30);
            }
            dc.DrawEllipse(_fillBrush, new Pen(_outlineBrush, 6), new Point(x, 0), 15, 15);
        }
        else
        {
            dc.DrawEllipse(_fillBrush, new Pen(_outlineBrush, 6), new Point(x, 0), 30, 30);
        }
    }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time > rangeStart && Time <= rangeEnd && Config.HitSounds.Freestyle)
        {
            return (long)(Time - rangeStart);
        }

        return null;
    }

    public override string ToString() => $"Freestyle: Time={Time}ms";
}