using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using UNBUGGABLE.Resources;

namespace UNBUGGABLE.Views;

public enum CopState
{
    LEFT,
    RIGHT,
    DEAD
}

public class GamePreview : Control
{
    public static double PreviewWidth { get; private set; }
    public static double PreviewHeight { get; private set; }
    public static double TopLaneY { get; private set; }
    public static double NoteTargetX { get; private set; }
    public static double BottomLaneY { get; private set; }
    public static bool CurrentNotesFromRight { get; private set; } = true;
    
    public static CopState Cop1State = CopState.DEAD;
    public static CopState Cop2State = CopState.DEAD;
    public static CopState Cop3State = CopState.DEAD;
    public static CopState Cop4State = CopState.DEAD;
    
    public const int PixelsPerSecond = 650;

    private static readonly Geometry _directionIndicatorShape = new PolylineGeometry([
        new Point(22, -40),
        new Point(-22, 0),
        new Point(22, 40)
    ], true);

    private static SolidColorBrush _backgroundBrush;
    private static SolidColorBrush _outlineBrush;
    private static SolidColorBrush _copBrush;
    private static SolidColorBrush _viewableAreaOutlineBrush;
    private static SolidColorBrush _cameraArrowBrush;
    private static SolidColorBrush _noteTargetLineBrush;
    private static SolidColorBrush _noteTargetCircleFillBrush;
    private static SolidColorBrush _noteTargetCircleOutlineBrush;

    private static double _cornerRadius;
    private static double _outlineThickness;
    private static double _viewableAreaOutlineThickness;
    private static double _cameraArrowScale;
    private static double _noteTargetLineThickness;
    private static double _noteTargetCircleOutlineThickness;
    private static double _noteTargetCircleRadius;

    public static void UpdateStyles()
    {
        _backgroundBrush = (SolidColorBrush)App.Current.Resources["GamePreview.BackgroundColor"];
        _outlineBrush = (SolidColorBrush)App.Current.Resources["GamePreview.OutlineColor"];
        _copBrush = (SolidColorBrush)App.Current.Resources["GamePreview.CopColor"];
        _viewableAreaOutlineBrush =
            (SolidColorBrush)App.Current.Resources["GamePreview.ViewableArea.OutlineColor"];
        _cameraArrowBrush = (SolidColorBrush)App.Current.Resources["GamePreview.CameraArrowColor"];
        _noteTargetLineBrush =
            (SolidColorBrush)App.Current.Resources["GamePreview.NoteTargets.LineColor"];
        _noteTargetCircleFillBrush =
            (SolidColorBrush)App.Current.Resources["GamePreview.NoteTargets.Circles.FillColor"];
        _noteTargetCircleOutlineBrush =
            (SolidColorBrush)App.Current.Resources["GamePreview.NoteTargets.Circles.OutlineColor"];
        
        _cornerRadius = ((CornerRadius)App.Current.Resources["GamePreview.CornerRadius"]).TopLeft;
        _outlineThickness = ((Thickness)App.Current.Resources["GamePreview.OutlineThickness"]).Top;
        _viewableAreaOutlineThickness =
            ((Thickness)App.Current.Resources["GamePreview.ViewableArea.OutlineThickness"]).Top;
        _noteTargetLineThickness =
            (double)App.Current.Resources["GamePreview.NoteTargets.LineThickness"];
        _noteTargetCircleOutlineThickness = ((Thickness)
            App.Current.Resources["GamePreview.NoteTargets.Circles.OutlineThickness"]).Top;
        _noteTargetCircleRadius =
            (double)App.Current.Resources["GamePreview.NoteTargets.Circles.Radius"];
        _cameraArrowScale = (double)App.Current.Resources["GamePreview.CameraArrowScale"];
    }
    
    /// <summary>
    /// Given a time in milliseconds, returns the x coordinate of that time.
    /// </summary>
    public static double TimeToScreenCoords(double time)
    {
        var x = ((time - Chart.CurrentTimeRaw) / 1000) * PixelsPerSecond + NoteTargetX;
        return (CurrentNotesFromRight ? x : -x);
    }
    
