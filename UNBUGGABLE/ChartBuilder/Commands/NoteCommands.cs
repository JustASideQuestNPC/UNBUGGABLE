using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UNBEATABLEChartEditor;
using UNBUGGABLE.Resources;

namespace UNBUGGABLE.Commands;

public class AddNotesCommand(List<NoteBase> notes) : ICommand
{
    public string Name => "Add Notes";
    
    public bool UpdatesPriorityList => true;
    
    public void Execute()
    {
        ChartBuilder.ClearSelection();
        foreach (var note in notes)
        {
            Chart.AddNote(note);
        }

        if (Config.Settings.AutoSelectBehavior == "all")
        {
            ChartBuilder.SelectedNotes = [..notes];
        }
    }
    
    public void Undo()
    {
        ChartBuilder.ClearSelection();
        Trace.WriteLine(notes.Count);
        foreach (var note in notes)
        {
            Chart.RemoveNote(note);
        }
    }
}

public class PasteNotesCommand : ICommand
{
    public string Name => "Paste Notes";
    
    public bool UpdatesPriorityList => true;

    private readonly List<NoteBase> _addedNotes;
    private readonly List<NoteBase> _removedNotes = [];
    
    public PasteNotesCommand(List<NoteBase> notes)
    {
        switch (Config.Settings.PasteBehavior)
        {
            case "region":
            {
                _addedNotes = notes;
                var start = notes.Min(n => n.Time);
                var end = notes.Max(n => n.EndTime);
                _removedNotes = Chart.GetNoteRegion(start, end);
                break;
            }
            case "notes":
            {
                _addedNotes = notes;
                foreach (var note in notes)
                {
                    var existingNote = Chart.GetNote(note.Time, note.Lane);
                    if (existingNote != null)
                    {
                        _removedNotes.Add(existingNote);
                    }
                }

                break;
            }
            default: // "none"
            {
                _addedNotes = [];
                foreach (var note in notes)
                {
                    var existingNote = Chart.GetNote(note.Time, note.Lane);
                    if (existingNote == null)
                    {
                        _addedNotes.Add(note);
                    }
                }
                break;
            }
        }
    }
    
    public void Execute()
    {
        ChartBuilder.ClearSelection();
        foreach (var note in _removedNotes)
        {
            Chart.RemoveNote(note);
        }

        foreach (var note in _addedNotes)
        {
            Chart.AddNote(note);
        }
        
        if (Config.Settings.AutoSelectBehavior != "none")
        {
            ChartBuilder.SelectedNotes = [.._addedNotes];
        }
    }

    public void Undo()
    {
        ChartBuilder.ClearSelection();
        foreach (var note in _addedNotes)
        {
            Chart.RemoveNote(note);
        }
        
        foreach (var note in _removedNotes)
        {
            Chart.AddNote(note);
        }
    }
}

public class DeleteNotesCommand(List<NoteBase> notes) : ICommand
{
    public string Name => "Delete Notes";
    
    public bool UpdatesPriorityList => true;
    
    public void Execute()
    {
        ChartBuilder.ClearSelection();
        foreach (var note in notes)
        {
            Chart.RemoveNote(note);
        }
    }
    
    public void Undo()
    {
        ChartBuilder.ClearSelection();
        foreach (var note in notes)
        {
            Chart.AddNote(note);
        }
    }
}

public class UpdateNotesCommand(List<NoteBase> oldNotes, List<NoteBase> newNotes,
    bool transferSelected = false) : ICommand
{
    public string Name => "Update Notes";
    
    public bool UpdatesPriorityList => true;
    
    public void Execute()
    {
        if (transferSelected)
        {
            ChartBuilder.SelectedNotes = [..newNotes];
        }
        else
        {
            ChartBuilder.ClearSelection();
        }
        
        foreach (var note in oldNotes)
        {
            Chart.RemoveNote(note);
        }
        foreach (var note in newNotes)
        {
            Chart.AddNote(note);
        }
    }
    
    public void Undo()
    {
        if (transferSelected)
        {
            ChartBuilder.SelectedNotes = [..oldNotes];
        }
        else
        {
            ChartBuilder.ClearSelection();
        }
        
        foreach (var note in newNotes)
        {
            Chart.RemoveNote(note);
        }
        foreach (var note in oldNotes)
        {
            Chart.AddNote(note);
        }
    }
}

