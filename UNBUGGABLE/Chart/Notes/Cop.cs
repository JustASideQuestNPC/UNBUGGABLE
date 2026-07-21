using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public class CopNote : NoteBase
{
    private const double PreviewPixelsPerSecond = 200;
    
    private static readonly List<Point> FinisherStarVertices =
    [
        new(0.000, -20.000),
        new(7.302, -9.670),
        new(27.778, -16.452),
        new(19.117, -5.977),
        new(44.946, -6.284),
        new(23.630, 0.000),
        new(44.946, 6.284),
        new(19.117, 5.977),
        new(27.778, 16.452),
        new(7.302, 9.670),
        new(0.000, 20.000),
        new(-7.302, 9.670),
        new(-27.778, 16.452),
        new(-19.117, 5.977),
        new(-44.946, 6.284),
        new(-23.630, 0.000),
        new(-44.946, -6.284),
        new(-19.117, -5.977),
        new(-27.778, -16.452),
        new(-7.302, -9.670),
    ];
    
    public override NoteType Type { get; }

    /// <summary>
    /// (Cop notes only) Whether the note should kill the current cop.
    /// </summary>
    public bool IsFinisher => Flags.F;
    
    private readonly Geometry _finisherStar = new PolylineGeometry(FinisherStarVertices, true);
    
    private readonly SolidColorBrush _fillBrush;
    private readonly SolidColorBrush _tailBrush;

    public CopNote(NoteType type, int id, bool finisher = false)
    {
        Type = type;
        CopId = id;
        if (finisher)
        {
            Flags.F = true;
        }
        
        _outlineBrush = (SolidColorBrush)App.Current.Resources["NoteOutline"];
        _fillBrush = (SolidColorBrush)App.Current.Resources[id switch
        {
            1 => "Cop1",
            2 => "Cop2",
            3 => "Cop3",
            _ => "Cop4"
        }];
        _tailBrush = new SolidColorBrush(_fillBrush.Color)
        {
            Opacity = 0.6
        };
        
        _typeface = new Typeface((FontFamily)App.Current.Resources["RobotoMonoBold"]);
    }
    
    public override void Render(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var startY = NoteViewer.TimeToScreenCoords(Time);
        if (!Instant)
        {
            var endY = NoteViewer.TimeToScreenCoords(EndTime);
            if ((startY < -50 && endY < -50) ||
                (startY > NoteViewer.ViewerHeight + 50 && endY > NoteViewer.ViewerHeight + 50))
            {
                return;
            }

            if (Type == NoteType.COP_MASH)
            {
                var clip = dc.PushClip(new Rect(x - 24, startY, 48, endY - startY));
                
                dc.DrawLine(new Pen(_fillBrush, 8), new Point(x - 24, endY),
                            new Point(x + 24, endY));
                for (var y = startY; y < endY; y += 32)
                {
                    dc.DrawLine(new Pen(_fillBrush, 5), new Point(x - 24, y),
                                new Point(x + 24, y + 16));
                    dc.DrawLine(new Pen(_fillBrush, 5), new Point(x - 24, y + 32),
                                new Point(x + 24, y + 16));
                }
                clip.Dispose();
            }
            else
            {
                dc.DrawRectangle(_tailBrush, null, new Rect(x - 16, startY, 32, endY - startY));
            }
        }
        else if (startY < -50 || startY > NoteViewer.ViewerHeight + 50)
        {
            return;
        }
        
        if (IsFinisher)
        {
            var shape = _finisherStar.Clone();
            shape.Transform = new TranslateTransform(x, startY);
            dc.DrawGeometry(_fillBrush, new Pen(_outlineBrush, 4), shape);
            if (selected)
            {
                dc.DrawGeometry(_outlineBrush, new Pen(_outlineBrush, 4), shape);
            }
        }
        else
        {
            dc.DrawRectangle(_fillBrush, new Pen(_outlineBrush, 4),
                             new Rect(x - 40, startY - 12, 80, 24));
            if (selected)
            {
                dc.DrawRectangle(null, new Pen(_selectedBrush, 4),
                                 new Rect(x - 40, startY - 12, 80, 24));
            }
        }
        
        var textColor = (SolidColorBrush)App.Current.Resources["TextPrimary"];
        var textOutline = new Pen((SolidColorBrush)App.Current.Resources["TextDark"], 2);
        var text = new FormattedText(CopId.ToString(), CultureInfo.CurrentCulture,
                                     FlowDirection.LeftToRight, _typeface, 40, textColor);
        dc.DrawOutlinedText(text, new Point(x - text.Width / 2, startY - 2 - text.Height / 2),
                            textColor, textOutline);
    }

    public override void RenderPreview(DrawingContext dc)
    {
        var x = (GamePreview.CurrentNotesFromRight ? GamePreview.NoteTargetX + 90 :
            -GamePreview.NoteTargetX - 90);
        
        switch (Type)
        {
            case NoteType.COP_SINGLE:
                RenderSinglePreview(dc, x);
                break;
            case NoteType.COP_HOLD:
                RenderHoldPreview(dc, x);
                break;
            case NoteType.COP_MASH:
                RenderMashPreview(dc, x);
                break;
        }
    }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        switch (Type)
        {
            case NoteType.COP_SINGLE:
                if (Time > rangeStart && Time <= rangeEnd && Config.HitSounds.CopSingle)
                {
                    return (long)(Time - rangeStart);
                }
                break;
            case NoteType.COP_HOLD:
                if (Time > rangeStart && Time <= rangeEnd && Config.HitSounds.CopHoldStart)
                {
                    return (long)(Time - rangeStart);
                }
                if (EndTime > rangeStart && EndTime <= rangeEnd && Config.HitSounds.CopHoldEnd)
                {
                    return (long)(EndTime - rangeStart);
                }
                break;
            case NoteType.COP_MASH:
                if (Time > rangeStart && Time <= rangeEnd && Config.HitSounds.CopMashStart)
                {
                    return (long)(Time - rangeStart);
                }
                if (EndTime > rangeStart && EndTime <= rangeEnd && Config.HitSounds.CopMashEnd)
                {
                    return (long)(EndTime - rangeStart);
                }
                break;
        }

        return null;
    }

    public override bool MouseOver()
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);
        return IsFinisher ?
            Utils.PointInPolygon(FinisherStarVertices, new Point(x, y),
                                 ChartBuilder.MousePosition) :
            new Rect(x - 40, y - 12, 80, 24).Contains(ChartBuilder.MousePosition);
    }
    
    public override string ToHitObjectString(bool isFirstNote, bool isStandardFile)
    {
        List<string> chunks =
        [
            Lane switch
            {
                NoteLane.TOP => "213",
                NoteLane.BOTTOM => "298",
                NoteLane.CENTER => "384",
                NoteLane.CAMERA => "469",
                _ => throw new ArgumentOutOfRangeException()
            },
            "192",
            Time.ToString(),
        ];
        
        if (Instant)
        {
            chunks.Add(isFirstNote ? "5" : "1");
        }
        else
        {
            chunks.Add(isFirstNote ? "132" : "128");
        }

        var flagNumber = (CopId switch
        {
            1 => 0,
            2 => 2,
            3 => 8,
            _ => 10
        });
        if (IsFinisher)
        {
            flagNumber += 4;
        }
        chunks.Add(flagNumber.ToString());

        chunks.Add((Instant ? "3:" : $"{EndTime}:3:") +
                   (Type == NoteType.COP_MASH ? "0:0:0:" : "1:0:0:"));

        return string.Join(",", chunks);
    }

    private void RenderSinglePreview(DrawingContext dc, double x)
    {
        if (Time < Chart.CurrentTime || Time > Chart.CurrentTime + 1000)
        {
            return;
        }
        
        var y = TimeToPreviewCoords(Time);
        dc.DrawLine(new Pen(_fillBrush, 6), new Point(x - 30, y), new Point(x + 30, y));
        dc.DrawEllipse(null, new Pen(_fillBrush, 6), new Point(x, y), 18, 10);
    }
    
    private void RenderHoldPreview(DrawingContext dc, double x)
    {
        if (EndTime < Chart.CurrentTime || Time > Chart.CurrentTime + 1000)
        {
            return;
        }
        
        var startY = TimeToPreviewCoords(Time < Chart.CurrentTime ? Chart.CurrentTime : Time);
        var endY = TimeToPreviewCoords(EndTime);
        
        dc.DrawLine(new Pen(_fillBrush, 6), new Point(x - 30, startY), new Point(x + 30, startY));
        dc.DrawLine(new Pen(_fillBrush, 10), new Point(x, startY), new Point(x, endY));
        dc.DrawLine(new Pen(_fillBrush, 6), new Point(x - 30, endY), new Point(x + 30, endY));
    }
    
    private void RenderMashPreview(DrawingContext dc, double x)
    {
        if (EndTime < Chart.CurrentTime || Time > Chart.CurrentTime + 1000)
        {
            return;
        }
        
        var startY = TimeToPreviewCoords(Time < Chart.CurrentTime ? Chart.CurrentTime : Time);
        var endY = TimeToPreviewCoords(EndTime);
        
        var clip = dc.PushClip(Lane == NoteLane.TOP ? new Rect(x - 30, endY, 60, startY - endY) :
                                   new Rect(x - 30, startY, 60, endY - startY));
        
        dc.DrawLine(new Pen(_fillBrush, 6), new Point(x - 30, startY), new Point(x + 30, startY));
        dc.DrawLine(new Pen(_fillBrush, 6), new Point(x - 30, endY), new Point(x + 30, endY));
        for (var y = startY; y < endY; y += 32)
        {
            dc.DrawLine(new Pen(_fillBrush, 5), new Point(x - 24, y),
                        new Point(x + 24, y + 16));
            dc.DrawLine(new Pen(_fillBrush, 5), new Point(x - 24, y + 32),
                        new Point(x + 24, y + 16));
        }
        clip.Dispose();
    }
    
    private double TimeToPreviewCoords(double time)
    {
        var rangeStart = (Lane == NoteLane.TOP ? -GamePreview.TopLaneY - 20 :
            GamePreview.BottomLaneY - 20);
        
        var x = ((time - Chart.CurrentTime) / 1000) * PreviewPixelsPerSecond + rangeStart;
        return (Lane == NoteLane.TOP ? -x : x);
    }

}