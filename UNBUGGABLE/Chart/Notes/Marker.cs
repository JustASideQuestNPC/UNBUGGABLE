using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class MarkerNote : NoteBase
{
    private static readonly List<SolidColorBrush> Brushes = [];
    private static readonly List<TransformGroup> Transforms = [];
    
    public override NoteType Type => NoteType.MARKER;
    public override NoteLane Lane => NoteLane.MARKER;

    public bool Color1
    {
        get => _colorStates[0];
        set => _colorStates[0] = value;
    }
    public bool Color2
    {
        get => _colorStates[1];
        set => _colorStates[1] = value;
    }
    public bool Color3
    {
        get => _colorStates[2];
        set => _colorStates[2] = value;
    }

    private readonly List<bool> _colorStates = [false, false, false];
    
    private readonly Geometry _shape = new PolylineGeometry([
        new Point(-12, -10),
        new Point(0, 0),
        new Point(-12, 10)
    ], true);

    public static void UpdateStyles()
    {
        Brushes.Clear();
        Transforms.Clear();
        var arrowScale = (double)App.Current.Resources["NoteViewer.Markers.ArrowScale"];
        for (var i = 0; i < 3; ++i)
        {
            var offset = 147 - i * 6 * arrowScale;
            Brushes.Add((SolidColorBrush)App.Current.Resources[$"NoteViewer.Markers.Color{i + 1}"]);
            Transforms.Add(new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(arrowScale, arrowScale),
                    new TranslateTransform(offset, 0)
                }
            });
        }
    }

    public MarkerNote(long time)
    {
        Time = time;
    }
    
    // markers can never be selected
    public override void Render(DrawingContext dc, bool _)
    {
        var y = NoteViewer.TimeToScreenCoords(Time);
        if (y < -50 || y > NoteViewer.ViewerHeight + 50)
        {
            return;
        }

        var transformIndex = 0;
        for (var i = 0; i < _colorStates.Count; ++i)
        {
            if (_colorStates[i])
            {
                _shape.Transform = new TransformGroup { Children =
                {
                    Transforms[transformIndex++],
                    new TranslateTransform(i, y),
                } };
                dc.DrawGeometry(Brushes[i], null, _shape);
            }
        }
    }

    public override void RenderPreview(DrawingContext dc)
    {
        if (Time < Chart.CurrentTimeRaw || Time > Chart.CurrentTimeRaw + 1000)
        {
            return;
        }

        var color =
            Color1 && Config.Settings.MarkerPreviews.Color1 ? Brushes[0] :
            Color2 && Config.Settings.MarkerPreviews.Color2 ? Brushes[1] :
            Color3 && Config.Settings.MarkerPreviews.Color3 ? Brushes[2] :
            null;

        if (color == null)
        {
            return;
        }
        
        var pen = new Pen(color, 2);
        var x = GamePreview.TimeToScreenCoords(Time);
        dc.DrawLine(pen, new Point(x, -GamePreview.PreviewHeight / 2), 
                    new Point(x, GamePreview.PreviewHeight / 2));
    }
    
    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time < rangeStart || Time > rangeEnd)
        {
            return null;
        }
        
        var offset = (long)(Time - rangeStart);
        if ((_colorStates[0] && Config.Settings.HitSounds.Marker1) ||
            (_colorStates[1] && Config.Settings.HitSounds.Marker2) ||
            (_colorStates[2] && Config.Settings.HitSounds.Marker3))
        {
            return offset;
        }
        
        return null;
    }
    
    public override string ToHitObjectString(bool isFirstNote, bool isStandardFile)
    {
        if (Config.Settings.SaveMarkersInLane2 || isStandardFile)
        {
            return $"128,192,{Time + Chart.Metadata.ChartOffset}," +
                   $"{(isFirstNote ? 1 : 5)},{GetFlagString()},0:0:0:0:";
        }

        return "";
    }

    public override string ToCopyPasteString(long startTime)
    {
        var type = (int)Type;
        var time = Time - startTime;
        var colorString = Convert.ToInt32(
            $"{(_colorStates[0] ? 1 : 0)}{(_colorStates[1] ? 1 : 0)}{(_colorStates[2] ? 1 : 0)}",
            2);
        return $"{type},{time},{colorString}";
    }

    public override string ToString() => $"Marker: Colors={string.Join(",", _colorStates)} " +
                                         $"Time={Time}ms";
}