public class MirrorNotesCommand(List<NoteBase> notes) : ICommand
{
    public string Name => "Mirror Notes";
    
    public bool UpdatesPriorityList => true;
    
    public void Execute()
    {
        foreach (var note in notes)
        {
            MirrorNote(note);
        }
    }
    
    public void Undo()
    {
        foreach (var note in notes)
        {
            MirrorNote(note);
        }
    }
    
    private void MirrorNote(NoteBase note)
    {
        if (note.Lane == NoteLane.TOP)
        {
            note.Lane = NoteLane.BOTTOM;
        }
        else if (note.Lane == NoteLane.BOTTOM)
        {
            note.Lane = NoteLane.TOP;
        }
    }
}

public class SetFlagsCommand(char flag, bool newValue, List<(NoteBase, bool)> notes) : ICommand
{
    public string Name => newValue ? "Set note flags" : "Unset note flags";
    
    public bool UpdatesPriorityList => true;
    
    public void Execute()
    {
        foreach (var note in notes)
        {
            if (flag == 'n' && (note.Item1.Type is not NoteType.SINGLE and not NoteType.SPIKE
                    and not NoteType.HOLD and not NoteType.DOUBLE))
            {
                continue;
            }
            switch (flag)
            {
                case 'c':
                    note.Item1.Flags.C = newValue;
                    break;
                case 'f':
                    note.Item1.Flags.F = newValue;
                    break;
                case 'w':
                    note.Item1.Flags.W = newValue;
                    break;
                case 'n':
                    note.Item1.Flags.N = newValue;
                    break;
            }
        }
    }

    public void Undo()
    {
        foreach (var note in notes)
        {
            if (flag == 'n' && (note.Item1.Type is not NoteType.SINGLE and not NoteType.SPIKE
                    and not NoteType.HOLD and not NoteType.DOUBLE))
            {
                continue;
            }
            switch (flag)
            {
                case 'c':
                    note.Item1.Flags.C = note.Item2;
                    break;
                case 'f':
                    note.Item1.Flags.F = note.Item2;
                    break;
                case 'w':
                    note.Item1.Flags.W = note.Item2;
                    break;
                case 'n':
                    note.Item1.Flags.N = note.Item2;
                    break;
            }
        }   
    }
}

public class SetNotesCopIdCommand : ICommand
{
    public string Name => "Set Cop ID";
    
    public bool UpdatesPriorityList => true;
    
    private readonly List<NoteBase> _oldNotes;
    private readonly List<NoteBase> _newNotes;

    public SetNotesCopIdCommand(List<NoteBase> notes, int copId)
    {
        _oldNotes = notes;
        _newNotes = notes.Select(note => GetNoteWithCopId(note, copId)).ToList();
    }
    
    public void Execute()
    {
        for (var i = 0; i < _oldNotes.Count; i++)
        {
            Chart.ReplaceNote(_oldNotes[i], _newNotes[i]);
        }
        ChartBuilder.SelectedNotes = _newNotes;
    }
    
    public void Undo()
    {
        for (var i = 0; i < _oldNotes.Count; i++)
        {
            Chart.ReplaceNote(_newNotes[i], _oldNotes[i]);
        }
        ChartBuilder.SelectedNotes = _oldNotes;
    }

