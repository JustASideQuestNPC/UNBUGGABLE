using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Media;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public enum NoteType
{
    SINGLE,
    HOLD,
    SPIKE,
    DOUBLE,
    FREESTYLE,
    MASH,
    CAMERA_SWAP,
    CAMERA_WIDE,
    CAMERA_INSTANT,
    COP_SINGLE,
    COP_HOLD,
    COP_MASH,
    MARKER_DUMMY
}

public enum NoteLane
{
    TOP,
    BOTTOM,
    CENTER,
    CAMERA,
    MARKER
}

public class NoteFlags(bool c, bool f, bool w)
{
    public bool C { get; set; } = c;
    public bool F { get; set; } = f;
    public bool W { get; set; } = w;
}

/// <summary>
/// Any one note on the chart.
/// </summary>
public abstract partial class NoteBase
{
    [GeneratedRegex(@"\d\d\d,192,-?\d+,\d+,\d+,(\d+:){4,5}")]
    private static partial Regex HitObjectRegex();
    
    public double Time { get; set; }

    /// <summary>
    /// If true, the note has no duration and is a single, spike, freestyle, or camera note. If
    /// false, the note is a hold, double, or mash note.
    /// </summary>
    public bool Instant => Type != NoteType.HOLD && Type != NoteType.DOUBLE &&
                           Type != NoteType.MASH && Type != NoteType.COP_HOLD &&
                           Type != NoteType.COP_MASH;

    /// <summary>
    /// What time the note ends at after being hit. Only used by holds, doubles, and mash notes.
    /// </summary>
    public double EndTime { get; set; } = 0;
    public double Duration => Instant ? 0 : EndTime - Time;
    
    public abstract NoteType Type { get; }
    public virtual NoteLane Lane { get; set; }
    
    // invisible notes disappear 1 beat before reaching the player
    public bool Invisible { get; }
    
    /// <summary>
    /// Sound flags applied to the note, ordered as [c, f, w].
    /// </summary>
    public NoteFlags Flags { get; set; }

    /// <summary>
    /// Which cop (from 1-4) is assigned to the note, or 0 if it's a normal note.
    /// </summary>
    public int CopId { get; set; } = 0;
    
    protected SolidColorBrush _outlineBrush;
    protected SolidColorBrush _selectedBrush;
    protected Typeface _typeface;
    
    /// <summary>
    /// Attempts to construct a note from a hit object string in a chart file.
    /// </summary>
    /// <returns>The note if it could be constructed, otherwise null.</returns>
    public static NoteBase? FromHitObjectString(string hitObjectString, out string errorMessage)
    {
        // Logger.WriteLine($"\nHit object string: {hitObjectString}");
        if (!HitObjectRegex().IsMatch(hitObjectString))
        {
            errorMessage =
                $"Hit object string \"{hitObjectString}\" does not match expected format.";
            return null;
        }
        
        var d = hitObjectString.Split(',');
        var laneNumber = int.Parse(d[0]);
        var noteTime = double.Parse(d[2]) - Chart.Metadata.ChartOffset;
        var instantNumber = int.Parse(d[3]);
        
        var noteFlagNumber = int.Parse(d[4]);
        
        var hitObjectParams = d[5].Split(":");
        var param1 = int.Parse(hitObjectParams[0]);
        var param2 = int.Parse(hitObjectParams[1]);
        var param3 = int.Parse(hitObjectParams[2]);

        var lane = NoteLane.CENTER;
        bool instant;
        NoteFlags flags;
        double endTime = 0;
        
        if ((laneNumber is 213 or 298 or 384 or 469) || (laneNumber == 128 && Config.Lane2Markers))
        {
            if (laneNumber == 128)
            {
                Chart.TryAddMarker(noteTime, 0);
                errorMessage = "marker";
                return null;
            }
            
            lane = laneNumber switch
                {
                    213 => NoteLane.TOP,
                    298 => NoteLane.BOTTOM,
                    384 => NoteLane.CAMERA,
                    _ => NoteLane.CENTER
                };
        }
        // notes in lane 1 (and lane 2 if they aren't being used as markers) aren't used by this
        // editor but they're still valid notes
        else if (laneNumber != 42 && laneNumber != 128)
        {
            errorMessage = $"Invalid lane number: {laneNumber}";
            return null;
        }
        
        // this number is *supposed* to be 1 for an instant note and 128 for a hold, but for some
        // reason the very first note in a chart uses 5 and 132 instead
        if (instantNumber is 1 or 5 or 128 or 132)
        {
            instant = (instantNumber is 1 or 5);
            // Logger.WriteLine($"Instant note: {instant}");
        }
        else
        {
            errorMessage = $"Invalid instant number: {instantNumber}";
            return null;
        }

        if (noteFlagNumber is >= 0 and <= 14)
        {
            var noteFlagString = Convert.ToString(noteFlagNumber, 2).PadLeft(4, '0');
            flags = new NoteFlags(noteFlagString[0] == '1', noteFlagString[1] == '1',
                                  noteFlagString[2] == '1');
        }
        else
        {
            errorMessage = $"Invalid note flags: {noteFlagNumber}/" +
                           $"{Convert.ToString(noteFlagNumber, 2).PadLeft(4, '0')}";
            return null;
        }

        if (!instant)
        {
            endTime = param1 - Chart.Metadata.ChartOffset;
        }

        NoteBase note;
        if ((instant && param1 == 3) || (!instant && param2 == 3))
        {
            var copId = 0;
            var isFinisher = false;
            switch (noteFlagNumber)
            {
                case 0:
                    copId = 1;
                    isFinisher = false;
                    break;
                case 2:
                    copId = 2;
                    isFinisher = false;
                    break;
                case 4:
                    copId = 1;
                    isFinisher = true;
                    break;
                case 6:
                    copId = 2;
                    isFinisher = true;
                    break;
                case 8:
                    copId = 3;
                    isFinisher = false;
                    break;
                case 10:
                    copId = 4;
                    isFinisher = false;
                    break;
                case 12:
                    copId = 3;
                    isFinisher = true;
                    break;
                case 14:
                    copId = 4;
                    isFinisher = true;
                    break;
            }

            note = new CopNote(
                instant ? NoteType.COP_SINGLE : param3 == 1 ? NoteType.COP_HOLD : NoteType.COP_MASH,
                copId, isFinisher
            );
        }
        else
        {
            switch (lane)
            {
                case NoteLane.CAMERA:
                    note = new CameraChange();
                    break;
                case NoteLane.CENTER:
                    note = (instant ? new FreestyleNote() : new MashNote());
                    break;
                default:
                {
                    note = (instant ? new SingleNote() : new HoldNote());
                    break;
                }
            }
        }

        note.Lane = lane;
        note.Time = noteTime;
        note.Flags = flags;
        note.EndTime = endTime;
        
        errorMessage = "";
        return note;
    }

