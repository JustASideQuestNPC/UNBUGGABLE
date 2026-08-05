using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class SingleNote : NoteBase
{
    private class StyleGroup
    {
        public required SolidColorBrush FillBrush;
        public required SolidColorBrush OutlineBrush;
        public required double OutlineThickness;
        public required SolidColorBrush SelectedFillBrush;
        public required SolidColorBrush SelectedOutlineBrush;
        public required double SelectedOutlineThickness;
    }
    
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
    
    private static StyleGroup SingleStyles;
    private static StyleGroup SpikeStyles;
    
    public override NoteType Type => (Flags.W ? NoteType.SPIKE : NoteType.SINGLE);
    
    private readonly Geometry _spikeShape = new PolylineGeometry(SpikeVertices, true);
    private readonly Geometry _spikePreviewShape = new PolylineGeometry(SpikePreviewVertices, true);
    
    public static void UpdateStyles()
    {
        SingleStyles = new StyleGroup
        {
            FillBrush = (SolidColorBrush)App.Current.Resources["Notes.Single.FillColor"],
            OutlineBrush = (SolidColorBrush)App.Current.Resources["Notes.Single.OutlineColor"],
            OutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Single.OutlineThickness"]).Top,

            SelectedFillBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Single.Selected.FillColor"],
            SelectedOutlineBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Single.Selected.OutlineColor"],
            SelectedOutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Single.Selected.OutlineThickness"]).Top
        };
        SpikeStyles = new StyleGroup
        {
            FillBrush = (SolidColorBrush)App.Current.Resources["Notes.Spike.FillColor"],
            OutlineBrush = (SolidColorBrush)App.Current.Resources["Notes.Spike.OutlineColor"],
            OutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Spike.OutlineThickness"]).Top,

            SelectedFillBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Spike.Selected.FillColor"],
            SelectedOutlineBrush =
                (SolidColorBrush)App.Current.Resources["Notes.Spike.Selected.OutlineColor"],
            SelectedOutlineThickness =
                ((Thickness)App.Current.Resources["Notes.Spike.Selected.OutlineThickness"]).Top
        };
    }

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
        if (Time < Chart.CurrentTimeRaw || Time > Chart.CurrentTimeRaw + 1000)
        {
            return;
        }
        
        var y = Flags.N ? TimeToNoiszPreviewY(Time) :
            Lane == NoteLane.TOP ? GamePreview.TopLaneY : GamePreview.BottomLaneY;
        
        if (Type == NoteType.SINGLE)
        {
            RenderSinglePreview(dc, y);
        }
        else
        {
            RenderSpikePreview(dc, y);
        }
    }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        var shouldPlay = Type switch
        {
            NoteType.SINGLE => Time > rangeStart && Time <= rangeEnd &&
                               Config.Settings.HitSounds.Single,
            NoteType.SPIKE => Time > rangeStart && Time <= rangeEnd &&
                              Config.Settings.HitSounds.Spike,
            _ => false
        };

        if (shouldPlay)
        {
            return (long)(Time - rangeStart);
        }

        return null;
    }
    
    private void RenderSingle(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);

        if (y < -50 || y > NoteViewer.ViewerHeight + 50)
        {
            return;
        }
        
            
        var pen = selected ?
            new Pen(SingleStyles.SelectedOutlineBrush, SingleStyles.SelectedOutlineThickness) :
            new Pen(SingleStyles.OutlineBrush, SingleStyles.OutlineThickness);
        dc.DrawRectangle(selected ? SingleStyles.SelectedFillBrush : SingleStyles.FillBrush, pen,
                         new Rect(x - 40, y - 12, 80, 24));
        
        RenderFlags(dc, x, y);
        RenderDebugTime(dc, x, y);
    }

    private void RenderSinglePreview(DrawingContext dc, double y)
    {
        dc.DrawEllipse(SingleStyles.FillBrush, new Pen(SingleStyles.OutlineBrush, 6),
                       new Point(GamePreview.TimeToScreenCoords(Time), y), 30, 30);
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
        
            
        var pen = selected ?
            new Pen(SpikeStyles.SelectedOutlineBrush, SpikeStyles.SelectedOutlineThickness) :
            new Pen(SpikeStyles.OutlineBrush, SpikeStyles.OutlineThickness);
        dc.DrawGeometry(selected ? SpikeStyles.SelectedFillBrush : SpikeStyles.FillBrush, pen,
                        shape);
        
        RenderFlags(dc, x, y, new NoteFlags(Flags.C, Flags.F, false, Flags.N));
        RenderDebugTime(dc, x, y);
    }

    private void RenderSpikePreview(DrawingContext dc, double y)
    {
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
        
        dc.DrawGeometry(SpikeStyles.FillBrush, new Pen(SpikeStyles.OutlineBrush, 6), shape);
    }
    
    private double TimeToNoiszPreviewY(double time)
    {
        var rangeStart = (Lane == NoteLane.TOP ? -GamePreview.TopLaneY : GamePreview.BottomLaneY);
        var y =
            Math.Clamp((Chart.CurrentTimeRaw - time) / 1000 * (GamePreview.PixelsPerSecond / 4.0) +
                       rangeStart, 0, rangeStart);
        return (Lane == NoteLane.TOP ? -y : y);
    }
    
    public override string ToString()
    {
        return $"{(Type == NoteType.SINGLE ? "Single" : "Spike")}: Lane={Lane}, " +
               $"Time={Time}ms";
    }
}