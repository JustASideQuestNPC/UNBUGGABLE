using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class MashNote : NoteBase
{
    public override NoteType Type => NoteType.MASH;
    
    public override NoteLane Lane => NoteLane.CENTER;
    
    private readonly SolidColorBrush _fillBrush =
        App.Current.Resources["Freestyle"] as SolidColorBrush;
    private readonly SolidColorBrush _tailBrush;
    
    public MashNote(NoteFlags? startingFlags = null) : base(startingFlags)
    {
        _tailBrush = new SolidColorBrush(_fillBrush.Color)
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
        
        dc.DrawRectangle(_tailBrush, null, new Rect(x - 16, startY, 32, endY - startY));
        dc.DrawRectangle(_fillBrush, new Pen(_outlineBrush, 4),
                         new Rect(x - 40, startY - 12, 80, 24));
        if (selected)
        {
            dc.DrawRectangle(null, new Pen(_selectedBrush, 4),
                             new Rect(x - 40, startY - 12, 80, 24));
        }
        
        RenderFlags(dc, x, startY);
    }

    public override void RenderPreview(DrawingContext dc)
    {
        if (EndTime < Chart.CurrentTime || Time > Chart.CurrentTime + 1000)
        {
            return;
        }
        
        var startX = GamePreview.TimeToScreenCoords(Time < Chart.CurrentTime ?
                                                        Chart.CurrentTime : Time);
        var endX = GamePreview.TimeToScreenCoords(EndTime);
        var rect = new RoundedRect(
            new Rect(startX - 30, GamePreview.TopLaneY, 60,
                     -GamePreview.TopLaneY + GamePreview.BottomLaneY), 30);

        if (Config.Settings.EnhancedPreview)
        {
            dc.DrawLine(new Pen(_tailBrush, 40), new Point(startX, 0), new Point(endX, 0));
        }
        dc.DrawRectangle(_fillBrush, new Pen(_outlineBrush, 6), rect);
    }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time > rangeStart && Time <= rangeEnd && Config.Settings.HitSounds.HoldStart)
        {
            return (long)(Time - rangeStart);
        }
        if (EndTime > rangeStart && EndTime <= rangeEnd && Config.Settings.HitSounds.HoldEnd)
        {
            return (long)(EndTime - rangeStart);
        }
        
        return null;
    }
    
    public override string ToString() => $"Mash: Time={Time}-{EndTime}ms";
}