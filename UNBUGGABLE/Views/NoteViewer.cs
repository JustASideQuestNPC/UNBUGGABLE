using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Input;
using UNBUGGABLE.Resources;

namespace UNBUGGABLE.Views;

internal class LineStyle
{
    public SolidColorBrush Color;
    public double Thickness;
}

internal class LabeledLineStyle : LineStyle
{
    public double TextSize;
}

public class NoteViewer : Control
{
    public static double ViewerWidth => 560;
    public static double ViewerHeight { get; private set; }
    public static double CurrentZoom { get; private set; } = 1.0;
    
    private static int _topLaneX;
    private static int _centerLaneX;
    private static int _bottomLaneX;
    private static int _cameraLaneX;

    // how many pixels a single second is on the viewer at 1.0 zoom
    private const int PixelsPerSecond = 150;

    private static SolidColorBrush _backgroundBrush;
    private static SolidColorBrush _outlineBrush;
    private static SolidColorBrush _selectDragBrush;
    private static SolidColorBrush _deleteDragBrush;
    private static SolidColorBrush _laneNumberFillBrush;
    private static SolidColorBrush _laneNumberOutlineBrush;
    
    private static SolidColorBrush _topLaneBackgroundBrush;
    private static SolidColorBrush _centerLaneBackgroundBrush;
    private static SolidColorBrush _bottomLaneBackgroundBrush;
    private static SolidColorBrush _cameraLaneBackgroundBrush;
    
    private static double _topLaneWidth;
    private static double _bottomLaneWidth;
    private static double _centerLaneWidth;
    private static double _cameraLaneWidth;

    private static double _outlineThickness;
    private static double _laneNumberOutlineThickness;
    private static double _laneNumberTextSize;
    private static double _breakpointArrowScale;
    private static double _cornerRadius;

    private static LabeledLineStyle _fullBeatSnapLineStyle;
    private static LabeledLineStyle _bpmChangeStyle;
    private static LabeledLineStyle _labelStyle;
    private static LineStyle _subBeatSnapLineStyle;
    private static LineStyle _currentTimeLineStyle;
    private static LineStyle _breakpointLineStyle;

    private static Typeface _numberTypeface =
        new((FontFamily)App.Current.Resources["RobotoMonoBold"]);
    private static Typeface _beatLineTypeface = new((FontFamily)App.Current.Resources["RobotoMono"]);
    
    private static Geometry _breakpointShape = new PolylineGeometry([
        new Point(-12, -10),
        new Point(  0,  0),
        new Point(-12,  10)
    ], true);

