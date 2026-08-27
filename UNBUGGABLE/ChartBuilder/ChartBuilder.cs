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
using UNBEATABLEChartEditor.Input;
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
    
    public static long TopLaneStartTime { get; private set; } = -1000;
    public static long BottomLaneStartTime { get; private set; } = -1000;
    public static long CenterLaneStartTime { get; private set; } = -1000;
    
    public static bool PlacingNote => TopLaneStartTime != -1000 || BottomLaneStartTime != -1000 ||
                                      CenterLaneStartTime != -1000;
    
    public static bool QuickScroll { get; set; } = false;
    
    public static List<NoteBase> SelectedNotes = [];
    
    public static long BreakpointTime { get; private set; } = -1000;
    
    // 0 for normal notes, 1-4 for cop notes
    public static int CopId { get; private set; } = 0;
    
    private static readonly List<string> NoteTypeNames = [
        "notes", "cop 1", "cop 2", "cop 3", "cop 4"];
    
    private static readonly NoteFlags LockedFlags = new(false, false, false);

    public static void ResetInputStates()
    {
        MousePosition = new Point(-1000, -1000);
        MouseDragStart = null;
        MouseDragStartTime = -1000;
        RightMouseDrag = false;
        TopLaneStartTime = -1000;
        BottomLaneStartTime = -1000;
        CenterLaneStartTime = -1000;
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
            RightMouseDrag = rightButton;
            MouseDragStart = new Point(MousePosition.X, MousePosition.Y);
            MouseDragStartTime = NoteViewer.ScreenCoordsToTime(MouseDragStart.Value.Y);
        }
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

        var hoveredNote = Chart.NonMarkerNotes.FirstOrDefault(n => n.MouseOver());
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
            if (InputManager.CtrlPressed)
            {
                SelectedNotes.AddRange(notes.Where(n => !SelectedNotes.Contains(n)).ToList());
            }
            else
            {
                SelectedNotes = notes;
            }
            Trace.WriteLine($"Selected {SelectedNotes.Count} notes");
        }
        
        // tail selection only works if you don't drag select
        if (notes.Count == 0 && Config.Settings.HoldTailSelect != "none")
        {
            List<NoteBase> clickedNotes = [];
            if (Config.Settings.HoldTailSelect == "all")
            {
                clickedNotes = Chart.NonMarkerNotes.Where(n => !n.Instant && n.MouseOverTail())
                                    .ToList();
            }
            else
            {
                var note = Config.Settings.HoldTailSelect switch
                {
                    "first" => Chart.NonMarkerNotes
                                    .FirstOrDefault(n => !n.Instant && n.MouseOverTail()),
                    "last" => Chart.NonMarkerNotes
                                   .LastOrDefault(n => !n.Instant && n.MouseOverTail()),
                    _ => throw new Exception("Invalid hold tail select setting")
                };
                
                if (note != null)
                {
                    clickedNotes.Add(note);
                }
            }

            if (clickedNotes.Count > 0)
            {
                if (RightMouseDrag)
                {
                    ChartBuilderCommandInvoker.Execute(new DeleteNotesCommand(clickedNotes));
                }
                else if (InputManager.CtrlPressed)
                {
                    SelectedNotes.AddRange(clickedNotes.Where(n => !SelectedNotes.Contains(n))
                                                       .ToList());
                }
                else
                {
                    SelectedNotes = clickedNotes;
                }
            }
        }
        
        RightMouseDrag = false;
        MouseDragStart = null;
        MouseDragStartTime = -1000;
    }

    public static async Task<bool> TryCreateChartFromAudio(string path)
    {
        var result = await Chart.TryCreateChartFromAudio(path);
        if (result.Item1)
        {
            ChartBuilderCommandInvoker.Reset();
        }
        else
        {
            await new MessageDialog($"Audio loading failed: {result.Item2}").ShowAsync();
        }
        
        return result.Item1;
    }

    public static async Task<bool> TryLoadChartFile(string path, bool noErrorDialog = false)
    {
        var result = await Chart.TryLoadChartFile(path);
        if (result.Item1)
        {
            ChartBuilderCommandInvoker.Reset();
        }
        else if (!noErrorDialog)
        {
            await new MessageDialog($"Chart loading failed: {result.Item2}").ShowAsync();
        }
        
        return result.Item1;
    }

    public static async void TryAutoLoadChartFile()
    {
        if (UserData.LastOpenedChartFile != "")
        {
            await TryLoadChartFile(UserData.LastOpenedChartFile, true);
        }
    }

    public static async Task<bool> SaveToBeatPath(string path)
    {
        return await Chart.SaveToBeatPath(path);
    }
    
    public static async Task<bool> SaveToStandardPath(string path)
    {
        return await Chart.SaveToStandardPath(path);
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

    public static void SelectNonMarker()
    {
        SelectedNotes = Chart.NonMarkerNotes.ToList();
    }

    public static void SelectLane(NoteLane lane)
    {
        SelectedNotes = Chart.Notes.Where(n => n.Lane == lane).ToList();
    }
    
    public static async Task Cut()
    {
        if (SelectedNotes.Count == 0)
        {
            return;
        }
        
        var segmentStartTime = SelectedNotes.Min(n => n.Time);
        var serialized =
            string.Join(";", SelectedNotes.Select(n => n.ToCopyPasteString(segmentStartTime))
                                          .ToList());
        await App.TopLevel.Clipboard.SetTextAsync(serialized);
        
        ChartBuilderCommandInvoker.Execute(new DeleteNotesCommand([..SelectedNotes]));
    }
    
    public static async Task Copy()
    {
        if (SelectedNotes.Count == 0)
        {
            return;
        }
        
        var segmentStartTime = SelectedNotes.Min(n => n.Time);
        var serialized =
            string.Join(";", SelectedNotes.Select(n => n.ToCopyPasteString(segmentStartTime))
                                          .ToList());
        await App.TopLevel.Clipboard.SetTextAsync(serialized);
    }
    
    public static async Task Paste()
    {
        List<NoteBase> pasted = [];
        var clipboardText = await App.TopLevel.Clipboard.GetTextAsync();
        if (clipboardText is null or "")
        {
            Trace.WriteLine("Paste failed: clipboard is empty or contains non-text");
            return;
        }
        
        var noteStrings = clipboardText.Split(';');
        foreach (var noteString in noteStrings)
        {
            var note = NoteBase.FromCopyPasteString(noteString, Chart.CurrentTime);
            if (note == null)
            {
                Trace.WriteLine($"Paste failed: {noteString} is invalid");
                return;
            }
            
            pasted.Add(note);
        }
        
        if (pasted.Count == 0)
        {
            return;
        }
        
        ChartBuilderCommandInvoker.Execute(new PasteNotesCommand(pasted));
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
        else if (Chart.GetNotesAtCurrentTime().Count > 0)
        {
            ChartBuilderCommandInvoker.Execute(new MirrorNotesCommand(
                                                   Chart.GetNotesAtCurrentTime()
                                                        .Select(n => n.Item1).ToList()));
        }
    }

    public static void MoveSelectionForward()
    {
        if (SelectedNotes.Count > 0)
        {
            DoNoteMoveOperation(Chart.GetNextSnapTime() - Chart.CurrentTime);
        }
    }

    public static void MoveSelectionBack()
    {
        if (SelectedNotes.Count > 0)
        {
            DoNoteMoveOperation(Chart.GetPreviousSnapTime() - Chart.CurrentTime);
        }
    }

    public static async Task EditBpmRegion(BpmRegion region)
    {
        var bpm = await new NumberEntryDialog("edit bpm change",
                                              Math.Round(region.Bpm, 2)).ShowAsync();
        if (bpm.HasValue && bpm.Value.SoftNotEquals(region.Bpm, 0.0001))
        {
            // setting a region's bpm to the same as the previous region merges them
            if (region.Previous != null &&
                bpm.Value.SoftEquals(region.Previous.Bpm, 0.0001))
            {
                ChartBuilderCommandInvoker.Execute(new RemoveBpmRegionCommand(region));
            }
            else
            {
                ChartBuilderCommandInvoker.Execute(new EditBpmRegionCommand(region, bpm.Value));
            }
        }
    }

    public static void DeleteBpmRegion(BpmRegion region)
    {
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

    public static void StartTopLanePlacement()
    {
        if (TopLaneStartTime == -1000)
        {
            TopLaneStartTime = Chart.CurrentTime;
        }
    }

    public static void EndTopLanePlacement()
    {
        if (TopLaneStartTime != -1000)
        {
            var start = Math.Min(TopLaneStartTime, Chart.CurrentTime);
            var end = Math.Max(TopLaneStartTime, Chart.CurrentTime);
            CheckForNoteOperation(NoteLane.TOP, start, end);
            TopLaneStartTime = -1000;
        }
    }

    public static void StartBottomLanePlacement()
    {
        if (BottomLaneStartTime == -1000)
        {
            BottomLaneStartTime = Chart.CurrentTime;
        }
    }

    public static void EndBottomLanePlacement()
    {
        if (BottomLaneStartTime != -1000)
        {
            var start = Math.Min(BottomLaneStartTime, Chart.CurrentTime);
            var end = Math.Max(BottomLaneStartTime, Chart.CurrentTime);
            CheckForNoteOperation(NoteLane.BOTTOM, start, end);
            BottomLaneStartTime = -1000;
        }
    }

    public static void StartCenterLanePlacement()
    {
        if (CenterLaneStartTime == -1000)
        {
            CenterLaneStartTime = Chart.CurrentTime;
        }
    }

    public static void EndCenterLanePlacement()
    {
        if (CenterLaneStartTime != -1000)
        {
            var start = Math.Min(CenterLaneStartTime, Chart.CurrentTime);
            var end = Math.Max(CenterLaneStartTime, Chart.CurrentTime);
            CheckForNoteOperation(NoteLane.CENTER, start, end);
            CenterLaneStartTime = -1000;
        }
    }
    
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
                        false, false, InputManager.ShiftPressed)
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

    public static void PrevCop()
    {
        --CopId;
        if (CopId < 0)
        {
            CopId = 4;
        }
        SetCopId(CopId);
    }

    public static void NextCop()
    {
        ++CopId;
        if (CopId > 4)
        {
            CopId = 0;
        }
        SetCopId(CopId);
    }

    public static async Task AddBpmChange()
    {
        var time = Chart.CurrentTime;
        var existingRegion = Chart.GetBpmRegion(time);
        if (existingRegion != null)
        {
            await EditBpmRegion(existingRegion);
        }
        else
        {
            var bpm = await new NumberEntryDialog("add bpm change").ShowAsync();
            if (bpm.HasValue && bpm.Value != 0.0)
            {
                ChartBuilderCommandInvoker.Execute(new AddBpmRegionCommand(time, bpm.Value));
            }
        }
    }

    public static void RemoveBpmChange()
    {
        var time = Chart.CurrentTime;
        var existingRegion = Chart.GetBpmRegion(time);
        // the first bpm region can't be removed for obvious reasons
        if (existingRegion != null && existingRegion != Chart.BpmRegions[0])
        {
            ChartBuilderCommandInvoker.Execute(new RemoveBpmRegionCommand(existingRegion));
        }
    }

    public static async Task AddLabel()
    {
        var time = Chart.CurrentTime;
        var existingLabel = Chart.GetLabel(time);
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

    public static void RemoveLabel()
    {
        var time = Chart.CurrentTime;
        var existingLabel = Chart.GetLabel(time);
        if (existingLabel != null)
        {
            ChartBuilderCommandInvoker.Execute(new RemoveLabelCommand(existingLabel));
        }
    }
    
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

        if (Chart.CurrentTime == BreakpointTime)
        {
            TryRemoveBreakpoint();
            return;
        }
        
        BreakpointTime = Chart.CurrentTime;
        App.MainWindowViewModel.ShowEventIndicator(
            $@"Breakpoint set at {TimeSpan.FromMilliseconds(BreakpointTime):mm\:ss\.fff}");
        App.MainWindowViewModel.BreakpointTimeText = TimeSpan.FromMilliseconds(BreakpointTime)
                                                             .ToString(@"mm\:ss\.fff");
        
        // this loads the entire file into memory but the practice mode settings file is going to be
        // small enough that i can get away without streaming it
        var lines = File.ReadAllLines(Config.PracticeModConfigPath).ToList();
        var index = lines.FindIndex(
            l => l.StartsWith($"{Chart.Metadata.SongName.ToLowerInvariant()}:"));
        if (index == -1)
        {
            lines.Add($"{Chart.Metadata.SongName.ToLowerInvariant()}:" +
                      $"{BreakpointTime + Chart.Metadata.ChartOffset}");
        }
        else
        {
            lines[index] =
                $"{Chart.Metadata.SongName.ToLowerInvariant()}:" +
                $"{BreakpointTime + Chart.Metadata.ChartOffset}";
        }
        File.WriteAllLines(Config.PracticeModConfigPath, lines);
    }
    
    public static void TryRemoveBreakpoint(bool showEventIndicator = true)
    {
        if (BreakpointTime == -1000 || !Config.Settings.EnableBreakpoints ||
            !Config.PracticeModInstalled)
        {
            return;
        }
        
        BreakpointTime = -1000;
        if (!Config.Settings.EnableBreakpoints || !Config.PracticeModInstalled)
        {
            return;
        }
        
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

    public static void CheckExistingBreakpoint()
    {
        if (!Config.Settings.EnableBreakpoints || !Config.PracticeModInstalled)
        {
            return;
        }
        
        var lines = File.ReadAllLines(Config.PracticeModConfigPath).ToList();
        var index = lines.FindIndex(
            l => l.StartsWith($"{Chart.Metadata.SongName.ToLowerInvariant()}:"));
        if (index != -1 && long.TryParse(lines[index].Split(':')[1], out var time))
        {
            BreakpointTime = time - Chart.Metadata.ChartOffset;
            App.MainWindowViewModel.BreakpointTimeText = TimeSpan.FromMilliseconds(BreakpointTime)
                                                                 .ToString(@"mm\:ss\.fff");
            Trace.WriteLine($"Found existing breakpoint at {BreakpointTime}");
        }
        else
        {
            TryRemoveBreakpoint(false);
        }
    }

    public static void AddMarker(int type)
    {
        ChartBuilderCommandInvoker.Execute(new UpdateMarkerCommand(Chart.CurrentTime, type));
    }

    public static void SetNoteFlags(char flag)
    {
        List<NoteBase> notes_ = [];
        if (SelectedNotes.Count > 0)
        {
            notes_ = [..SelectedNotes];
        }
        else if (Chart.GetNotesAtCurrentTime().Count > 0)
        {
            notes_ = Chart.GetNotesAtCurrentTime().Select(n => n.Item1).ToList();
        }
        else
        {
            App.MainWindowViewModel.ShowEventIndicator("No notes selected to set flags");
            return;
        }
        
        // flag operations prioritize making the flag true for all notes
        var newValue = false;
        List<(NoteBase, bool)> notes = [];
        foreach (var note in notes_)
        {
            if (flag == 'n' && (note.Type is not NoteType.SINGLE and not NoteType.SPIKE
                    and not NoteType.HOLD and not NoteType.DOUBLE))
            {
                continue;
            }
            
            var currentValue = flag switch
            {
                'c' => note.Flags.C,
                'f' => note.Flags.F,
                'w' => note.Flags.W,
                'n' => note.Flags.N,
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

    public static void NudgeNoteHeads(int distance)
    {
        if (SelectedNotes.Count == 0)
        {
            return;
        }
        
        List<(NoteBase, int, int)> nudges = [];
        foreach (var note in SelectedNotes)
        {
            nudges.Add((note, distance, 0));
        }
        
        ChartBuilderCommandInvoker.Execute(new NudgeNotesCommand(nudges));
    }
    
    public static void NudgeNoteTails(int distance)
    {
        if (SelectedNotes.Count == 0)
        {
            return;
        }
        
        List<(NoteBase, int, int)> nudges = [];
        foreach (var note in SelectedNotes)
        {
            nudges.Add((note, 0, distance));
        }
        
        ChartBuilderCommandInvoker.Execute(new NudgeNotesCommand(nudges));
    }

    public static void ToggleFlagLock(char flag)
    {
        bool newValue;
        switch (flag)
        {
            case 'c':
                LockedFlags.C = !LockedFlags.C;
                newValue = LockedFlags.C;
                break;
            case 'f':
                LockedFlags.F = !LockedFlags.F;
                newValue = LockedFlags.F;
                break;
            case 'w':
                LockedFlags.W = !LockedFlags.W;
                newValue = LockedFlags.W;
                break;
            case 'n':
                LockedFlags.N = !LockedFlags.N;
                newValue = LockedFlags.N;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
        }

        App.MainWindowViewModel.ShowEventIndicator(
            flag == 'n'
                ? $"Noisz spawn {(newValue ? "locked" : "unlocked")}"
                : $"{char.ToUpper(flag)} flag {(newValue ? "locked" : "unlocked")}");
        App.MainWindowViewModel.LockedFlagsText = (LockedFlags.N ? "N" : "") +
                                                  (LockedFlags.C ? "C" : "") +
                                                  (LockedFlags.F ? "F" : "") +
                                                  (LockedFlags.W ? "W" : "");
    }

    private static void CheckForNoteOperation(NoteLane lane, long start, long end)
    {
        var oldNote = Chart.GetNote(start, lane, 1);
        // hold notes can also extend from the start of the note
        if (oldNote == null && end != start)
        {
            oldNote =
                Chart.GetNote(end, lane, Config.Settings.HoldExtensionSearchThreshold) ??
                Chart.GetNoteFromEnd(start, lane, Config.Settings.HoldExtensionSearchThreshold);
        }
        
        if (oldNote != null)
        {
            if (start == end)
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
                    if (start == end)
                    {
                        newNote = new SingleNote
                        {
                            Lane = lane,
                            Time = start,
                            Flags = new NoteFlags(false, false, InputManager.ShiftPressed)
                        };
                    }
                    else
                    {
                        newNote = new HoldNote
                        {
                            Lane = lane,
                            Time = start,
                            EndTime = end,
                            Flags = new NoteFlags(false, false, InputManager.ShiftPressed)
                        };
                    }
                }
                else
                {
                    if (start != end && InputManager.ShiftPressed && lane == NoteLane.TOP &&
                        !Config.Settings.AllowTopLaneCopMashes)
                    {
                        App.MainWindowViewModel.ShowEventIndicator(
                            "Top lane cop mashes do not appear in-game.");
                        return;
                    }
                    
                    // holding shift always creates a spike
                    if (InputManager.ShiftPressed && start == end)
                    {
                        newNote = new SingleNote
                        {
                            Lane = lane,
                            Time = start,
                            Flags = new NoteFlags(false, false, true)
                        };
                    }
                    else
                    {
                        newNote = new CopNote(start == end ? NoteType.COP_SINGLE : 
                                              InputManager.ShiftPressed ? NoteType.COP_MASH :
                                              NoteType.COP_HOLD, CopId)
                        {
                            Time = start,
                            EndTime = end,
                            Lane = lane
                        };
                    }
                }
                break;
            }
            default: // NoteLane.CENTER
                newNote = (start == end ?
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
            if (newNote.Time == oldNote.Time)
            {
                shouldReplace = true;
            }

            // holding ctrl always places a new note instead of trying to extend existing notes
            // (this is mainly useful for chaining doubles)
            if (!shouldReplace && !newNote.Instant && !oldNote.Instant && !InputManager.CtrlPressed)
            {
                if ((newNote.Time == oldNote.EndTime ||
                     oldNote.Time == newNote.EndTime) && newNote.Type == oldNote.Type)
                {
                    newNote.Time = Math.Min(newNote.Time, oldNote.Time);
                    newNote.EndTime = Math.Max(newNote.EndTime, oldNote.EndTime);
                    shouldReplace = true;
                }
            }
        }

        if (shouldReplace)
        {
            if (Config.Settings.PreserveNoiszFlag && newNote is SingleNote or HoldNote &&
                oldNote is SingleNote or HoldNote)
            {
                newNote.Flags.N = oldNote.Flags.N;
            }
            
            ChartBuilderCommandInvoker.Execute(
                new UpdateNotesCommand([oldNote], [newNote],
                                       Config.Settings.AutoSelectBehavior == "all"));
        }
        else
        {
            if (LockedFlags.C)
            {
                newNote.Flags.C = true;
            }
            if (LockedFlags.F)
            {
                newNote.Flags.F = true;
            }
            if (LockedFlags.W)
            {
                newNote.Flags.W = true;
            }
            if (LockedFlags.N)
            {
                newNote.Flags.N = true;
            }
            ChartBuilderCommandInvoker.Execute(new AddNotesCommand([newNote]));
        }
    }
    
    private static void DoNoteMoveOperation(long delta)
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