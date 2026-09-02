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
    NEGATIVE_MASH,
    CAMERA_SWAP,
    CAMERA_ZOOM,
    CAMERA_INSTANT,
    CAMERA_SWAP_AND_ZOOM,
    COP_SINGLE,
    COP_HOLD,
    COP_MASH,
    MARKER
}

public enum NoteLane
{
    TOP,
    BOTTOM,
    CENTER,
    CAMERA,
    MARKER
}

public class NoteFlags(bool c, bool f, bool w, bool n = false)
{
    public bool C { get; set; } = c;
    public bool F { get; set; } = f;
    public bool W { get; set; } = w;
    
    /// <summary>
    /// Whether to make the note spawn in the center of the screen like in the Noisz stages from the
    /// base game; only applies to singles and holds. This isn't actually a note flag, but treating
    /// it like one makes things work infinitely better under the hood. 
    /// </summary>
    public bool N { get; set; } = n;
    
    public NoteFlags Clone() => new(C, F, W, N);
}

/// <summary>
/// Any one note on the chart.
/// </summary>
public abstract partial class NoteBase
{
    [GeneratedRegex(@"\d{2,3},192,-?\d+,\d+,\d+,[\d+:]{4,5}")]
    private static partial Regex HitObjectRegex();
    
    public long Time { get; set; }

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
    public long EndTime { get; set; } = 0;
    public long Duration => Instant ? 0 : EndTime - Time;
    
    public abstract NoteType Type { get; }
    
    public virtual NoteLane Lane { get; set; }
    
    // invisible notes disappear 1 beat before reaching the player
    public bool Invisible => Type is NoteType.SINGLE or NoteType.HOLD && Flags.C;
    
    /// <summary>
    /// Sound flags applied to the note, ordered as [c, f, w].
    /// </summary>
    public NoteFlags Flags { get; set; }

    /// <summary>
    /// Which cop (from 1-4) is assigned to the note, or 0 if it's a normal note.
    /// </summary>
    public int CopId { get; set; } = 0;
    
    protected Typeface Typeface = new((FontFamily)App.Current.Resources["RobotoMonoBold"]);
    
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
        var noteTime = long.Parse(d[2]);
        var instantNumber = int.Parse(d[3]);
        
        var noteFlagNumber = int.Parse(d[4]);
        
        var hitObjectParams = d[5].Split(":");
        var param1 = long.Parse(hitObjectParams[0]);
        var param2 = int.Parse(hitObjectParams[1]);
        var param3 = int.Parse(hitObjectParams[2]);

        var lane = NoteLane.CENTER;
        bool instant;
        NoteFlags flags;
        long endTime = 0;
        
