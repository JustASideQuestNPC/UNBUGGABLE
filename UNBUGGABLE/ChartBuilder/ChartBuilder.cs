using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Dialogs;
using UNBUGGABLE.Commands;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public static class ChartBuilder
{
    public static Point MousePosition { get; set; } = new(-1000, -1000);
    public static Point? MouseDragStart { get; private set; } = null;
    public static double MouseDragStartTime { get; private set; } = -1000;
    public static bool RightMouseDrag { get; private set; } = false;
    
    public static double TopLaneStartTime { get; private set; } = -1000;
    public static double BottomLaneStartTime { get; private set; } = -1000;
    public static double CenterLaneStartTime { get; private set; } = -1000;
    
    public static List<NoteBase> SelectedNotes = new();
    
    public static double BreakpointTime { get; private set; } = -1000;
    
    // 0 for normal notes, 1-4 for cop notes
    public static int CopId { get; private set; } = 0;
    
    // for handling modifiers and key binds that behave differently if the key is held down
    private static bool _leftCtrlPressed = false;
    private static bool _rightCtrlPressed = false;
    public static bool CtrlPressed => _leftCtrlPressed || _rightCtrlPressed;
    
    private static bool _leftShiftPressed = false;
    private static bool _rightShiftPressed = false;
    public static bool ShiftPressed => _leftShiftPressed || _rightShiftPressed;
    
    private static List<NoteBase> _clipboard = new();
    
    private static readonly List<string> NoteTypeNames = [
        "notes", "cop 1", "cop 2", "cop 3", "cop 4"];
    
    public static async Task OnKeyDown(Key k)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }

        switch (k)
        {
            // modifier keys
            case Key.LeftCtrl:
                _leftCtrlPressed = true;
                break;
            case Key.RightCtrl:
                _rightCtrlPressed = true;
                break;
            case Key.LeftShift:
                _leftShiftPressed = true;
                break;
            case Key.RightShift:
                _rightShiftPressed = true;
                break;

            case Key.D2:
                if (CtrlPressed)
                {
                    if (SelectedNotes.Count > 0)
                    {
                        CommandInvoker.Execute(new SetNotesCopIdCommand(SelectedNotes, 2));
                    }
                    else
                    {
                        CopId = 2;
                        App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[CopId];
                    }
                }
                break;

            // 3 and 4 are also used to change note cop IDs (which is a non-hold keybind)
            case Key.D3:
                if (CtrlPressed)
                {
                    if (SelectedNotes.Count > 0)
                    {
                        CommandInvoker.Execute(new SetNotesCopIdCommand(SelectedNotes, 3));
                    }
                    else
                    {
                        CopId = 3;
                        App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[CopId];
                    }
                }
                else if (TopLaneStartTime.SoftEquals(-1000))
                {
                    Console.WriteLine($"start top lane placement: {Chart.CurrentTime}");
                    TopLaneStartTime = Chart.CurrentTime;
                }

                break;
            case Key.D4:
                if (CtrlPressed)
                {
                    if (SelectedNotes.Count > 0)
                    {
                        CommandInvoker.Execute(new SetNotesCopIdCommand(SelectedNotes, 4));
                    }
                    else
                    {
                        CopId = 4;
                        App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[CopId];
                    }
                }
                else if (BottomLaneStartTime.SoftEquals(-1000))
                {
                    Console.WriteLine($"start bottom lane placement: {Chart.CurrentTime}");
                    BottomLaneStartTime = Chart.CurrentTime;
                }

                break;

            case Key.D6:
                if (CenterLaneStartTime.SoftEquals(-1000))
                {
                    Console.WriteLine($"start center lane placement: {Chart.CurrentTime}");
                    CenterLaneStartTime = Chart.CurrentTime;
                }

                break;

            // non-hold keybinds
            // case Key.D0:
            case Key.OemTilde: // not an official editor keybind
            case Key.D0:
                if (CtrlPressed)
                {
                    if (SelectedNotes.Count > 0)
                    {
                        CommandInvoker.Execute(new SetNotesCopIdCommand(SelectedNotes, 0));
                    }
                    else
                    {
                        CopId = 0;
                        App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[CopId];
                    }
                }

                break;
            case Key.D1:
                if (CtrlPressed)
                {
                    if (SelectedNotes.Count > 0)
                    {
                        CommandInvoker.Execute(new SetNotesCopIdCommand(SelectedNotes, 1));
                    }
                    else
                    {
                        CopId = 1;
                        App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[CopId];
                    }
                }

                break;
            case Key.D5:
                DoCameraSwapOperation();
                break;
            case Key.M:
                if (CtrlPressed && SelectedNotes.Count > 0)
                {
                    CommandInvoker.Execute(new MirrorNotesCommand(SelectedNotes));
                }

                break;
            case Key.Delete:
                if (SelectedNotes.Count > 0)
                {
                    CommandInvoker.Execute(new DeleteNotesCommand(SelectedNotes));
                    SelectedNotes.Clear();
                }

                break;
            case Key.Escape:
                ClearSelection();
                break;
            case Key.L:
                await DoLabelOperation();
                break;
            case Key.Q:
                DoMarkerOperation();
                break;
            case Key.B:
                if (ShiftPressed)
                {
                    DeleteBreakpoint();
                }
                else
                {
                    SetBreakpoint();
                }

                break;
            case Key.F9:
                await DoBpmChangeOperation();
                break;
            // case Key.Up:
            //     if (SelectedNotes.Count > 0 && ShiftPressed)
            //     {
            //         Console.WriteLine("move selected notes up");
            //     }
            //     break;
            // case Key.Down:
            //     if (SelectedNotes.Count > 0 && ShiftPressed)
            //     {
            //         Console.WriteLine("move selected notes down");
            //     }
            //     break;
            case Key.Left:
                Chart.DecreaseBeatSnap();
                break;
            case Key.Right:
                Chart.IncreaseBeatSnap();
                break;
            case Key.PageUp:
                Chart.MoveToPreviousLabel();
                break;
            case Key.PageDown:
                Chart.MoveToNextLabel();
                break;
            case Key.OemPipe:
            case Key.OemComma:
                --CopId;
                if (CopId < 0)
                {
                    CopId = 4;
                }
                App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[CopId];
                break;
            case Key.OemQuestion:
            case Key.OemPeriod:
                ++CopId;
                if (CopId > 4)
                {
                    CopId = 0;
                }
                App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[CopId];
                break;
            case Key.E:
            case Key.F:
                DoSetFlagOperation('f');
                break;
            case Key.W:
                DoSetFlagOperation('w');
                break;
            case Key.C:
            case Key.R:
                DoSetFlagOperation('c');
                break;
            case Key.Space:
                Chart.PlayOrPauseSong();
                break;
        }
    }

    public static void OnKeyUp(Key k)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        switch (k)
        {
            case Key.LeftCtrl:
                _leftCtrlPressed = false;
                break;
            case Key.RightCtrl:
                _rightCtrlPressed = false;
                break;
            case Key.LeftShift:
                _leftShiftPressed = false;
                break;
            case Key.RightShift:
                _rightShiftPressed = false;
                break;
            case Key.D3:
                if (TopLaneStartTime.SoftNotEquals(-1000))
                {
                    Console.WriteLine("end top lane placement");
                    var start = Math.Min(TopLaneStartTime, Chart.CurrentTime);
                    var end = Math.Max(TopLaneStartTime, Chart.CurrentTime);
                    CheckForNoteOperation(NoteLane.TOP, start, end);
                    TopLaneStartTime = -1000;
                }
                break;
            case Key.D4:
                if (BottomLaneStartTime.SoftNotEquals(-1000))
                {
                    Console.WriteLine("end bottom lane placement");
                    var start = Math.Min(BottomLaneStartTime, Chart.CurrentTime);
                    var end = Math.Max(BottomLaneStartTime, Chart.CurrentTime);
                    CheckForNoteOperation(NoteLane.BOTTOM, start, end);
                    BottomLaneStartTime = -1000;
                }
                break;
            case Key.D6:
                if (CenterLaneStartTime.SoftNotEquals(-1000))
                {
                    Console.WriteLine("end center lane placement");
                    var start = Math.Min(CenterLaneStartTime, Chart.CurrentTime);
                    var end = Math.Max(CenterLaneStartTime, Chart.CurrentTime);
                    CheckForNoteOperation(NoteLane.CENTER, start, end);
                    CenterLaneStartTime = -1000;
                }
                break;
        }
    }

    public static void OnScroll(double scrollAmount)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        if (scrollAmount > 0)
        {
            if (CtrlPressed)
            {
                NoteViewer.IncreaseZoom();
            }
            else if (!Chart.Playing)
            {
                Chart.MoveToPreviousSnap();
            }
        }
        else
        {
            if (CtrlPressed)
            {
                NoteViewer.DecreaseZoom();
            }
            else if (!Chart.Playing)
            {
                Chart.MoveToNextSnap();
            }
        }
    }

    public static void OnMousePress(bool rightButton)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }

        if (MouseDragStart == null)
        {
            RightMouseDrag = rightButton;
            MouseDragStart = new Point(MousePosition.X, MousePosition.Y);
            MouseDragStartTime = NoteViewer.ScreenCoordsToTime(MouseDragStart.Value.Y);
        }
        
        // Chart.PlayHitSound();
    }

    public static void OnMouseRelease()
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        var selectedLanes = NoteViewer.GetSelectedLanes();
        var dragEndTime = NoteViewer.ScreenCoordsToTime(MousePosition.Y);
        var notes = Chart.GetNoteRegion(Math.Min(MouseDragStartTime, dragEndTime),
                                        Math.Max(MouseDragStartTime, dragEndTime), selectedLanes);

        var hoveredNote = Chart.Notes.FirstOrDefault(n => n.MouseOver());
        if (hoveredNote != null && !notes.Contains(hoveredNote))
        {
            notes.Add(hoveredNote);
        }
        
        if (RightMouseDrag)
        {
            CommandInvoker.Execute(new DeleteNotesCommand(notes));
        }
        else
        {
            SelectedNotes = notes;
            Console.WriteLine($"Selected {SelectedNotes.Count} notes");
        }
        
        RightMouseDrag = false;
        MouseDragStart = null;
        MouseDragStartTime = -1000;
    }

    public static async Task<bool> TryCreateChartFromAudio(string path)
    {
        var result = await Chart.TryCreateChartFromAudio(path);
        if (result)
        {
            CommandInvoker.Reset();
        }

        return result;
    }
    
    public static async Task<bool> TryLoadChartFile(string path)
    {
        var result = await Chart.TryLoadChartFile(path);
        if (result)
        {
            CommandInvoker.Reset();
        }

        return result;
    }

    public static async void TryAutoLoadChartFile()
    {
        // command line arguments are used for file association
        Console.WriteLine(Environment.CommandLine);
        var i = Environment.CommandLine.IndexOf(' ');
        if (i != -1)
        {
            await TryLoadChartFile(Environment.CommandLine[(i + 1)..]);
        }
        else if (UserData.LastOpenedChartFile != "")
        {
            await TryLoadChartFile(UserData.LastOpenedChartFile);
        }
    }

    public static async Task SaveToBeatPath(string path)
    {
        await Chart.SaveToBeatPath(path);
    }
    
    public static async Task SaveToStandardPath(string path)
    {
        await Chart.SaveToStandardPath(path);
    }

    public static void Undo()
    {
        CommandInvoker.Undo();
    }
    
    public static void Redo()
    {
        CommandInvoker.Redo();
    }
    
    public static void SelectAll()
    {
        SelectedNotes = Chart.Notes.ToList();
    }
    
    public static void Cut()
    {
        _clipboard = new List<NoteBase>([..SelectedNotes]);
        CommandInvoker.Execute(new DeleteNotesCommand([..SelectedNotes]));
    }
    
    public static void Copy()
    {
        _clipboard = new List<NoteBase>([..SelectedNotes]);
    }
    
    public static void Paste()
    {
        if (_clipboard.Count == 0)
        {
            return;
        }
        
        var timeOffset = Chart.CurrentTime - _clipboard[0].Time;
        
        List<NoteBase> newNotes = [];
        foreach (var note in _clipboard)
        {
            newNotes.Add(note.Clone(note.Time + timeOffset));
        }
        
        CommandInvoker.Execute(new AddNotesCommand(newNotes, true));
    }
    
    public static void ClearSelection()
    {
        SelectedNotes.Clear();
    }

    public static void CheckExistingBreakpoint()
    {
        if (!Config.EnableBreakpoints || !Config.PracticeModInstalled)
        {
            return;
        }
        
        var lines = File.ReadAllLines(Config.PracticeModConfigPath).ToList();
        var index = lines.FindIndex(
            l => l.StartsWith($"{Chart.Metadata.SongName.ToLowerInvariant()}:"));
        if (index != -1 && double.TryParse(lines[index].Split(':')[1], out var time))
        {
            BreakpointTime = time;
            App.MainWindowViewModel.BreakpointTimeText = TimeSpan.FromMilliseconds(BreakpointTime)
                                                                 .ToString(@"mm\:ss\.fff");
            Console.WriteLine($"Found existing breakpoint at {BreakpointTime}");
        }
    }

    private static async Task DoLabelOperation()
    {
        var time = Chart.CurrentTime;
        var existingLabel = Chart.GetLabel(time);
        if (CtrlPressed)
        {
            if (existingLabel != null)
            {
                CommandInvoker.Execute(new RemoveLabelCommand(existingLabel));
            }
        }
        else
        {
            if (existingLabel != null)
            {
                var text = await new TextEntryDialog("edit label", existingLabel.Text).ShowAsync();
                if (text.HasValue && text.Value != existingLabel.Text)
                {
                    CommandInvoker.Execute(new EditLabelCommand(existingLabel, text.Value));
                }
            }
            else
            {
                var text = await new TextEntryDialog("add label").ShowAsync();
                if (text.HasValue && text.Value != "")
                {
                    CommandInvoker.Execute(new AddLabelCommand(time + Chart.Metadata.ChartOffset,
                                                               text.Value));
                }
            }
        }
    }

    private static async Task DoBpmChangeOperation()
    {
        var time = Chart.CurrentTime;
        var existingRegion = Chart.GetBpmRegion(time);
        if (CtrlPressed)
        {
            // the first bpm region can't be removed for obvious reasons
            if (existingRegion != null && existingRegion != Chart.BpmRegions[0])
            {
                Console.WriteLine("Remove bpm region");
                CommandInvoker.Execute(new RemoveBpmRegionCommand(existingRegion));
            }
        }
        else
        {
            if (existingRegion != null)
            {
                var bpm = await new NumberEntryDialog("edit bpm change",
                                                      existingRegion.Bpm).ShowAsync();
                if (bpm.HasValue && bpm.Value.SoftNotEquals(existingRegion.Bpm))
                {
                    if (existingRegion.Previous != null)
                    {
                        Console.WriteLine($"{bpm.Value}, {existingRegion.Previous.Bpm}");
                    }
                    
                    // setting a region's bpm to the same as the previous region merges them
                    if (existingRegion.Previous != null &&
                        bpm.Value.SoftEquals(existingRegion.Previous.Bpm))
                    {
                        Console.WriteLine("Merge bpm regions");
                        CommandInvoker.Execute(new RemoveBpmRegionCommand(existingRegion));
                    }
                    else
                    {
                        Console.WriteLine("Edit bpm region");
                        CommandInvoker.Execute(new EditBpmRegionCommand(existingRegion, bpm.Value));
                    }
                }
            }
            else
            {
                var bpm = await new NumberEntryDialog("add bpm change").ShowAsync();
                if (bpm.HasValue)
                {
                    Console.WriteLine("Add bpm region");
                    CommandInvoker.Execute(new AddBpmRegionCommand(time, bpm.Value));
                }
            }
        }
    }

    private static void CheckForNoteOperation(NoteLane lane, double start, double end)
    {
        var oldNote = Chart.GetNote(start, lane);
        Console.WriteLine(oldNote);
        // hold notes can also extend from the start of the note
        if (oldNote == null && end.SoftNotEquals(start))
        {
            oldNote = Chart.GetNote(end, lane) ?? Chart.GetNoteFromEnd(start, lane);
        }
        Console.WriteLine(oldNote);
        
        if (oldNote != null)
        {
            if (start.SoftEquals(end))
            {
                CommandInvoker.Execute(new DeleteNotesCommand([oldNote]));
                return;
            }
        }

        NoteBase newNote;
        switch (lane)
        {
            case NoteLane.TOP:
            case NoteLane.BOTTOM:
            {
                if (CopId == 0)
                {
                    if (start.SoftEquals(end))
                    {
                        newNote = new SingleNote
                        {
                            Lane = lane,
                            Time = start,
                            Flags = new NoteFlags(false, false, ShiftPressed)
                        };
                    }
                    else
                    {
                        newNote = new HoldNote
                        {
                            Lane = lane,
                            Time = start,
                            EndTime = end,
                            Flags = new NoteFlags(false, false, ShiftPressed)
                        };
                    }
                }
                else
                {
                    if (start.SoftNotEquals(end) && ShiftPressed && lane == NoteLane.TOP &&
                        !Config.AllowTopLaneCopMashes)
                    {
                        App.MainWindowViewModel.ShowEventIndicator(
                            "Top lane cop mashes do not appear in-game.");
                        return;
                    }
                    
                    newNote = new CopNote((start.SoftEquals(end) ? NoteType.COP_SINGLE :
                            ShiftPressed ? NoteType.COP_MASH : NoteType.COP_HOLD), CopId)
                    {
                        Time = start,
                        EndTime = end,
                        Lane = lane
                    };
                }
                break;
            }
            default: // case NoteLane.CENTER
                newNote = (start.SoftEquals(end) ?
                    new FreestyleNote
                    {
                        Time = start
                    } :
                    new MashNote
                    {
                        Time = start,
                        EndTime = end
                    });
                break;
            // camera and marker lanes can be skipped because they will never appear here
        }

        var shouldReplace = false;
        if (oldNote != null)
        {
            if (newNote.Time.SoftEquals(oldNote.Time))
            {
                shouldReplace = true;
            }

            if (!shouldReplace && !newNote.Instant && !oldNote.Instant)
            {
                if ((newNote.Time.SoftEquals(oldNote.EndTime) ||
                     oldNote.Time.SoftEquals(newNote.EndTime)) && newNote.Type == oldNote.Type)
                {
                    newNote.Time = Math.Min(newNote.Time, oldNote.Time);
                    newNote.EndTime = Math.Max(newNote.EndTime, oldNote.EndTime);
                    shouldReplace = true;
                }
            }
        }

        if (shouldReplace)
        {
            CommandInvoker.Execute(new UpdateNoteCommand(oldNote, newNote));
        }
        else
        {
            CommandInvoker.Execute(new AddNotesCommand([newNote]));
        }
    }

    private static void DoMarkerOperation()
    {
        if (Chart.GetNote(Chart.CurrentTime, NoteLane.MARKER) is { } marker)
        {
            Console.WriteLine("Delete marker");
            CommandInvoker.Execute(new DeleteNotesCommand([marker]));
        }
        else
        {
            CommandInvoker.Execute(new AddNotesCommand([
                new MarkerDummyNote(Chart.CurrentTime,
                                    ShiftPressed ? 1 : CtrlPressed ? 2 : 0)
            ]));
        }
    }
    
    private static void DoCameraSwapOperation()
    {
        if (Chart.GetNote(Chart.CurrentTime, NoteLane.CAMERA) is { } note)
        {
            CommandInvoker.Execute(new DeleteNotesCommand([note]));
        }
        else
        {
            CommandInvoker.Execute(new AddNotesCommand([new CameraChange
            {
                Time = Chart.CurrentTime,
                Flags = new NoteFlags(false, false, ShiftPressed)
            }]));
        }
    }

    private static void DoSetFlagOperation(char flag)
    {
        if (SelectedNotes.Count == 0)
        {
            App.MainWindowViewModel.ShowEventIndicator("No notes selected to set flags");
            return;
        }
        
        // flag operations prioritize making the flag true for all notes
        var newValue = false;
        List<(NoteBase, bool)> notes = [];
        foreach (var note in SelectedNotes)
        {
            var currentValue = flag switch
            {
                'c' => note.Flags.C,
                'f' => note.Flags.F,
                'w' => note.Flags.W,
                _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, null)
            };
            notes.Add((note, currentValue));
            if (!currentValue)
            {
                newValue = true;
            }
        }
        
        CommandInvoker.Execute(new SetFlagsCommand(flag, newValue, notes));
    }
    
    private static void SetBreakpoint()
    {
        if (!Config.EnableBreakpoints)
        {
            App.MainWindowViewModel.ShowEventIndicator("Breakpoints are disabled.");
            return;
        }

        if (!Config.PracticeModInstalled)
        {
            App.MainWindowViewModel.ShowEventIndicator("Install Practice Mod to use breakpoints.");
            return;
        }

        if (Chart.Metadata.SongName == "")
        {
            App.MainWindowViewModel.ShowEventIndicator("Set song name to use breakpoints.");
            return;
        }

        if (Chart.CurrentTime.SoftEquals(BreakpointTime))
        {
            DeleteBreakpoint();
            return;
        }
        
        BreakpointTime = Chart.CurrentTime;
        App.MainWindowViewModel.ShowEventIndicator(
            $@"Breakpoint set at {TimeSpan.FromMilliseconds(BreakpointTime):mm\:ss\.fff} seconds.");
        App.MainWindowViewModel.BreakpointTimeText = TimeSpan.FromMilliseconds(BreakpointTime)
                                                             .ToString(@"mm\:ss\.fff");
        
        // this loads the entire file into memory but the practice mode settings file is going to be
        // small enough that i can get away without streaming it
        var lines = File.ReadAllLines(Config.PracticeModConfigPath).ToList();
        var index = lines.FindIndex(
            l => l.StartsWith($"{Chart.Metadata.SongName.ToLowerInvariant()}:"));
        if (index == -1)
        {
            lines.Add($"{Chart.Metadata.SongName.ToLowerInvariant()}:{Math.Floor(BreakpointTime)}");
        }
        else
        {
            lines[index] =
                $"{Chart.Metadata.SongName.ToLowerInvariant()}:{Math.Floor(BreakpointTime)}";
        }
        File.WriteAllLines(Config.PracticeModConfigPath, lines);
    }
    
    private static void DeleteBreakpoint()
    {
        BreakpointTime = -1;
        App.MainWindowViewModel.ShowEventIndicator("Breakpoint deleted.");
        App.MainWindowViewModel.BreakpointTimeText = "n/a";
        
        var lines = File.ReadAllLines(Config.PracticeModConfigPath).ToList();
        var index = lines.FindIndex(
            l => l.StartsWith($"{Chart.Metadata.SongName.ToLowerInvariant()}:"));
        if (index != -1)
        {
            lines.RemoveAt(index);
        }
        File.WriteAllLines(Config.PracticeModConfigPath, lines);
    }
}