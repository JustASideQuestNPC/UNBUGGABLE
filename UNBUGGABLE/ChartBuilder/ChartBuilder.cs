using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public static void ResetInputStates()
    {
        MousePosition = new Point(-1000, -1000);
        MouseDragStart = null;
        MouseDragStartTime = -1000;
        RightMouseDrag = false;
        TopLaneStartTime = -1000;
        BottomLaneStartTime = -1000;
        CenterLaneStartTime = -1000;
        _leftCtrlPressed = false;
        _rightCtrlPressed = false;
        _leftShiftPressed = false;
        _rightShiftPressed = false;
    }

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
                        ChartBuilderCommandInvoker.Execute(
                            new SetNotesCopIdCommand([..SelectedNotes], 2));
                    }
                    else
                    {
                        CopId = (CopId == 2 ? 0 : 2);
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
                        ChartBuilderCommandInvoker.Execute(
                            new SetNotesCopIdCommand([..SelectedNotes], 3));
                    }
                    else
                    {
                        CopId = (CopId == 3 ? 0 : 3);
                        App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[CopId];
                    }
                }
                else if (TopLaneStartTime.SoftEquals(-1000))
                {
                    Trace.WriteLine($"start top lane placement: {Chart.CurrentTime}");
                    TopLaneStartTime = Chart.CurrentTime;
                }

                break;
            case Key.D4:
                if (CtrlPressed)
                {
                    if (SelectedNotes.Count > 0)
                    {
                        ChartBuilderCommandInvoker.Execute(
                            new SetNotesCopIdCommand([..SelectedNotes], 4));
                    }
                    else
                    {
                        CopId = (CopId == 4 ? 0 : 4);
                        App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[CopId];
                    }
                }
                else if (BottomLaneStartTime.SoftEquals(-1000))
                {
                    Trace.WriteLine($"start bottom lane placement: {Chart.CurrentTime}");
                    BottomLaneStartTime = Chart.CurrentTime;
                }

                break;

            case Key.D6:
                if (CenterLaneStartTime.SoftEquals(-1000))
                {
                    Trace.WriteLine($"start center lane placement: {Chart.CurrentTime}");
                    CenterLaneStartTime = Chart.CurrentTime;
                }

                break;

            // non-hold keybinds
            // case Key.D0:
            case Key.D5:
                DoCameraSwapOperation();
                break;
            case Key.M:
                if (CtrlPressed && SelectedNotes.Count > 0)
                {
                    ChartBuilderCommandInvoker.Execute(new MirrorNotesCommand([..SelectedNotes]));
                }

                break;
            case Key.Back:
            case Key.Delete:
                if (SelectedNotes.Count > 0)
                {
                    ChartBuilderCommandInvoker.Execute(new DeleteNotesCommand([..SelectedNotes]));
                    SelectedNotes.Clear();
                }
                break;
            case Key.Escape:
                if (SelectedNotes.Count > 0)
                {
                    ClearSelection();
                }
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
            case Key.Up:
                if (SelectedNotes.Count > 0 && ShiftPressed)
                {
                    DoNoteMoveOperation(Chart.GetPreviousSnapTime() - Chart.CurrentTime);
                }
                else if (!Chart.Playing)
                {
                    Chart.MoveToPreviousSnap();
                }
                break;
            case Key.Down:
                if (SelectedNotes.Count > 0 && ShiftPressed)
                {
                    DoNoteMoveOperation(Chart.GetNextSnapTime() - Chart.CurrentTime);
                }
                else if (!Chart.Playing)
                {
                    Chart.MoveToNextSnap();
                }
                break;
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
                    Trace.WriteLine($"end top lane placement: {Chart.CurrentTime}");
                    var start = Math.Min(TopLaneStartTime, Chart.CurrentTime);
                    var end = Math.Max(TopLaneStartTime, Chart.CurrentTime);
                    CheckForNoteOperation(NoteLane.TOP, start, end);
                    TopLaneStartTime = -1000;
                }
                break;
            case Key.D4:
                if (BottomLaneStartTime.SoftNotEquals(-1000))
                {
                    Trace.WriteLine($"end bottom lane placement: {Chart.CurrentTime}");
                    var start = Math.Min(BottomLaneStartTime, Chart.CurrentTime);
                    var end = Math.Max(BottomLaneStartTime, Chart.CurrentTime);
                    CheckForNoteOperation(NoteLane.BOTTOM, start, end);
                    BottomLaneStartTime = -1000;
                }
                break;
            case Key.D6:
                if (CenterLaneStartTime.SoftNotEquals(-1000))
                {
                    Trace.WriteLine($"end center lane placement: {Chart.CurrentTime}");
                    var start = Math.Min(CenterLaneStartTime, Chart.CurrentTime);
                    var end = Math.Max(CenterLaneStartTime, Chart.CurrentTime);
                    CheckForNoteOperation(NoteLane.CENTER, start, end);
                    CenterLaneStartTime = -1000;
                }
                break;
        }
    }

    public static async Task OnMousePress(bool rightButton)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }

        if (await NoteViewer.CheckForEditByMouse(rightButton))
        {
            return;
        }

        if (MouseDragStart == null)
        {
            Trace.WriteLine($"Mouse press: {rightButton}");
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

        // this will be null if the mouse was pressed to edit or delete a label or bpm change
        if (MouseDragStart == null)
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
            ChartBuilderCommandInvoker.Execute(new DeleteNotesCommand(notes));
        }
        else
        {
            SelectedNotes = notes;
            Trace.WriteLine($"Selected {SelectedNotes.Count} notes");
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
            ChartBuilderCommandInvoker.Reset();
        }

        return result;
    }
    
    public static async Task<bool> TryLoadChartFile(string path)
    {
        var result = await Chart.TryLoadChartFile(path);
        if (result)
        {
            ChartBuilderCommandInvoker.Reset();
        }

        return result;
    }

    public static async void TryAutoLoadChartFile()
    {
        // command line arguments are used for file association
        Trace.WriteLine(Environment.CommandLine);
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
        ChartBuilderCommandInvoker.Undo();
    }
    
    public static void Redo()
    {
        ChartBuilderCommandInvoker.Redo();
    }
    
    public static void SelectAll()
    {
        SelectedNotes = Chart.Notes.ToList();
    }
    
    public static void Cut()
    {
        _clipboard = new List<NoteBase>([..SelectedNotes]);
        ChartBuilderCommandInvoker.Execute(new DeleteNotesCommand([..SelectedNotes]));
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
        
        ChartBuilderCommandInvoker.Execute(new AddNotesCommand(newNotes, true));
    }
    
    public static void ClearSelection()
    {
        SelectedNotes.Clear();
    }

    public static void DeleteSelection()
    {
        if (SelectedNotes.Count > 0)
        {
            ChartBuilderCommandInvoker.Execute(new DeleteNotesCommand([..SelectedNotes]));
            SelectedNotes.Clear();
        }
    }

    public static void MirrorSelection()
    {
        if (SelectedNotes.Count > 0)
        {
            ChartBuilderCommandInvoker.Execute(new MirrorNotesCommand([..SelectedNotes]));
        }
    }

    public static void MoveSelectionForward()
    {
        if (SelectedNotes.Count > 0)
        {
            DoNoteMoveOperation(Chart.GetPreviousSnapTime() - Chart.CurrentTime);
        }
    }

    public static void MoveSelectionBack()
    {
        if (SelectedNotes.Count > 0)
        {
            DoNoteMoveOperation(Chart.GetNextSnapTime() - Chart.CurrentTime);
        }
    }

    public static async Task EditBpmRegion(BpmRegion region)
    {
        var bpm = await new NumberEntryDialog("edit bpm change",
                                              Math.Round(region.Bpm, 2)).ShowAsync();
        if (bpm.HasValue && bpm.Value.SoftNotEquals(region.Bpm))
        {
            if (region.Previous != null)
            {
                Trace.WriteLine(
                    $"Edit bpm region: {bpm.Value} -> {region.Previous.Bpm}");
            }
                    
            // setting a region's bpm to the same as the previous region merges them
            if (region.Previous != null &&
                bpm.Value.SoftEquals(region.Previous.Bpm))
            {
                Trace.WriteLine("Merge bpm regions");
                ChartBuilderCommandInvoker.Execute(new RemoveBpmRegionCommand(region));
            }
            else
            {
                Trace.WriteLine("Edit bpm region");
                ChartBuilderCommandInvoker.Execute(new EditBpmRegionCommand(region, bpm.Value));
            }
        }
    }

    public static void DeleteBpmRegion(BpmRegion region)
    {
        Trace.WriteLine($"Remove bpm region at {region.StartTime} ms");
        ChartBuilderCommandInvoker.Execute(new RemoveBpmRegionCommand(region));
    }

    public static async Task EditLabel(Chart.Label label)
    {
        var text = await new TextEntryDialog("edit label", label.Text).ShowAsync();
        if (text.HasValue && text.Value != label.Text)
        {
            ChartBuilderCommandInvoker.Execute(new EditLabelCommand(label, text.Value));
        }
    }

    public static void DeleteLabel(Chart.Label label)
    {
        ChartBuilderCommandInvoker.Execute(new RemoveLabelCommand(label));
    }
    
    public static void StartTopLanePlacement() {}
    
    public static void EndTopLanePlacement() {}
    
    public static void StartBottomLanePlacement() {}
    
    public static void EndBottomLanePlacement() {}
    
    public static void StartCenterLanePlacement() {}
    
    public static void EndCenterLanePlacement() {}
    
    public static void PlaceCameraChange()
    {
        if (Chart.GetNote(Chart.CurrentTime, NoteLane.CAMERA) is { } note)
        {
            ChartBuilderCommandInvoker.Execute(new DeleteNotesCommand([note]));
        }
        else
        {
            ChartBuilderCommandInvoker.Execute(new AddNotesCommand([
                new CameraChange
                {
                    Time = Chart.CurrentTime,
                    Flags = new NoteFlags(
                        false, false, ShiftPressed)
                }
            ]));
        }
    }

    public static void SetCopId(int id)
    {
        if (SelectedNotes.Count > 0)
        {
            ChartBuilderCommandInvoker.Execute(
                new SetNotesCopIdCommand([..SelectedNotes], id));
        }
        else
        {
            CopId = id;
            App.MainWindowViewModel.CurrentNoteTypeText = NoteTypeNames[id];
        }
    }

    public static void PrevCop() {}
    
    public static void NextCop() {}
    
    public static void AddBpmChange() {}
    
    public static void RemoveBpmChange() {}
    
    public static void AddLabel() {}
    
    public static void RemoveLabel() {}
    
    public static void SetBreakpoint()
    {
        if (!Config.Settings.EnableBreakpoints)
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
            RemoveBreakpoint();
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
    
    public static void RemoveBreakpoint(bool showEventIndicator = true)
    {
        BreakpointTime = -1000;
        if (showEventIndicator)
        {
            App.MainWindowViewModel.ShowEventIndicator("Breakpoint deleted.");
        }
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

    public static void AddMarker(int type) {}

    public static void SetNoteFlags(char flag)
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
        
        ChartBuilderCommandInvoker.Execute(new SetFlagsCommand(flag, newValue, notes));
    }

    public static void CheckExistingBreakpoint()
    {
        if (!Config.Settings.EnableBreakpoints || !Config.PracticeModInstalled)
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
            Trace.WriteLine($"Found existing breakpoint at {BreakpointTime}");
        }
        else
        {
            RemoveBreakpoint(false);
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
                DeleteLabel(existingLabel);
            }
        }
        else
        {
            if (existingLabel != null)
            {
                await EditLabel(existingLabel);
            }
            else
            {
                var text = await new TextEntryDialog("add label").ShowAsync();
                if (text.HasValue && text.Value != "")
                {
                    ChartBuilderCommandInvoker.Execute(new AddLabelCommand(
                                                           time + Chart.Metadata.ChartOffset,
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
                Trace.WriteLine($"Remove bpm region at {time} ms");
                ChartBuilderCommandInvoker.Execute(new RemoveBpmRegionCommand(existingRegion));
            }
        }
        else
        {
            if (existingRegion != null)
            {
                await EditBpmRegion(existingRegion);
            }
            else
            {
                var bpm = await new NumberEntryDialog("add bpm change").ShowAsync();
                if (bpm.HasValue)
                {
                    Trace.WriteLine($"Add bpm region at {time} ms");
                    ChartBuilderCommandInvoker.Execute(new AddBpmRegionCommand(time, bpm.Value));
                }
            }
        }
    }

    private static void CheckForNoteOperation(NoteLane lane, double start, double end)
    {
        var oldNote = Chart.GetNote(start, lane);
        Trace.WriteLine(oldNote);
        // hold notes can also extend from the start of the note
        if (oldNote == null && end.SoftNotEquals(start))
        {
            oldNote = Chart.GetNote(end, lane) ?? Chart.GetNoteFromEnd(start, lane);
        }
        Trace.WriteLine(oldNote);
        
        if (oldNote != null)
        {
            if (start.SoftEquals(end))
            {
                ChartBuilderCommandInvoker.Execute(new DeleteNotesCommand([oldNote]));
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
                        !Config.Settings.AllowTopLaneCopMashes)
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
            ChartBuilderCommandInvoker.Execute(new UpdateNotesCommand([oldNote], [newNote]));
        }
        else
        {
            ChartBuilderCommandInvoker.Execute(new AddNotesCommand([newNote]));
        }
    }

    private static void DoMarkerOperation()
    {
        if (Chart.GetNote(Chart.CurrentTime, NoteLane.MARKER) is { } marker)
        {
            Trace.WriteLine("Delete marker");
            ChartBuilderCommandInvoker.Execute(new DeleteNotesCommand([marker]));
        }
        else
        {
            ChartBuilderCommandInvoker.Execute(new AddNotesCommand([
                new MarkerDummyNote(Chart.CurrentTime,
                                    ShiftPressed ? 1 : CtrlPressed ? 2 : 0)
            ]));
        }
    }
    
    private static void DoNoteMoveOperation(double delta)
    {
        if (delta == 0)
        {
            return;
        }

        List<NoteBase> newNotes = [];
        foreach (var note in SelectedNotes)
        {
            newNotes.Add(note.Clone(note.Time + delta));
        }
        
        ChartBuilderCommandInvoker.Execute(new UpdateNotesCommand([..SelectedNotes], newNotes,
                                                                  true));
    }
}