    /// <summary>
    /// Attempts to return a copy of a note with a new cop id. When converting non-cop notes to cop
    /// notes, only singles and holds will become cop notes. When converting cop notes to non-cop
    /// notes, both cop holds and cop mashes will become hold notes. Note flags are reset when
    /// converting between cop notes and non-cop notes.
    /// </summary>
    private static NoteBase GetNoteWithCopId(NoteBase note, int copId)
    {
        Trace.WriteLine($"{note.Type}");
        
        NoteBase newNote;
        if (copId == 0)
        {
            newNote = note.Type switch
            {
                NoteType.COP_SINGLE => new SingleNote
                {
                    Lane = note.Lane, Time = note.Time
                },
                NoteType.COP_HOLD or NoteType.COP_MASH => new HoldNote
                {
                    Lane = note.Lane, Time = note.Time, EndTime = note.EndTime
                },
                _ => note
            };
        }
        else
        {
            newNote = note.Type switch
            {
                NoteType.SINGLE => new CopNote(NoteType.COP_SINGLE, copId)
                {
                    Lane = note.Lane, Time = note.Time
                },
                NoteType.HOLD => new CopNote(NoteType.COP_HOLD, copId)
                {
                    Lane = note.Lane, Time = note.Time, EndTime = note.EndTime
                },
                NoteType.COP_SINGLE or NoteType.COP_HOLD or NoteType.COP_MASH =>
                    new CopNote(note.Type, copId)
                    {
                        Lane = note.Lane, Time = note.Time, EndTime = note.EndTime
                    },
                _ => note
            };
        }
        
        return newNote;
    }
}

public class ReorderNotesCommand(List<(NoteBase, int)> indexedOldOrder, List<NoteBase> newOrder) :
    ICommand
{
    public string Name => "Reorder Notes";
    
    public bool UpdatesPriorityList => false;
    
    private readonly List<NoteBase> _oldOrder = indexedOldOrder.Select(i => i.Item1).ToList();
    private readonly List<(NoteBase, int)> _indexedNewOrder =
        newOrder.Select((note, i) => (note, i)).ToList();

    // prevents a crash caused by modifying the ui during an event
    private bool _isFirstRun = true;
    
    public void Execute()
    {
        Chart.SetNoteOrder(newOrder);
        Trace.WriteLine(string.Join(',', _indexedNewOrder.Select(n => n.Item1.Lane)));
        if (!_isFirstRun)
        {
            App.MainWindowViewModel.UpdatePriorityListEntries(_indexedNewOrder);
        }
        else
        {
            _isFirstRun = false;
        }
    }
    
    public void Undo()
    {
        Chart.SetNoteOrder(_oldOrder);
        App.MainWindowViewModel.UpdatePriorityListEntries(indexedOldOrder);
        Trace.WriteLine(string.Join(',', indexedOldOrder.Select(n => n.Item1.Lane)));
    }   
}

public class NudgeNotesCommand(List<(NoteBase, int, int)> nudges) : ICommand
{
    public string Name => "Nudge Notes";
    
    public bool UpdatesPriorityList => true;
    
    public void Execute()
    {
        foreach (var nudge in nudges)
        {
            nudge.Item1.Time += nudge.Item2;
            // nudging the tail will nudge the head of non-instant notes -- this makes lining up
            // spike telegraphs and end chords much easier
            if (nudge.Item1.Instant)
            {
                nudge.Item1.Time += nudge.Item3;
            }
            else
            {
                nudge.Item1.EndTime += nudge.Item3;
            }
        }
        App.MainWindowViewModel.UpdatePriorityListEntries(Chart.GetNotesAtTime(Chart.CurrentTime));
    }
    
    public void Undo()
    {
        foreach (var nudge in nudges)
        {
            nudge.Item1.Time -= nudge.Item2;
            if (nudge.Item1.Instant)
            {
                nudge.Item1.Time -= nudge.Item3;
            }
            else
            {
                nudge.Item1.EndTime -= nudge.Item3;
            }
        }
        App.MainWindowViewModel.UpdatePriorityListEntries(Chart.GetNotesAtTime(Chart.CurrentTime));
    }
}

public class UpdateMarkerCommand(long time, int type) : ICommand
{
    public string Name => "Update Marker";
    
    public bool UpdatesPriorityList => true;

    public void Execute()
    {
        Chart.AddOrUpdateMarker(time, type == 0, type == 1, type == 2);
    }

    public void Undo()
    {
        // this stays the same because AddOrUpdateMarker toggles colors rather than setting them
        Chart.AddOrUpdateMarker(time, type == 0, type == 1, type == 2);
    }
}