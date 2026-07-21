using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

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
    
    public static string LeftCopText { get; private set; } = "Left:";
    public static string RightCopText { get; private set; } = "Right:";
    
    private readonly SolidColorBrush _viewableAreaBrush =
        (SolidColorBrush)App.Current.Resources["ViewableArea"];
    private readonly SolidColorBrush _accentBrush =
        (SolidColorBrush)App.Current.Resources["Accent"];
    private readonly SolidColorBrush _subBeatLineBrush =
        (SolidColorBrush)App.Current.Resources["SubBeatSnapLine"];
    private readonly SolidColorBrush _editorBackgroundBrush =
        (SolidColorBrush)App.Current.Resources["EditorBackground"];
    
    private const int PixelsPerSecond = 650;
    
    /// <summary>
    /// Given a time in milliseconds, returns the x coordinate of that time.
    /// </summary>
    public static double TimeToScreenCoords(double time)
    {
        var x = ((time - Chart.CurrentTime) / 1000) * PixelsPerSecond + NoteTargetX;
        return (CurrentNotesFromRight ? x : -x);
    }
    
    public override void Render(DrawingContext dc)
    {
        PreviewWidth = Bounds.Size.Width;
        PreviewHeight = Bounds.Size.Height;
        NoteTargetX = 80;
        TopLaneY = -PreviewHeight / 2 + 75;
        BottomLaneY = PreviewHeight / 2 - 75;
        
        var clip = dc.PushClip(new Rect(0, 0, Bounds.Size.Width, Bounds.Size.Height));
        
        var positionOffset = dc.PushTransform(new Matrix(1, 0, 0, 1, PreviewWidth / 2,
                                                         PreviewHeight / 2));
        dc.DrawRectangle(_editorBackgroundBrush, null, new Rect(0, 0, PreviewWidth, PreviewHeight));
        
        // left line/note targets
        dc.DrawLine(new Pen(_subBeatLineBrush, 2), new Point(-NoteTargetX, -PreviewHeight / 2),
                    new Point(-NoteTargetX, PreviewHeight / 2));
        dc.DrawEllipse(null, new Pen(_accentBrush, 5), new Point(-NoteTargetX, TopLaneY), 30, 30);
        dc.DrawEllipse(null, new Pen(_accentBrush, 5), new Point(-NoteTargetX, BottomLaneY), 30,
                       30);
        
        // right line/note targets
        dc.DrawLine(new Pen(_subBeatLineBrush, 2), new Point(NoteTargetX, -PreviewHeight / 2),
                    new Point(NoteTargetX, PreviewHeight / 2));
        dc.DrawEllipse(null, new Pen(_accentBrush, 5), new Point(NoteTargetX, TopLaneY), 30, 30);
        dc.DrawEllipse(null, new Pen(_accentBrush, 5), new Point(NoteTargetX, BottomLaneY), 30, 30);
        
        // get cop states because the cop sprite gets rendered below notes
        Cop1State = CopState.DEAD;
        Cop2State = CopState.DEAD;
        Cop3State = CopState.DEAD;
        Cop4State = CopState.DEAD;
        var fromRight = true;
        foreach (var note in Chart.Notes)
        {
            if (note.Time > Chart.CurrentTime + 1000)
            {
                break;
            }
            if (note.Type is NoteType.CAMERA_SWAP or NoteType.CAMERA_INSTANT)
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
                            if (finishTime <= Chart.CurrentTime)
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
                            if (finishTime <= Chart.CurrentTime)
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
                            if (finishTime <= Chart.CurrentTime)
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
                            if (finishTime <= Chart.CurrentTime)
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

        LeftCopText = $"Left: {string.Join(", ", leftCopStates)}";
        RightCopText = $"Right: {string.Join(", ", rightCopStates)}";
        
        if (Cop1State == CopState.LEFT || Cop2State == CopState.LEFT ||
            Cop3State == CopState.LEFT || Cop4State == CopState.LEFT)
        {
            var rect = new RoundedRect(
                new Rect(-NoteTargetX - 120, TopLaneY + 20, 60, -TopLaneY + BottomLaneY - 40), 15);
            dc.DrawRectangle(_editorBackgroundBrush, new Pen(_accentBrush, 5), rect);
            dc.DrawEllipse(_accentBrush, null, new Point(-NoteTargetX - 78, TopLaneY + 45), 6, 6);
            dc.DrawEllipse(_accentBrush, null, new Point(-NoteTargetX - 102, TopLaneY + 45), 6, 6);
            dc.DrawArc(null, new Pen(_accentBrush, 5), new Point(-NoteTargetX - 90, TopLaneY + 55),
                       20, 30, 20, 160);
        }
        if (Cop1State == CopState.RIGHT || Cop2State == CopState.RIGHT ||
            Cop3State == CopState.RIGHT || Cop4State == CopState.RIGHT)
        {
            var rect = new RoundedRect(
                new Rect(NoteTargetX + 60, TopLaneY + 20, 60, -TopLaneY + BottomLaneY - 40), 15);
            dc.DrawRectangle(_editorBackgroundBrush, new Pen(_accentBrush, 5), rect);
            dc.DrawEllipse(_accentBrush, null, new Point(NoteTargetX + 78, TopLaneY + 45), 6, 6);
            dc.DrawEllipse(_accentBrush, null, new Point(NoteTargetX + 102, TopLaneY + 45), 6, 6);
            dc.DrawArc(null, new Pen(_accentBrush, 5), new Point(NoteTargetX + 90, TopLaneY + 55),
                       20, 30, 20, 160);
        }
        
        // render notes
        var viewableNotesFromRight = true;
        CurrentNotesFromRight = true;
        var viewableZoomedOut = false;
        var currentNoteZoomedOut = false;
        foreach (var note in Chart.Notes)
        {
            if (note.Type == NoteType.CAMERA_WIDE)
            {
                currentNoteZoomedOut = !currentNoteZoomedOut;
                if (note.Time < Chart.CurrentTime)
                {
                    viewableZoomedOut = currentNoteZoomedOut;
                }
            }
            else if (note.Type is NoteType.CAMERA_SWAP or NoteType.CAMERA_INSTANT)
            {
                CurrentNotesFromRight = !CurrentNotesFromRight;
                if (note.Time < Chart.CurrentTime)
                {
                    viewableNotesFromRight = CurrentNotesFromRight;
                }
            }
            else
            {
                double clipX, clipY, clipWidth, currentHeight;
                if (currentNoteZoomedOut)
                {
                    clipX = -PreviewWidth / 2;
                    clipY = -PreviewHeight / 2 + 20;
                    clipWidth = PreviewWidth;
                    currentHeight = PreviewHeight - 40;
                }
                else if (CurrentNotesFromRight)
                {
                    clipX = 30;
                    clipY = -PreviewHeight / 2 + 30;
                    clipWidth = PreviewWidth / 2 - 30;
                    currentHeight = PreviewHeight - 60;
                }
                else
                {
                    clipX = -PreviewWidth / 2;
                    clipY = -PreviewHeight / 2 + 30;
                    clipWidth = PreviewWidth / 2 - 30;
                    currentHeight = PreviewHeight - 60;
                }
                
                // var cameraClip = dc.PushClip(new Rect(clipX, clipY, clipWidth, currentHeight));
                note.RenderPreview(dc);
                // cameraClip.Dispose();
            }
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
        
        dc.DrawRectangle(null, new Pen(_viewableAreaBrush, 5),
                         new Rect(viewableX, viewableY, viewableWidth, viewableHeight));
        
        positionOffset.Dispose();
        dc.DrawRectangle(null, new Pen(_accentBrush, 5),
                         new Rect(0, 0, PreviewWidth, PreviewHeight));
        
        clip.Dispose();
    }
}