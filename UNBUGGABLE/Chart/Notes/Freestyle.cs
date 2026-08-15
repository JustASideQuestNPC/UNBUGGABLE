using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class FreestyleNote : NoteBase
{
    public override NoteType Type => Config.Settings.NegativeMashConversion && Flags.F ?
        NoteType.NEGATIVE_MASH : NoteType.FREESTYLE;
    
    public override NoteLane Lane => NoteLane.CENTER;
    
    // these are public because the game preview occasionally needs access to them
    public static SolidColorBrush FillBrush;
    public static SolidColorBrush OutlineBrush;
    
    private static double _outlineThickness;
    private static SolidColorBrush _selectedOutlineBrush;
    private static SolidColorBrush _selectedFillBrush;
    private static double _selectedOutlineThickness;
    
    public static void UpdateStyles()
    {
        FillBrush = (SolidColorBrush)App.Current.Resources["Notes.Freestyle.FillColor"];
        OutlineBrush = (SolidColorBrush)App.Current.Resources["Notes.Freestyle.OutlineColor"];
        _outlineThickness =
            ((Thickness)App.Current.Resources["Notes.Freestyle.OutlineThickness"]).Top;
        _selectedFillBrush =
            (SolidColorBrush)App.Current.Resources["Notes.Freestyle.Selected.FillColor"];
        _selectedOutlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.Freestyle.Selected.OutlineColor"];
        _selectedOutlineThickness =
            ((Thickness)App.Current.Resources["Notes.Freestyle.Selected.OutlineThickness"]).Top;
    }
    
    public override void Render(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);

        if (y < -50 || y > NoteViewer.ViewerHeight + 50)
        {
            return;
        }

        var pen = selected ?
            new Pen(_selectedOutlineBrush, _selectedOutlineThickness) :
            new Pen(OutlineBrush, _outlineThickness);
        var fill = selected ? _selectedFillBrush : FillBrush;
        
        var rect = new Rect(x - 40, y - 12, 80, 24);
        var parentNote = Chart.GetPreviousNote(this);
        if (parentNote?.Type == NoteType.FREESTYLE && Type == NoteType.FREESTYLE)
        {
            rect = new Rect(x - 24, y - 12, 48, 24);
            
            fill = selected ? MashNote.SelectedFillBrush : MashNote.FillBrush;
            pen = selected ?
                new Pen(MashNote.SelectedOutlineBrush, MashNote.SelectedOutlineThickness) :
                new Pen(MashNote.OutlineBrush, MashNote.OutlineThickness);
        }
        
        
        dc.DrawRectangle(fill, pen, rect);
        
        RenderFlags(dc, x, y);
        
        RenderDebugTime(dc, x, y);
    }

    public override void RenderPreview(DrawingContext dc)
    {
        if (Time < Chart.CurrentTimeRaw || Time > Chart.CurrentTimeRaw + 1000)
        {
            return;
        } 
        
        var x = GamePreview.TimeToScreenCoords(Time < Chart.CurrentTimeRaw ?
                                                   Chart.CurrentTimeRaw : Time);
        
        if (Type == NoteType.NEGATIVE_MASH)
        {
            var rect = new RoundedRect(
                new Rect(x - 30, GamePreview.TopLaneY, 60,
                         -GamePreview.TopLaneY + GamePreview.BottomLaneY), 30);
            dc.DrawRectangle(MashNote.FillBrush, new Pen(MashNote.OutlineBrush, 6), rect);
            return;
        }
        
        var parentNote = Chart.GetPreviousNote(this);
        var isSubNote = parentNote?.Type == NoteType.FREESTYLE
                        && !(Config.Settings.NegativeMashConversion && parentNote.Flags.F);
        if (isSubNote)
        {
            dc.DrawEllipse(FillBrush, new Pen(OutlineBrush, 6), new Point(x, 0), 15, 15);
        }
        else
        {
            dc.DrawEllipse(FillBrush, new Pen(OutlineBrush, 6), new Point(x, 0), 30, 30);
        }
    }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time > rangeStart && Time <= rangeEnd && Config.Settings.HitSounds.Freestyle)
        {
            return (long)(Time - rangeStart);
        }

        return null;
    }

    public override string ToHitObjectString(bool isFirstNote, bool isStandardFile)
    {
        if (Config.Settings.NegativeMashConversion && Flags.F)
        {
            // convert to a mash note with the end set to the very beginning of the chart
            List<string> chunks = [
                "469",
                "192",
                (Time + Chart.Metadata.ChartOffset).ToString(),
                isFirstNote ? "132" : "128",
                GetFlagString(),
                "0:0:0:0:0:"
            ];
            return string.Join(",", chunks);
        }
        return base.ToHitObjectString(isFirstNote, isStandardFile);
    }

    public override string ToString() => $"Freestyle: Time={Time}ms";
}