    public static void UpdateStyles()
    {
        UpdateNoteColumnPositions();
        
        _backgroundBrush = (SolidColorBrush)App.Current.Resources["NoteViewer.BackgroundColor"];
        _outlineBrush = (SolidColorBrush)App.Current.Resources["NoteViewer.OutlineColor"];
        _selectDragBrush = (SolidColorBrush)App.Current.Resources["NoteViewer.SelectDragColor"];
        _deleteDragBrush = (SolidColorBrush)App.Current.Resources["NoteViewer.DeleteDragColor"];
        _laneNumberFillBrush =
            (SolidColorBrush)App.Current.Resources["NoteViewer.LaneNumbers.Color"];
        _laneNumberOutlineBrush =
            (SolidColorBrush)App.Current.Resources["NoteViewer.LaneNumbers.OutlineColor"];
        
        _topLaneBackgroundBrush =
            (SolidColorBrush)App.Current.Resources["NoteViewer.NoteLanes.TopColor"];
        _bottomLaneBackgroundBrush =
            (SolidColorBrush)App.Current.Resources["NoteViewer.NoteLanes.BottomColor"];
        _centerLaneBackgroundBrush =
            (SolidColorBrush)App.Current.Resources["NoteViewer.NoteLanes.CenterColor"];
        _cameraLaneBackgroundBrush =
            (SolidColorBrush)App.Current.Resources["NoteViewer.NoteLanes.CameraColor"];
        
        _outlineThickness =
            ((Thickness)App.Current.Resources["NoteViewer.OutlineThickness"]).Top;
        _laneNumberOutlineThickness =
            ((Thickness)App.Current.Resources["NoteViewer.LaneNumbers.OutlineThickness"]).Top;
        _laneNumberTextSize =
            (double)App.Current.Resources["NoteViewer.LaneNumbers.TextSize"];
        _breakpointArrowScale = (double)App.Current.Resources["NoteViewer.Breakpoint.ArrowScale"];
        
        // corner radius is always the same on all corners
        _cornerRadius = ((CornerRadius)App.Current.Resources["NoteViewer.CornerRadius"]).TopLeft;
        
        _topLaneWidth = (double)App.Current.Resources["NoteViewer.NoteLanes.TopWidth"];
        _bottomLaneWidth = (double)App.Current.Resources["NoteViewer.NoteLanes.BottomWidth"];
        _centerLaneWidth = (double)App.Current.Resources["NoteViewer.NoteLanes.CenterWidth"];
        _cameraLaneWidth = (double)App.Current.Resources["NoteViewer.NoteLanes.CameraWidth"];

        _fullBeatSnapLineStyle = new LabeledLineStyle
        {
            Color = (SolidColorBrush)App.Current.Resources["NoteViewer.FullBeatSnapLine.Color"],
            Thickness = (double)App.Current.Resources["NoteViewer.FullBeatSnapLine.Thickness"],
            TextSize = (double)App.Current.Resources["NoteViewer.FullBeatSnapLine.TextSize"]
        };
        _bpmChangeStyle = new LabeledLineStyle
        {
            Color = (SolidColorBrush)App.Current.Resources["NoteViewer.BpmChange.Color"],
            Thickness = (double)App.Current.Resources["NoteViewer.BpmChange.LineThickness"],
            TextSize = (double)App.Current.Resources["NoteViewer.BpmChange.TextSize"]
        };
        _labelStyle = new LabeledLineStyle
        {
            Color = (SolidColorBrush)App.Current.Resources["NoteViewer.Label.Color"],
            Thickness = (double)App.Current.Resources["NoteViewer.Label.LineThickness"],
            TextSize = (double)App.Current.Resources["NoteViewer.Label.TextSize"]
        };

        _subBeatSnapLineStyle = new LineStyle
        {
            Color = (SolidColorBrush)App.Current.Resources["NoteViewer.SubBeatSnapLine.Color"],
            Thickness = (double)App.Current.Resources["NoteViewer.SubBeatSnapLine.Thickness"]
        };
        _currentTimeLineStyle = new LineStyle
        {
            Color = (SolidColorBrush)App.Current.Resources["NoteViewer.CurrentTimeLine.Color"],
            Thickness = (double)App.Current.Resources["NoteViewer.CurrentTimeLine.Thickness"]
        };
        _breakpointLineStyle = new LineStyle
        {
            Color = (SolidColorBrush)App.Current.Resources["NoteViewer.Breakpoint.Color"],
            Thickness = (double)App.Current.Resources["NoteViewer.Breakpoint.Thickness"]
        };
    }
    
    public static void UpdateNoteColumnPositions()
    {
        List<int> columnXPositions = [208, 306, 404, 502];
        for (var i = 0; i < Config.Settings.LaneOrder.Count; ++i)
        {
            var x = columnXPositions[i];
            switch (Config.Settings.LaneOrder[i])
            {
                case "top":
                    _topLaneX = x;
                    break;
                case "bottom":
                    _bottomLaneX = x;
                    break;
                case "center":
                    _centerLaneX = x;
                    break;
                case "camera":
                    _cameraLaneX = x;
                    break;
            }
        }
    }

    /// <summary>
    /// Given a time in milliseconds, returns the y coordinate of that time (based on the current
    /// zoom setting and scroll position).
    /// </summary>
    public static double TimeToScreenCoords(double time)
    {
        var scaledPixelsPerMs = PixelsPerSecond * CurrentZoom / 1000;
        var visibleRangeStart = Chart.CurrentTimeRaw - Config.Settings.CurrentTimePosition /
                                scaledPixelsPerMs;
        return (time - visibleRangeStart) * scaledPixelsPerMs;
    }
    
    /// <summary>
    /// Given a y coordinate in pixels, returns the time in milliseconds (based on the current
    /// zoom setting and scroll position).
    /// </summary>
    public static double ScreenCoordsToTime(double y)
    {
        var scaledPixelsPerMs = PixelsPerSecond * CurrentZoom / 1000;
        var visibleRangeStart = Chart.CurrentTimeRaw - Config.Settings.CurrentTimePosition /
                                scaledPixelsPerMs;
        return visibleRangeStart + y / scaledPixelsPerMs;
    }
    
    public static void SetZoom(double zoom)
    {
        CurrentZoom = zoom;
        App.MainWindowViewModel.CurrentZoomText = CurrentZoom.ToString("0.000");
    }
    