    public override void Render(DrawingContext dc)
    {
        PreviewWidth = Bounds.Size.Width;
        PreviewHeight = Bounds.Size.Height;
        NoteTargetX = 80;
        TopLaneY = -PreviewHeight / 2 + 75;
        BottomLaneY = PreviewHeight / 2 - 75;
        
        var clip = dc.PushClip(
            new RoundedRect(new Rect(0, 0, Bounds.Size.Width, Bounds.Size.Height), _cornerRadius));
        
        var positionOffset = dc.PushTransform(new Matrix(1, 0, 0, 1, PreviewWidth / 2,
                                                         PreviewHeight / 2));
        dc.DrawRectangle(_backgroundBrush, null, new Rect(0, 0, PreviewWidth, PreviewHeight));
        
        var linePen = new Pen(_noteTargetLineBrush, _noteTargetLineThickness);
        var circlePen = new Pen(_noteTargetCircleOutlineBrush, _noteTargetCircleOutlineThickness);
        
        // left line/note targets
        dc.DrawLine(linePen, new Point(-NoteTargetX, -PreviewHeight / 2),
                    new Point(-NoteTargetX, PreviewHeight / 2));
        dc.DrawEllipse(_noteTargetCircleFillBrush, circlePen, new Point(-NoteTargetX, TopLaneY),
                       _noteTargetCircleRadius, _noteTargetCircleRadius);
        dc.DrawEllipse(_noteTargetCircleFillBrush, circlePen, new Point(-NoteTargetX, BottomLaneY),
                       _noteTargetCircleRadius, _noteTargetCircleRadius);
        
        // right line/note targets
        dc.DrawLine(linePen, new Point(NoteTargetX, -PreviewHeight / 2),
                    new Point(NoteTargetX, PreviewHeight / 2));
        dc.DrawEllipse(_noteTargetCircleFillBrush, circlePen, new Point(NoteTargetX, TopLaneY),
                       _noteTargetCircleRadius, _noteTargetCircleRadius);
        dc.DrawEllipse(_noteTargetCircleFillBrush, circlePen, new Point(NoteTargetX, BottomLaneY),
                       _noteTargetCircleRadius, _noteTargetCircleRadius);
        
        // get cop states because the cop sprite gets rendered below notes
        Cop1State = CopState.DEAD;
        Cop2State = CopState.DEAD;
        Cop3State = CopState.DEAD;
        Cop4State = CopState.DEAD;
        var fromRight = true;
        foreach (var note in Chart.Notes)
        {
            if (note.Time > Chart.CurrentTimeRaw + 1000)
            {
                break;
            }
            if (note.Type is NoteType.CAMERA_SWAP or NoteType.CAMERA_INSTANT or
                NoteType.CAMERA_SWAP_AND_ZOOM)
            {
                fromRight = !fromRight;
            }

            if (note is CopNote copNote)
            {

                var finishTime = (note.Type == NoteType.COP_SINGLE ? note.Time : note.EndTime);
                switch (copNote.CopId)
                {
                    case 1:
                        if (copNote.IsFinisher)
                        {
                            if (finishTime <= Chart.CurrentTimeRaw)
                            {
                                Cop1State = CopState.DEAD;
                            }
                        }
                        else
                        {
                            Cop1State = (fromRight ? CopState.RIGHT : CopState.LEFT);
                        }
                        break;
                    case 2:
                        if (copNote.IsFinisher)
                        {
                            if (finishTime <= Chart.CurrentTimeRaw)
                            {
                                Cop2State = CopState.DEAD;
                            }
                        }
                        else
                        {
                            Cop2State = (fromRight ? CopState.RIGHT : CopState.LEFT);
                        }
                        break;
                    case 3:
                        if (copNote.IsFinisher)
                        {
                            if (finishTime <= Chart.CurrentTimeRaw)
                            {
                                Cop3State = CopState.DEAD;
                            }
                        }
                        else
                        {
                            Cop3State = (fromRight ? CopState.RIGHT : CopState.LEFT);
                        }
                        break;
                    case 4:
                        if (copNote.IsFinisher)
                        {
                            if (finishTime <= Chart.CurrentTimeRaw)
                            {
                                Cop4State = CopState.DEAD;
                            }
                        }
                        else
                        {
                            Cop4State = (fromRight ? CopState.RIGHT : CopState.LEFT);
                        }
                        break;
                }
            }
        }
        
        List<string> leftCopStates = [], rightCopStates = [];
        if (Cop1State != CopState.DEAD)
        {
            if (Cop1State == CopState.LEFT)
            {
                leftCopStates.Add("1");
            }
            else
            {
                rightCopStates.Add("1");
            }
        }
        if (Cop2State != CopState.DEAD)
        {
            if (Cop2State == CopState.LEFT)
            {
                leftCopStates.Add("2");
            }
            else
            {
                rightCopStates.Add("2");
            }
        }
        if (Cop3State != CopState.DEAD)
        {
            if (Cop3State == CopState.LEFT)
            {
                leftCopStates.Add("3");
            }
            else
            {
                rightCopStates.Add("3");
            }
        }
        if (Cop4State != CopState.DEAD)
        {
            if (Cop4State == CopState.LEFT)
            {
                leftCopStates.Add("4");
            }
            else
            {
                rightCopStates.Add("4");
            }
        }
        
        if (Cop1State == CopState.LEFT || Cop2State == CopState.LEFT ||
            Cop3State == CopState.LEFT || Cop4State == CopState.LEFT)
        {
            var rect = new RoundedRect(
                new Rect(-NoteTargetX - 120, TopLaneY + 20, 60, -TopLaneY + BottomLaneY - 40), 15);
            dc.DrawRectangle(Brushes.Transparent, new Pen(_copBrush, 5), rect);
            dc.DrawEllipse(_copBrush, null, new Point(-NoteTargetX - 78, TopLaneY + 45), 6, 6);
            dc.DrawEllipse(_copBrush, null, new Point(-NoteTargetX - 102, TopLaneY + 45), 6, 6);
            dc.DrawArc(null, new Pen(_copBrush, 5), new Point(-NoteTargetX - 90, TopLaneY + 55),
                       20, 30, 20, 160);
        }
        if (Cop1State == CopState.RIGHT || Cop2State == CopState.RIGHT ||
            Cop3State == CopState.RIGHT || Cop4State == CopState.RIGHT)
        {
            var rect = new RoundedRect(
                new Rect(NoteTargetX + 60, TopLaneY + 20, 60, -TopLaneY + BottomLaneY - 40), 15);
            dc.DrawRectangle(Brushes.Transparent, new Pen(_copBrush, 5), rect);
            dc.DrawEllipse(_copBrush, null, new Point(NoteTargetX + 78, TopLaneY + 45), 6, 6);
            dc.DrawEllipse(_copBrush, null, new Point(NoteTargetX + 102, TopLaneY + 45), 6, 6);
            dc.DrawArc(null, new Pen(_copBrush, 5), new Point(NoteTargetX + 90, TopLaneY + 55),
                       20, 30, 20, 160);
        }
        
        // render notes
        var viewableNotesFromRight = true;
        CurrentNotesFromRight = true;
        var viewableZoomedOut = false;
        var currentNoteZoomedOut = false;
        
        // render freestyles that have been "caught" -- originally this was rendered as part of the
        // freestyle note class, but doing it here avoids issues when notes are really far apart
        var previousNote = Chart.GetLastNoteBeforeTime(Chart.CurrentTime);
        var nextNote = Chart.GetFirstNoteAfterTime(Chart.CurrentTime);

        if (previousNote?.Type == NoteType.FREESTYLE && nextNote?.Type == NoteType.FREESTYLE)
        {
            dc.DrawEllipse(FreestyleNote.FillBrush, new Pen(FreestyleNote.OutlineBrush, 6),
                           new Point(TimeToScreenCoords(Chart.CurrentTimeRaw), 0), 30, 30);
        }
        
        foreach (var note in Chart.Notes)
        {
            if (note.Type is NoteType.CAMERA_ZOOM or NoteType.CAMERA_SWAP_AND_ZOOM)
            {
                currentNoteZoomedOut = !currentNoteZoomedOut;
                if (note.Time < Chart.CurrentTimeRaw)
                {
                    viewableZoomedOut = currentNoteZoomedOut;
                }
            }

            if (note.Type is NoteType.CAMERA_SWAP or NoteType.CAMERA_INSTANT or
                NoteType.CAMERA_SWAP_AND_ZOOM)
            {
                CurrentNotesFromRight = !CurrentNotesFromRight;
                if (note.Time < Chart.CurrentTimeRaw)
                {
                    viewableNotesFromRight = CurrentNotesFromRight;
                }
            }
            
            // camera notes have an empty RenderPreview() method
            note.RenderPreview(dc);
        }

        double viewableX, viewableY, viewableWidth, viewableHeight;
        if (viewableZoomedOut)
        {
            viewableX = -PreviewWidth / 2 + 100;
            viewableY = -PreviewHeight / 2 + 20;
            viewableWidth = PreviewWidth - 200;
            viewableHeight = PreviewHeight - 40;
        }
        else if (viewableNotesFromRight)
        {
            viewableX = 30;
            viewableY = -PreviewHeight / 2 + 30;
            viewableWidth = PreviewWidth / 2 - 60;
            viewableHeight = PreviewHeight - 60;
        }
        else
        {
            viewableX = -PreviewWidth / 2 + 30;
            viewableY = -PreviewHeight / 2 + 30;
            viewableWidth = PreviewWidth / 2 - 60;
            viewableHeight = PreviewHeight - 60;
        }
        
        dc.DrawRectangle(null, new Pen(_viewableAreaOutlineBrush, _viewableAreaOutlineThickness),
                         new Rect(viewableX, viewableY, viewableWidth, viewableHeight));

        if (viewableZoomedOut && Config.Settings.EnhancedPreview)
        {
            var shape = _directionIndicatorShape.Clone();
            var transform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(_cameraArrowScale, _cameraArrowScale)
                }
            };
            if (viewableNotesFromRight)
            {
                transform.Children.Add(new RotateTransform(180));
            }
            shape.Transform = transform;
            
            dc.DrawGeometry(_cameraArrowBrush, null, shape);
        }
        
        positionOffset.Dispose();
        // dc.DrawRectangle(null, new Pen(_outlineBrush, _outlineThickness),
        //                  new RoundedRect(new Rect(0, 0, PreviewWidth, PreviewHeight),
        //                                  _cornerRadius));
        
        clip.Dispose();
    }
}