using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class MashNote : NoteBase
{
    // these are public because they're used by negative mashes (which use the FreestyleNote class)
    public static SolidColorBrush FillBrush;
    public static SolidColorBrush OutlineBrush;
    public static double OutlineThickness;
    public static SolidColorBrush SelectedFillBrush;
    public static SolidColorBrush SelectedOutlineBrush;
    public static double SelectedOutlineThickness;
    
    private static SolidColorBrush _tailFillBrush;
    private static SolidColorBrush _tailOutlineBrush;
    private static double _tailOutlineThickness;
    private static SolidColorBrush _selectedTailFillBrush;
    private static SolidColorBrush _selectedTailOutlineBrush;
    private static double _selectedTailOutlineThickness;
    
    public override NoteType Type => NoteType.MASH;
    
    public override NoteLane Lane => NoteLane.CENTER;

    public static void UpdateStyles()
    {
        FillBrush = (SolidColorBrush)App.Current.Resources["Notes.Mash.FillColor"];
        OutlineBrush = (SolidColorBrush)App.Current.Resources["Notes.Mash.OutlineColor"];
        OutlineThickness =
            ((Thickness)App.Current.Resources["Notes.Mash.OutlineThickness"]).Top;
        _tailFillBrush = (SolidColorBrush)App.Current.Resources["Notes.Mash.TailColor"];
        _tailOutlineBrush = (SolidColorBrush)App.Current.Resources["Notes.Mash.TailOutlineColor"];
        _tailOutlineThickness =
            ((Thickness)App.Current.Resources["Notes.Mash.TailOutlineThickness"]).Top;

        SelectedFillBrush = (SolidColorBrush)App.Current.Resources["Notes.Mash.Selected.FillColor"];
        SelectedOutlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.Mash.Selected.OutlineColor"];
        SelectedOutlineThickness =
            ((Thickness)App.Current.Resources["Notes.Mash.Selected.OutlineThickness"]).Top;
        _selectedTailFillBrush =
            (SolidColorBrush)App.Current.Resources["Notes.Mash.Selected.TailColor"];
        _selectedTailOutlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.Mash.Selected.TailOutlineColor"];
        _selectedTailOutlineThickness =
            ((Thickness)App.Current.Resources["Notes.Mash.Selected.TailOutlineThickness"]).Top;
    }

    public MashNote(NoteFlags? startingFlags = null) : base(startingFlags)
    {
        Flags.F = true;
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
        
        var tailPen = selected ?
            new Pen(_selectedTailOutlineBrush, _selectedTailOutlineThickness) : 
            new Pen(_tailOutlineBrush, _tailOutlineThickness);
        dc.DrawRectangle(selected ? _selectedTailFillBrush : _tailFillBrush, tailPen,
                         new Rect(x - 16, startY, 32, endY - startY));
            
        var pen = selected ?
            new Pen(SelectedOutlineBrush, SelectedOutlineThickness) : 
            new Pen(OutlineBrush, OutlineThickness);
        dc.DrawRectangle(selected ? SelectedFillBrush : FillBrush, pen,
                         new Rect(x - 40, startY - 12, 80, 24));
        
        RenderFlags(dc, x, startY, new NoteFlags(Flags.C, false, Flags.W));
        
        RenderDebugTime(dc, x, startY, endY);
    }

    public override void RenderPreview(DrawingContext dc)
    {
        if (EndTime < Chart.CurrentTimeRaw || Time > Chart.CurrentTimeRaw + 1000)
        {
            return;
        }
        
        var startX = GamePreview.TimeToScreenCoords(Time < Chart.CurrentTimeRaw ?
                                                        Chart.CurrentTimeRaw : Time);
        var endX = GamePreview.TimeToScreenCoords(EndTime);
        var rect = new RoundedRect(
            new Rect(startX - 30, GamePreview.TopLaneY, 60,
                     -GamePreview.TopLaneY + GamePreview.BottomLaneY), 30);

        if (Config.Settings.EnhancedPreview)
        {
            dc.DrawLine(new Pen(_tailFillBrush, 40), new Point(startX, 0), new Point(endX, 0));
        }
        dc.DrawRectangle(FillBrush, new Pen(OutlineBrush, 6), rect);
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