    public static void IncreaseZoom()
    {
        if (CurrentZoom < Config.Settings.MaxZoom)
        {
            CurrentZoom += Config.Settings.ZoomIncrement;
            App.MainWindowViewModel.CurrentZoomText = CurrentZoom.ToString("0.000");
        }
    }

    public static void DecreaseZoom()
    {
        if (CurrentZoom > Config.Settings.MinZoom)
        {
            CurrentZoom -= Config.Settings.ZoomIncrement;
            App.MainWindowViewModel.CurrentZoomText = CurrentZoom.ToString("0.000");
        }
    }

    public static List<NoteLane> GetSelectedLanes()
    {
        var left = Math.Min(ChartBuilder.MouseDragStart.Value.X, ChartBuilder.MousePosition.X);
        var right = Math.Max(ChartBuilder.MouseDragStart.Value.X, ChartBuilder.MousePosition.X);

        List<NoteLane> lanes = [];
        if (_topLaneX > left && _topLaneX < right)
        {
            lanes.Add(NoteLane.TOP);
        }

        if (_centerLaneX > left && _centerLaneX < right)
        {
            lanes.Add(NoteLane.CENTER);
        }

        if (_bottomLaneX > left && _bottomLaneX < right)
        {
            lanes.Add(NoteLane.BOTTOM);
        }

        if (_cameraLaneX > left && _cameraLaneX < right)
        {
            lanes.Add(NoteLane.CAMERA);
        }

        if (left < 150)
        {
            lanes.Add(NoteLane.MARKER);
        }

        return lanes;
    }

    public static int GetNoteX(NoteLane lane)
    {
        return lane switch
        {
            NoteLane.TOP => _topLaneX,
            NoteLane.BOTTOM => _bottomLaneX,
            NoteLane.CENTER => _centerLaneX,
            _ => _cameraLaneX
        };
    }
    