    protected NoteBase(NoteFlags? startingFlags = null)
    {
        _outlineBrush = (SolidColorBrush)App.Current.Resources["NoteOutline"];
        _selectedBrush = (SolidColorBrush)App.Current.Resources["SelectedNoteOverlay"];
        _typeface = new Typeface((FontFamily)App.Current.Resources["RobotoMonoBold"]);
        Flags = startingFlags ?? new NoteFlags(false, false, false);
    }

    /// <summary>
    /// Renders the note in the note viewer.
    /// </summary>
    public abstract void Render(DrawingContext dc, bool selected);
    
    /// <summary>
    /// Renders the note in the note preview. This should always render the note as if it comes
    /// from the left.
    /// </summary>
    /// <param name="dc"></param>
    public abstract void RenderPreview(DrawingContext dc);

    public abstract long? ShouldPlayHitSound(double rangeStart, double rangeEnd);

    public virtual bool MouseOver()
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);
        return new Rect(x - 40, y - 12, 80, 24).ContainsPoint(ChartBuilder.MousePosition);
    }

    public NoteBase Clone(double? newTime = null)
    {
        var clone = (NoteBase)MemberwiseClone();
        if (newTime != null)
        {
            clone.EndTime = newTime.Value + (EndTime - Time);
            clone.Time = newTime.Value;
        }
        return clone;
    }
    
    public virtual string ToHitObjectString(bool isFirstNote, bool isStandardFile)
    {
        List<string> chunks =
        [
            Lane switch
            {
                NoteLane.TOP => "213",
                NoteLane.BOTTOM => "298",
                NoteLane.CENTER => "469",
                NoteLane.CAMERA => "384",
                _ => throw new ArgumentOutOfRangeException()
            },
            "192",
            Math.Floor(Time + Chart.Metadata.ChartOffset).ToString(),
        ];
        if (Instant)
        {
            chunks.Add(isFirstNote ? "5" : "1");
        }
        else
        {
            chunks.Add(isFirstNote ? "132" : "128");
        }
        chunks.Add(GetFlagString());
        chunks.Add(Instant ? "0:0:0:0:" : $"{Math.Floor(EndTime + Chart.Metadata.ChartOffset)}:0:0:0:0:");

        return string.Join(",", chunks);
    }

    protected void RenderFlags(DrawingContext dc, int x, double y, NoteFlags? flags = null)
    {
        if (flags == null || Config.AlwaysShowAllFlags)
        {
            flags = Flags;
        }

        var flagString =
            (flags.C ? "C" : "") +
            (flags.F ? "F" : "") +
            (flags.W ? "W" : "");
        
        var color = (SolidColorBrush)App.Current.Resources["TextPrimary"];
        var outline = new Pen((SolidColorBrush)App.Current.Resources["TextDark"], 2);
        var text = new FormattedText(flagString, CultureInfo.CurrentCulture,
                                     FlowDirection.LeftToRight, _typeface, 40, color);
        
        dc.DrawOutlinedText(text, new Point(x - text.Width / 2, y - 2 - text.Height / 2),
                            color, outline);
    }

    protected string GetFlagString()
    {
        var binaryString =
            $"{(Flags.C ? 1 : 0)}{(Flags.F ? 1 : 0)}{(Flags.W ? 1 : 0)}0";
        return Convert.ToInt32(binaryString, 2).ToString();
    }
}