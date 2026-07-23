using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using UNBEATABLEChartEditor;
using UNBUGGABLE.Resources;

namespace UNBUGGABLE.Views;

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

    private readonly SolidColorBrush _singleNoteBackgroundBrush;
    private readonly SolidColorBrush _freestyleBackgroundBrush;
    private readonly SolidColorBrush _cameraSwapBackgroundBrush;
    private readonly SolidColorBrush _currentTimeLineBrush;
    private readonly SolidColorBrush _subBeatLineBrush;
    private readonly SolidColorBrush _fullBeatLineBrush;
    private readonly SolidColorBrush _selectDragOverlayBrush;
    private readonly SolidColorBrush _deleteDragOverlayBrush;
    
    private readonly SolidColorBrush _breakpointBrush =
        (SolidColorBrush)App.Current.Resources["Breakpoint"];
    private readonly SolidColorBrush _editorBackgroundBrush =
        (SolidColorBrush)App.Current.Resources["EditorBackground"];
    private readonly SolidColorBrush _accentBrush =
        (SolidColorBrush)App.Current.Resources["Accent"];
    private readonly SolidColorBrush _bpmChangeBrush =
        (SolidColorBrush)App.Current.Resources["BpmChange"];
    private readonly SolidColorBrush _laneNumberFillBrush =
        (SolidColorBrush)App.Current.Resources["TextPrimary"];
    private readonly SolidColorBrush _labelBrush = (SolidColorBrush)App.Current.Resources["Label"];
    
    private readonly Pen _textOutlinePen;

    private readonly Typeface _numberTypeface =
        new((FontFamily)App.Current.Resources["RobotoMonoBold"]);
    
    private readonly FormattedText _topLaneText;
    private readonly FormattedText _centerLaneText;
    private readonly FormattedText _bottomLaneText;
    private readonly FormattedText _cameraLaneText;
    
    private readonly Geometry _breakpointShape = new PolylineGeometry([
        new Point(-12, -10),
        new Point(  0,  0),
        new Point(-12,  10)
    ], true);
    
    public NoteViewer()
    {
        var singleNoteBrush = (SolidColorBrush)App.Current.Resources["SingleNote"];
        _singleNoteBackgroundBrush = new SolidColorBrush(singleNoteBrush.Color)
        {
            Opacity = 0.1
        };
        
        var freestyleBrush = (SolidColorBrush)App.Current.Resources["Freestyle"];
        _freestyleBackgroundBrush = new SolidColorBrush(freestyleBrush.Color)
        {
            Opacity = 0.1
        };
        
        var cameraSwapBrush = (SolidColorBrush)App.Current.Resources["CameraChange"];
        _cameraSwapBackgroundBrush = new SolidColorBrush(cameraSwapBrush.Color)
        {
            Opacity = 0.1
        };
        
        var fullBeatLineBrush = (SolidColorBrush)App.Current.Resources["FullBeatSnapLine"];
        _fullBeatLineBrush = new SolidColorBrush(fullBeatLineBrush.Color)
        {
            Opacity = 0.6
        };
        
        var subBeatLineBrush = (SolidColorBrush)App.Current.Resources["SubBeatSnapLine"];
        _subBeatLineBrush = new SolidColorBrush(subBeatLineBrush.Color)
        {
            Opacity = 0.6
        };
        
        var currentTimeLineBrush = (SolidColorBrush)App.Current.Resources["CurrentTimeLine"];
        _currentTimeLineBrush = new SolidColorBrush(currentTimeLineBrush.Color)
        {
            Opacity = 0.4
        };
        
        var selectDragOverlayBrush = (SolidColorBrush)App.Current.Resources["SelectDragOverlay"];
        _selectDragOverlayBrush = new SolidColorBrush(selectDragOverlayBrush.Color)
        {
            Opacity = 0.4
        };
        
        var deleteDragOverlayBrush = (SolidColorBrush)App.Current.Resources["DeleteDragOverlay"];
        _deleteDragOverlayBrush = new SolidColorBrush(deleteDragOverlayBrush.Color)
        {
            Opacity = 0.4
        };
        
        _textOutlinePen = new Pen((SolidColorBrush)App.Current.Resources["TextDark"], 2);
        
        // brush color doesn't matter because it's ignored by DrawOutlinedText
        _topLaneText = new FormattedText("3", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                         _numberTypeface, 40, Brushes.White);
        _centerLaneText = new FormattedText("6", CultureInfo.CurrentCulture,
                                            FlowDirection.LeftToRight, _numberTypeface, 40,
                                            Brushes.White);
        _bottomLaneText = new FormattedText("4", CultureInfo.CurrentCulture,
                                            FlowDirection.LeftToRight, _numberTypeface, 40,
                                            Brushes.White);
        _cameraLaneText = new FormattedText("5", CultureInfo.CurrentCulture,
                                            FlowDirection.LeftToRight, _numberTypeface, 40,
                                            Brushes.White);
        
        UpdateNoteColumnPositions();
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
        var visibleRangeStart = Chart.CurrentTime - Config.Settings.CurrentTimePosition /
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
        var visibleRangeStart = Chart.CurrentTime - Config.Settings.CurrentTimePosition /
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
        
        var clip = dc.PushClip(new Rect(0, 0, Bounds.Size.Width, Bounds.Size.Height));
        
        dc.DrawRectangle(_editorBackgroundBrush, null, new Rect(0, 0, ViewerWidth, ViewerHeight));
        
        // note lanes
        dc.DrawRectangle(_singleNoteBackgroundBrush, null,
                         new Rect(_topLaneX - 16, 0, 32, ViewerHeight));
        dc.DrawRectangle(_freestyleBackgroundBrush, null,
                         new Rect(_centerLaneX - 16, 0, 32, ViewerHeight));
        dc.DrawRectangle(_singleNoteBackgroundBrush, null,
                         new Rect(_bottomLaneX - 16, 0, 32, ViewerHeight));
        dc.DrawRectangle(_cameraSwapBackgroundBrush, null,
                         new Rect(_cameraLaneX - 16, 0, 32, ViewerHeight));
        
        // full beat lines
        // Trace.WriteLine(Chart.SongLoaded);
        if (Chart.SongLoaded)
        {
            var scaledPixelsPerMs = PixelsPerSecond * CurrentZoom / 1000;
            var visibleRangeStart = Chart.CurrentTime - Config.Settings.CurrentTimePosition /
                                    scaledPixelsPerMs;
            var visibleRangeEnd =
                Chart.CurrentTime + (ViewerHeight - Config.Settings.CurrentTimePosition) /
                scaledPixelsPerMs;
            
            // Trace.WriteLine($"Visible range: {visibleRangeStart} - {visibleRangeEnd}");
            foreach (var subBeatTime in Chart.GetSnapTimesInRange(visibleRangeStart,
                                                                         visibleRangeEnd))
            {
                var adjustedTime = subBeatTime - visibleRangeStart;
                dc.DrawLine(new Pen(_subBeatLineBrush, 3),
                            new Point(150, adjustedTime * scaledPixelsPerMs),
                            new Point(ViewerWidth, adjustedTime * scaledPixelsPerMs));
            }
            
            foreach (var beatTime in Chart.GetBeatTimesInRange(visibleRangeStart,
                                                                      visibleRangeEnd))
            {
                var adjustedTime = beatTime - visibleRangeStart;
                dc.DrawLine(new Pen(_fullBeatLineBrush, 3),
                            new Point(150, adjustedTime * scaledPixelsPerMs),
                            new Point(ViewerWidth, adjustedTime * scaledPixelsPerMs));
            }
        }
        
        // current time
        dc.DrawLine(new Pen(_currentTimeLineBrush, 8),
                    new Point(150, Config.Settings.CurrentTimePosition),
                    new Point(ViewerWidth, Config.Settings.CurrentTimePosition));
        
        foreach (var bpmRegion in Chart.BpmRegions)
        {
            RenderBpmChange(dc, bpmRegion);
        }
         
        foreach (var label in Chart.Labels)
        {
            RenderLabel(dc, label);
        }
        
        // lane labels
        dc.DrawOutlinedText(_topLaneText, new Point(_topLaneX - _topLaneText.Width / 2,
                                                    Config.Settings.CurrentTimePosition - 2 -
                                                    _topLaneText.Height / 2),
                            _laneNumberFillBrush, _textOutlinePen);
        dc.DrawOutlinedText(_centerLaneText, new Point(_centerLaneX - _centerLaneText.Width / 2,
                                                       Config.Settings.CurrentTimePosition - 2 -
                                                       _centerLaneText.Height / 2),
                            _laneNumberFillBrush, _textOutlinePen);
        dc.DrawOutlinedText(_bottomLaneText, new Point(_bottomLaneX - _bottomLaneText.Width / 2,
                                                       Config.Settings.CurrentTimePosition - 2 -
                                                       _bottomLaneText.Height / 2),
                            _laneNumberFillBrush, _textOutlinePen);
        dc.DrawOutlinedText(_cameraLaneText, new Point(_cameraLaneX - _cameraLaneText.Width / 2,
                                                       Config.Settings.CurrentTimePosition - 2 -
                                                       _cameraLaneText.Height / 2),
                            _laneNumberFillBrush, _textOutlinePen);

        if (ChartBuilder.BreakpointTime.SoftNotEquals(-1000))
        {
            RenderBreakpoint(dc);
        }
        
        foreach (var note in Chart.Notes)
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
            dc.DrawRectangle(ChartBuilder.RightMouseDrag ? _deleteDragOverlayBrush :
                                  _selectDragOverlayBrush, null,
                             new Rect(left, top, right - left, bottom - top));
        }
        
        dc.DrawRectangle(null, new Pen(_accentBrush, 5),
                         new Rect(0, 0, ViewerWidth, ViewerHeight));
        dc.DrawLine(new Pen(_accentBrush, 5), new Point(150, 0), new Point(150, ViewerHeight));
        
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
            var rangeStart = TimeToScreenCoords(bpmRegion.StartTime) - 75;
            var rangeEnd = TimeToScreenCoords(bpmRegion.StartTime) + 75;
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
            var rangeStart = TimeToScreenCoords(label.Time) - 75;
            var rangeEnd = TimeToScreenCoords(label.Time) + 75;
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

    private void RenderBpmChange(DrawingContext dc, BpmRegion bpmRegion)
    {
        var y = TimeToScreenCoords(bpmRegion.StartTime == Chart.Metadata.ChartOffset ? 0 :
                                       bpmRegion.StartTime);
        if (y < -50 || y > ViewerHeight + 50)
        {
            return;
        }
        
        var text = new FormattedText(bpmRegion.Bpm.ToString("0.00"), CultureInfo.CurrentCulture,
                                     FlowDirection.LeftToRight, _numberTypeface, 22,
                                     _bpmChangeBrush);
        
        dc.DrawText(text, new Point(135 - text.Width, y - 2 - text.Height / 2));
        dc.DrawLine(new Pen(_bpmChangeBrush, 5), new Point(150, y), new Point(ViewerWidth, y));
    }
    
    private void RenderLabel(DrawingContext dc, Chart.Label label)
    {
        var y = TimeToScreenCoords(label.Time - Chart.Metadata.ChartOffset);
        if (y < -50 || y > ViewerHeight + 50)
        {
            return;
        }

        var formattedText = new FormattedText(label.Text, CultureInfo.CurrentCulture,
                                              FlowDirection.LeftToRight, _numberTypeface, 22,
                                              _labelBrush)
        {
            MaxTextWidth = 120
        };
        
        dc.DrawText(formattedText,
                    new Point(135 - formattedText.Width, y - 2 - formattedText.Height / 2));
        dc.DrawLine(new Pen(_labelBrush, 5), new Point(150, y), new Point(ViewerWidth, y));
    }

    private void RenderBreakpoint(DrawingContext dc)
    {
        var y = TimeToScreenCoords(ChartBuilder.BreakpointTime);
        if (y < -50 || y > ViewerHeight + 50)
        {
            return;
        }
        
        _breakpointShape.Transform = new TranslateTransform(147, y);
        dc.DrawGeometry(_breakpointBrush, null, _breakpointShape);
        dc.DrawLine(new Pen(_breakpointBrush, 3), new Point(150, y), new Point(ViewerWidth, y));
    }

    private void RenderPlacingNotes(DrawingContext dc)
    {
        NoteBase? topNote = null, centerNote = null, bottomNote = null;
        // cop id doesn't change the center lane
        if (ChartBuilder.CenterLaneStartTime.SoftNotEquals(-1000))
        {
            if (ChartBuilder.CenterLaneStartTime.SoftEquals(Chart.CurrentTime))
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

        if (ChartBuilder.TopLaneStartTime.SoftNotEquals(-1000))
        {
            topNote = MakeNotePlaceholder(
                Math.Min(ChartBuilder.TopLaneStartTime, Chart.CurrentTime),
                Math.Max(ChartBuilder.TopLaneStartTime, Chart.CurrentTime));
            topNote.Lane = NoteLane.TOP;
        }
            
        if (ChartBuilder.BottomLaneStartTime.SoftNotEquals(-1000))
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

    private NoteBase MakeNotePlaceholder(double start, double end)
    {
        if (ChartBuilder.CopId != 0)
        {
            if (start.SoftEquals(end))
            {
                return new CopNote(NoteType.COP_SINGLE, ChartBuilder.CopId)
                {
                    Time = start
                };
            }
            
            return new CopNote(ChartBuilder.ShiftPressed ? NoteType.COP_MASH : NoteType.COP_HOLD,
                               ChartBuilder.CopId)
            {
                Time = start,
                EndTime = end
            };
        }
        
        if (start.SoftEquals(end))
        {
            return new SingleNote
            {
                Time = start,
                Flags = new NoteFlags(false, false, ChartBuilder.ShiftPressed)
            };
        }
        
        return new HoldNote
        {
            Time = start,
            EndTime = end,
            Flags = new NoteFlags(false, false, ChartBuilder.ShiftPressed)
        };
    }
}