    public override void Render(DrawingContext dc)
    {
        ViewerHeight = Bounds.Size.Height;
        
        var clip = dc.PushClip(
            new RoundedRect(new Rect(0, 0, Bounds.Size.Width, Bounds.Size.Height), _cornerRadius));
        
        dc.DrawRectangle(_backgroundBrush, null, new Rect(0, 0, ViewerWidth, ViewerHeight));
        
        // note lanes
        dc.DrawRectangle(_topLaneBackgroundBrush, null,
                         new Rect(_topLaneX - _topLaneWidth / 2, 0, _topLaneWidth, ViewerHeight));
        dc.DrawRectangle(_bottomLaneBackgroundBrush, null,
                         new Rect(_bottomLaneX - _bottomLaneWidth / 2, 0, _bottomLaneWidth,
                                  ViewerHeight));
        dc.DrawRectangle(_centerLaneBackgroundBrush, null,
                         new Rect(_centerLaneX - _centerLaneWidth / 2, 0, _centerLaneWidth,
                                  ViewerHeight));
        dc.DrawRectangle(_cameraLaneBackgroundBrush, null,
                         new Rect(_cameraLaneX - _cameraLaneWidth / 2, 0, _cameraLaneWidth,
                                  ViewerHeight));
        
        // full beat lines
        // Trace.WriteLine(Chart.SongLoaded);
        if (Chart.SongLoaded)
        {
            var scaledPixelsPerMs = PixelsPerSecond * CurrentZoom / 1000;
            var visibleRangeStart = Chart.CurrentTimeRaw - Config.Settings.CurrentTimePosition /
                                    scaledPixelsPerMs;
            var visibleRangeEnd =
                Chart.CurrentTimeRaw + (ViewerHeight - Config.Settings.CurrentTimePosition) /
                scaledPixelsPerMs;
            
            // Trace.WriteLine($"Visible range: {visibleRangeStart} - {visibleRangeEnd}");
            foreach (var subBeatTime in Chart.GetSnapTimesInRange(visibleRangeStart,
                                                                         visibleRangeEnd))
            {
                var adjustedTime = subBeatTime - visibleRangeStart;
                dc.DrawLine(new Pen(_subBeatSnapLineStyle.Color, _subBeatSnapLineStyle.Thickness),
                            new Point(150, adjustedTime * scaledPixelsPerMs),
                            new Point(ViewerWidth, adjustedTime * scaledPixelsPerMs));
            }
            
            foreach (var beatLine in Chart.GetBeatTimesInRange(visibleRangeStart,
                                                                      visibleRangeEnd))
            {
                RenderFullBeatSnapLine(dc, beatLine, visibleRangeStart, scaledPixelsPerMs);
            }
        }
        
        // current time
        dc.DrawLine(new Pen(_currentTimeLineStyle.Color, _currentTimeLineStyle.Thickness),
                    new Point(150, Config.Settings.CurrentTimePosition),
                    new Point(ViewerWidth, Config.Settings.CurrentTimePosition));
        
        // render markers early so they don't cover up label names or BPM numbers
        foreach (var note in Chart.MarkerNotes)
        {
            note.Render(dc, ChartBuilder.SelectedNotes.Contains(note));
        }
        
        foreach (var bpmRegion in Chart.BpmRegions)
        {
            RenderBpmChange(dc, bpmRegion);
        }
         
        foreach (var label in Chart.Labels)
        {
            RenderLabel(dc, label);
        }
        
        // lane labels
        var topLaneKeybind = Utils.GetReadableKeybindString(Config.Keybinds.PlaceTopLane[0]);
        var bottomLaneKeybind = Utils.GetReadableKeybindString(Config.Keybinds.PlaceBottomLane[0]);
        var centerLaneKeybind = Utils.GetReadableKeybindString(Config.Keybinds.PlaceCenterLane[0]);
        var cameraLaneKeybind = Utils.GetReadableKeybindString(Config.Keybinds.PlaceCameraLane[0]);
        var laneNumberPen = new Pen(_laneNumberOutlineBrush, _laneNumberOutlineThickness);
        
        // brush color doesn't matter because it's ignored by DrawOutlinedText
        var topLaneText = new FormattedText(topLaneKeybind, CultureInfo.CurrentCulture,
                                            FlowDirection.LeftToRight, _numberTypeface,
                                            _laneNumberTextSize, Brushes.White);
        var centerLaneText = new FormattedText(centerLaneKeybind, CultureInfo.CurrentCulture,
                                               FlowDirection.LeftToRight, _numberTypeface,
                                               _laneNumberTextSize, Brushes.White);
        var bottomLaneText = new FormattedText(bottomLaneKeybind, CultureInfo.CurrentCulture,
                                               FlowDirection.LeftToRight, _numberTypeface,
                                               _laneNumberTextSize, Brushes.White);
        var cameraLaneText = new FormattedText(cameraLaneKeybind, CultureInfo.CurrentCulture,
                                               FlowDirection.LeftToRight, _numberTypeface,
                                               _laneNumberTextSize, Brushes.White);
        dc.DrawOutlinedText(topLaneText, new Point(_topLaneX - topLaneText.Width / 2,
                                                   Config.Settings.CurrentTimePosition - 2 -
                                                   topLaneText.Height / 2),
                            _laneNumberFillBrush, laneNumberPen);
        dc.DrawOutlinedText(centerLaneText, new Point(_centerLaneX - centerLaneText.Width / 2,
                                                      Config.Settings.CurrentTimePosition - 2 -
                                                      centerLaneText.Height / 2),
                            _laneNumberFillBrush, laneNumberPen);
        dc.DrawOutlinedText(bottomLaneText, new Point(_bottomLaneX - bottomLaneText.Width / 2,
                                                      Config.Settings.CurrentTimePosition - 2 -
                                                      bottomLaneText.Height / 2),
                            _laneNumberFillBrush, laneNumberPen);
        dc.DrawOutlinedText(cameraLaneText, new Point(_cameraLaneX - cameraLaneText.Width / 2,
                                                      Config.Settings.CurrentTimePosition - 2 -
                                                      cameraLaneText.Height / 2),
                            _laneNumberFillBrush, laneNumberPen);

        if (ChartBuilder.BreakpointTime != -1000)
        {
            RenderBreakpoint(dc);
        }
        
        foreach (var note in Chart.NonMarkerNotes)
        {
            note.Render(dc, ChartBuilder.SelectedNotes.Contains(note));
        }
        
        RenderPlacingNotes(dc);

        if (ChartBuilder.MouseDragStartTime.SoftNotEquals(-1000))
        {
            var startY = TimeToScreenCoords(ChartBuilder.MouseDragStartTime);
            var top = Math.Min(startY, ChartBuilder.MousePosition.Y);
            var bottom = Math.Max(startY, ChartBuilder.MousePosition.Y);
            var left = Math.Min(ChartBuilder.MouseDragStart.Value.X, ChartBuilder.MousePosition.X);
            var right = Math.Max(ChartBuilder.MouseDragStart.Value.X, ChartBuilder.MousePosition.X);
            dc.DrawRectangle(ChartBuilder.RightMouseDrag ? _deleteDragBrush : _selectDragBrush,
                             null, new Rect(left, top, right - left, bottom - top));
        }

        var outlinePen = new Pen(_outlineBrush, _outlineThickness);
        // dc.DrawRectangle(null, outlinePen,
        //                  new RoundedRect(new Rect(0, 0, ViewerWidth, ViewerHeight), _cornerRadius));
        dc.DrawLine(outlinePen, new Point(150, 0), new Point(150, ViewerHeight));
        
        clip.Dispose();
    }