        if ((laneNumber is 213 or 298 or 384 or 469) ||
            (laneNumber == 128 && Config.Settings.Lane2Markers))
        {
            if (laneNumber == 128)
            {
                Chart.TryAddMarker(noteTime);
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
        
        // convert negative mashes back to flagged freestyles
        if (note.Type == NoteType.MASH && note.EndTime < note.Time)
        {
            note = new FreestyleNote
            {
                Lane = note.Lane,
                Time = note.Time,
                Flags = note.Flags,
                EndTime = note.EndTime
            };
        }
        
        // handle noisz spawns
        if ((instant && param1 == 1) || (!instant && param2 == 1))
        {
            note.Flags.N = true;
        }
        
        errorMessage = "";
        return note;
    }

    public static NoteBase? FromCopyPasteString(string copyPasteString, long startTime)
    {
        var rawChunks = copyPasteString.Split(',').ToList();
        List<long> chunks = [];
        foreach (var chunk in rawChunks)
        {
            if (long.TryParse(chunk, out var parsedChunk))
            {
                chunks.Add(parsedChunk);
            }
            else
            {
                return null;
            }
        }

        if (chunks[0] < 0 || chunks[0] >= Enum.GetNames<NoteType>().Length)
        {
            return null;
        }
        
        var type = (NoteType)chunks[0];
        long time;
        
        if (type == NoteType.MARKER)
        {
            if (chunks.Count != 3)
            {
                return null;
            }
            
            // start time
            if (chunks[1] < 0)
            {
                return null;
            }
            time = chunks[1] + startTime;
            
            if (time < 0)
            {
                return null;
            }
            
            var colorString = Convert.ToString(chunks[2], 2).PadLeft(3, '0');
            if (colorString.Length != 3)
            {
                return null;
            }
            
            return new MarkerNote(time)
            {
                Color1 = colorString[0] == '1',
                Color2 = colorString[1] == '1',
                Color3 = colorString[2] == '1'
            };
        }
        
        if (chunks[1] < 0 || chunks[1] >= Enum.GetNames<NoteLane>().Length)
        {
            return null;
        }
        
        var lane = (NoteLane)chunks[1];
        
        if (type is NoteType.COP_SINGLE or NoteType.COP_HOLD or NoteType.COP_MASH)
        {
            // copy-paste format for cop notes:
            // type,lane,cop id,time from start,end time from start (or -1 for instant notes),
            // finisher
            if (chunks.Count != 6)
            {
                return null;
            }
            
            var copId = chunks[2];
            if (copId is < 1 or > 4)
            {
                return null;
            }
            
            // start time
            if (chunks[3] < 0)
            {
                return null;
            }
            time = chunks[3] + startTime;
                
            // finisher
            if (chunks[5] != 0 && chunks[5] != 1)
            {
                return null;
            }

            if (type == NoteType.COP_SINGLE)
            {
                return new CopNote(NoteType.COP_SINGLE, (int)copId, chunks[5] == 1)
                {
                    Time = time
                };
            }

            // end time
            if (chunks[4] < time)
            {
                return null;
            }
            
            return new CopNote(NoteType.COP_HOLD, (int)copId, chunks[5] == 1)
            {
                Time = time,
                EndTime = chunks[4] + startTime
            };
        }

        if (chunks.Count != 5)
        {
            return null;
        }
        
        // start time
        if (chunks[2] < 0)
        {
            return null;
        }
        time = chunks[2] + startTime;
        
        var flagString = Convert.ToString(chunks[4], 2).PadLeft(4, '0');
        if (flagString.Length != 4)
        {
            return null;
        }
        
        var flags = new NoteFlags(flagString[0] == '1', flagString[1] == '1', flagString[2] == '1',
                                  flagString[3] == '1');

        if (type is NoteType.HOLD or NoteType.DOUBLE or NoteType.MASH)
        {
            if ((chunks[3] + startTime) < time)
            {
                return null;
            }

            if (type is NoteType.HOLD or NoteType.DOUBLE)
            {
                return new HoldNote
                {
                    Time = time,
                    Lane = lane,
                    EndTime = chunks[3] + startTime,
                    Flags = flags
                };
            }

            return new MashNote
            {
                Time = time,
                // no lane because mashes are always in the center
                EndTime = chunks[3] + startTime,
                Flags = flags
            };
        }
        
        if (type is NoteType.SINGLE or NoteType.SPIKE)
        {
            return new SingleNote
            {
                Time = time,
                Lane = lane,
                Flags = flags
            };
        }

        if (type is NoteType.FREESTYLE or NoteType.NEGATIVE_MASH)
        {
            return new FreestyleNote
            {
                Time = time,
                // no lane because freestyles are always in the center
                Flags = flags
            };
        }
        
        // by process of elimination, any note that gets here must be a camera note
        return new CameraChange
        {
            Time = time,
            // no lane because camera notes are always in the camera lane
            Flags = flags
        };
    }

    protected NoteBase(NoteFlags? startingFlags = null)
    {
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

    public bool MouseOver()
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);
        return new Rect(x - 40, y - 12, 80, 24).ContainsPoint(ChartBuilder.MousePosition);
    }

