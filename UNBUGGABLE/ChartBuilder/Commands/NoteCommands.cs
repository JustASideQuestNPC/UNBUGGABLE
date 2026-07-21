using System;
using System.Collections.Generic;
using System.Linq;
using UNBUGGABLE.Resources;

namespace UNBUGGABLE.Commands;

public class AddNotesCommand(List<NoteBase> notes, bool isPaste = false) : ICommand
{
    public void Execute()
    {
        ChartBuilder.ClearSelection();
        foreach (var note in notes)
        {
            Chart.AddNote(note);
        }

        if (isPaste && Config.AutoSelectPastedNotes)
        {
            ChartBuilder.SelectedNotes = notes;
        }
    }
    
    public void Undo()
    {
        ChartBuilder.ClearSelection();
        foreach (var note in notes)
        {
            Chart.RemoveNote(note);
        }
    }
}

public class DeleteNotesCommand(List<NoteBase> notes) : ICommand
{
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

public class UpdateNoteCommand(NoteBase oldNote, NoteBase newNote) : ICommand
{
    public void Execute()
    {
        ChartBuilder.ClearSelection();
        Chart.RemoveNote(oldNote);
        Chart.AddNote(newNote);
    }
    
    public void Undo()
    {
        ChartBuilder.ClearSelection();
        Chart.RemoveNote(newNote);
        Chart.AddNote(oldNote);
    }
}

public class MirrorNotesCommand(List<NoteBase> notes) : ICommand
{
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
    public void Execute()
    {
        foreach (var note in notes)
        {
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
            }
        }
    }

    public void Undo()
    {
        foreach (var note in notes)
        {
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
            }
        }   
    }
}

public class SetNotesCopIdCommand : ICommand
{
    private List<NoteBase> _oldNotes;
    private List<NoteBase> _newNotes;

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
        Console.WriteLine($"{note.Type}");
        
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