    public static async Task<bool> CheckForEditByMouse(bool rightClick)
    {
        if (ChartBuilder.MousePosition.X > 150)
        {
            return false;
        }
        
        BpmRegion? hoveredRegion = null;
        foreach (var bpmRegion in Chart.BpmRegions)
        {
            var rangeStart = TimeToScreenCoords(bpmRegion.StartTime - Chart.AdjustedOffset) - 75;
            var rangeEnd = TimeToScreenCoords(bpmRegion.StartTime - Chart.AdjustedOffset) + 75;
            if (ChartBuilder.MousePosition.Y > rangeStart &&
                ChartBuilder.MousePosition.Y < rangeEnd)
            {
                hoveredRegion = bpmRegion;
                break;
            }
        }

        if (hoveredRegion != null)
        {
            if (rightClick)
            {
                if (hoveredRegion != Chart.BpmRegions[0])
                {
                    ChartBuilder.DeleteBpmRegion(hoveredRegion);
                    return true;
                }
            }
            else
            {
                await ChartBuilder.EditBpmRegion(hoveredRegion);
                return true;
            }
        }
        
        Chart.Label? hoveredLabel = null;
        foreach (var label in Chart.Labels)
        {
            var rangeStart = TimeToScreenCoords(label.Time - Chart.AdjustedOffset) - 75;
            var rangeEnd = TimeToScreenCoords(label.Time - Chart.AdjustedOffset) + 75;
            if (ChartBuilder.MousePosition.Y > rangeStart &&
                ChartBuilder.MousePosition.Y < rangeEnd)
            {
                hoveredLabel = label;
                break;
            }
        }

        if (hoveredLabel != null)
        {
            if (rightClick)
            {
                ChartBuilder.DeleteLabel(hoveredLabel);
            }
            else
            {
                await ChartBuilder.EditLabel(hoveredLabel);
            }
            return true;
        }

        return false;
    }

    private void RenderFullBeatSnapLine(DrawingContext dc, (double, int) snapLine,
        double visibleRangeStart, double scaledPixelsPerMs)
    {
        var time = snapLine.Item1;
        var index = snapLine.Item2;
        
        var y = (time - visibleRangeStart) * scaledPixelsPerMs;
        dc.DrawLine(new Pen(_fullBeatSnapLineStyle.Color, _fullBeatSnapLineStyle.Thickness),
                    new Point(150, y), new Point(ViewerWidth, y));
        
        var beatNumberText = new FormattedText(index.ToString(), CultureInfo.CurrentCulture,
                                               FlowDirection.RightToLeft, _beatLineTypeface,
                                               _fullBeatSnapLineStyle.TextSize,
                                               _fullBeatSnapLineStyle.Color);
        dc.DrawText(beatNumberText, new Point(ViewerWidth - beatNumberText.Width - 10,
                                              y - beatNumberText.Height - 2));
    }

    private void RenderBpmChange(DrawingContext dc, BpmRegion bpmRegion)
    {
        var y = TimeToScreenCoords(bpmRegion.StartTime == Chart.Metadata.ChartOffset ? 0 :
                                       bpmRegion.StartTime);
        if (y < -50 || y > ViewerHeight + 50)
        {
            return;
        }
        
        var text = new FormattedText(bpmRegion.Bpm.ToString("0.00"), CultureInfo.CurrentCulture,
                                     FlowDirection.LeftToRight, _numberTypeface,
                                     _bpmChangeStyle.TextSize, _bpmChangeStyle.Color)
        {
            MaxTextWidth = 122
        };
        
        dc.DrawText(text, new Point(137 - text.Width, y - 2 - text.Height / 2));
        dc.DrawLine(new Pen(_bpmChangeStyle.Color, _bpmChangeStyle.Thickness), new Point(150, y),
                    new Point(ViewerWidth, y));
    }
    