    public virtual bool MouseOverTail()
    {
        var x = NoteViewer.GetNoteX(Lane);
        var startY = NoteViewer.TimeToScreenCoords(Time);
        var endY = NoteViewer.TimeToScreenCoords(EndTime);
        return new Rect(x - 16, startY, 32, endY - startY)
            .ContainsPoint(ChartBuilder.MousePosition);
    }
    public NoteBase Clone(long? newTime = null)
    {
        var clone = (NoteBase)MemberwiseClone();
        // deep copy the flags
        clone.Flags = Flags.Clone();
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
            Time.ToString()
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

        var paramString = Instant ? "" : $"{EndTime + Chart.Metadata.ChartOffset}:";
        if (Flags.N)
        {
            paramString += "1:0:0:0:";
        }
        else
        {
            paramString += "0:0:0:0:";
        }
        chunks.Add(paramString);

        return string.Join(",", chunks);
    }

    public virtual string ToCopyPasteString(long startTime)
    {
        // copy-paste format for non-cop notes:
        // type,lane,time from start,end time from start (or -1 for instant notes),flags as [cfwn]
        var typeId = (int)Type;
        var laneId = (int)Lane;
        var time = Time - startTime;
        var endTime = Instant ? -1 : EndTime - startTime;
        var flagNumber = Convert.ToInt32(
            $"{(Flags.C ? 1 : 0)}{(Flags.F ? 1 : 0)}{(Flags.W ? 1 : 0)}{(Flags.N ? 1 : 0)}", 2);
        return $"{typeId},{laneId},{time},{endTime},{flagNumber}";
    }

    protected void RenderFlags(DrawingContext dc, int x, double y, NoteFlags? flags = null)
    {
        if (flags == null || Config.Settings.AlwaysShowAllFlags)
        {
            flags = Flags;
        }

        var flagString =
            (flags.N ? "N" : "") +
            (flags.C ? "C" : "") +
            (flags.F ? "F" : "") +
            (flags.W ? "W" : "");
        
        var brush = (SolidColorBrush)App.Current.Resources["Notes.Common.FlagTextColor"];
        var outline =
            new Pen((SolidColorBrush)App.Current.Resources["Notes.Common.FlagTextOutlineColor"],
                    ((Thickness)
                        App.Current.Resources["Notes.Common.FlagTextOutlineThickness"]).Top);
        var text = new FormattedText(
            flagString, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface,
            (double)App.Current.Resources["Notes.Common.FlagTextSize"], brush);
        dc.DrawOutlinedText(text, new Point(x - text.Width / 2, y - 2 - text.Height / 2),
                            brush, outline);
    }

    protected void RenderDebugTime(DrawingContext dc, double x, double y, double tailY = -1)
    {
        if (!Config.Settings.DebugToggles.Enabled || !Config.Settings.DebugToggles.NoteTimeStamps)
        {
            return;
        }
        
        var brush =
            (SolidColorBrush)App.Current.Resources["DebugInfo.NoteTimestampTextColor"];
        var outline =
            new Pen((SolidColorBrush)
                    App.Current.Resources["DebugInfo.NoteTimestampTextOutlineColor"],
                    ((Thickness)
                        App.Current.Resources["DebugInfo.NoteTimestampTextOutlineThickness"]).Top);
        var text = new FormattedText(
            $"{Time}", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface,
            (double)App.Current.Resources["DebugInfo.NoteTimestampTextSize"], brush);
        dc.DrawOutlinedText(text, new Point(x - text.Width / 2, y - 14 - text.Height),
                            brush, outline);

        if (!Instant)
        {
            var tailText = new FormattedText(
                $"{EndTime}", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface,
                (double)App.Current.Resources["DebugInfo.NoteTimestampTextSize"], brush);
            dc.DrawOutlinedText(tailText, new Point(x - text.Width / 2, tailY - 14 - text.Height),
                                brush, outline);
        }
    }

    protected string GetFlagString()
    {
        var binaryString = $"{(Flags.C ? 1 : 0)}{(Flags.F ? 1 : 0)}{(Flags.W ? 1 : 0)}0";
        return Convert.ToInt32(binaryString, 2).ToString();
    }
}