    private void RenderLabel(DrawingContext dc, Chart.Label label)
    {
        var y = TimeToScreenCoords(label.Time - Chart.Metadata.ChartOffset);
        if (y < -50 || y > ViewerHeight + 50)
        {
            return;
        }

        var formattedText = new FormattedText(label.Text, CultureInfo.CurrentCulture,
                                              FlowDirection.LeftToRight, _numberTypeface,
                                              _labelStyle.TextSize,
                                              _labelStyle.Color)
        {
            MaxTextWidth = 122
        };
        
        dc.DrawText(formattedText,
                    new Point(137 - formattedText.Width, y - 2 - formattedText.Height / 2));
        dc.DrawLine(new Pen(_labelStyle.Color, _labelStyle.Thickness), new Point(150, y),
                    new Point(ViewerWidth, y));
    }

    private void RenderBreakpoint(DrawingContext dc)
    {
        var y = TimeToScreenCoords(ChartBuilder.BreakpointTime);
        if (y < -50 || y > ViewerHeight + 50)
        {
            return;
        }
        
        _breakpointShape.Transform = new TransformGroup
        {
            Children =
            {
                new ScaleTransform(_breakpointArrowScale, _breakpointArrowScale),
                new TranslateTransform(147, y)
            }
        };
        dc.DrawGeometry(_breakpointLineStyle.Color, null, _breakpointShape);
        dc.DrawLine(new Pen(_breakpointLineStyle.Color, _breakpointLineStyle.Thickness),
                    new Point(150, y), new Point(ViewerWidth, y));
    }

    private void RenderPlacingNotes(DrawingContext dc)
    {
        NoteBase? topNote = null, centerNote = null, bottomNote = null;
        // cop id doesn't change the center lane
        if (ChartBuilder.CenterLaneStartTime != -1000)
        {
            if (ChartBuilder.CenterLaneStartTime == Chart.CurrentTime)
            {
                centerNote = new FreestyleNote
                {
                    Time = Chart.CurrentTime
                };
            }
            else
            {
                centerNote = new MashNote
                {
                    Time = Math.Min(ChartBuilder.CenterLaneStartTime, Chart.CurrentTime),
                    EndTime = Math.Max(ChartBuilder.CenterLaneStartTime, Chart.CurrentTime)
                };
            }
        }

        if (ChartBuilder.TopLaneStartTime != -1000)
        {
            topNote = MakeNotePlaceholder(
                Math.Min(ChartBuilder.TopLaneStartTime, Chart.CurrentTime),
                Math.Max(ChartBuilder.TopLaneStartTime, Chart.CurrentTime));
            topNote.Lane = NoteLane.TOP;
        }
            
        if (ChartBuilder.BottomLaneStartTime != -1000)
        {
            bottomNote = MakeNotePlaceholder(
                Math.Min(ChartBuilder.BottomLaneStartTime, Chart.CurrentTime),
                Math.Max(ChartBuilder.BottomLaneStartTime, Chart.CurrentTime));
            bottomNote.Lane = NoteLane.BOTTOM;
        }

        topNote?.Render(dc, false);
        bottomNote?.Render(dc, false);
        centerNote?.Render(dc, false);
    }

    private NoteBase MakeNotePlaceholder(long start, long end)
    {
        if (ChartBuilder.CopId != 0)
        {
            if (start == end)
            {
                // holding shift and tapping always creates a spike
                if (InputManager.ShiftPressed)
                {
                    return new SingleNote
                    {
                        Time = start,
                        Flags = new NoteFlags(false, false, true)
                    };
                }
                
                return new CopNote(NoteType.COP_SINGLE, ChartBuilder.CopId)
                {
                    Time = start
                };
            }
            
            return new CopNote(InputManager.ShiftPressed ? NoteType.COP_MASH : NoteType.COP_HOLD,
                               ChartBuilder.CopId)
            {
                Time = start,
                EndTime = end
            };
        }
        
        if (start == end)
        {
            return new SingleNote
            {
                Time = start,
                Flags = new NoteFlags(false, false, InputManager.ShiftPressed)
            };
        }
        
        return new HoldNote
        {
            Time = start,
            EndTime = end,
            Flags = new NoteFlags(false, false, InputManager.ShiftPressed)
        };
    }
}