using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Tmds.DBus.Protocol;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Audio;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;
using Path = System.IO.Path;

namespace UNBUGGABLE;

public enum DifficultySlot
{
    BEGINNER,
    NORMAL,
    HARD,
    EXPERT,
    UNBEATABLE,
    STAR
}

public static partial class Chart
{
    /// <summary>
    /// A large named label.
    /// </summary>
    public class Label(double time, string text)
    {
        public double Time => time;
        public string Text => text;
    }
    
    public class MetadataContainer
    {
        public string SongName = "";
        public string ArtistName = "";
        public string CoverArtistName = "";
        public string CharterName = "";
        public string FlavorText = "";
        public DifficultySlot DifficultySlot = DifficultySlot.BEGINNER;
        // this is supposed to only be used for star charts but using it for other difficulty slots
        // doesn't break the game, so have fun :)
        public string DifficultyName = "Beginner";
        public int DifficultyLevel = 0;
        public double ChartOffset = 0;
    }

    private static List<NoteBase> _notes = [];

    /// <summary>
    /// All notes in the chart, including markers.
    /// </summary>
    public static ReadOnlyCollection<NoteBase> Notes => _notes.AsReadOnly();

    public static ReadOnlyCollection<NoteBase> NonMarkerNotes =>
        _notes.Where(n => n is not MarkerDummyNote).ToList().AsReadOnly();
    
    public static ReadOnlyCollection<NoteBase> MarkerNotes =>
        _notes.Where(n => n is MarkerDummyNote).ToList().AsReadOnly();
    
    private static List<BpmRegion> _bpmRegions = [];
    public static ReadOnlyCollection<BpmRegion> BpmRegions => _bpmRegions.AsReadOnly();
    
    private static List<Label> _labels = [];
    public static ReadOnlyCollection<Label> Labels => _labels.AsReadOnly();
    
    private static MetadataContainer _metadata = new();
    public static MetadataContainer Metadata
    {
        get => _metadata;
        set
        {  
            // technically the chart can only be saved if it has a difficulty slot, but the way the
            // enum is set up makes it impossible to not have one
            var canSave = (value.SongName != "" && value.ArtistName != "" &&
                           value.CharterName != "");
            App.MainWindowViewModel.CanSave = canSave;
            if (canSave)
            {
                ChartFileName = GetChartFileName();
                Trace.WriteLine(ChartFileName);
            }

            if (_bpmRegions.Count != 0 && _bpmRegions[0].StartTime
                                                        .SoftNotEquals(value.ChartOffset))
            {
                _bpmRegions[0].StartTime = value.ChartOffset;

                if (_bpmRegions.Count > 1)
                {
                    foreach (var region in _bpmRegions.Skip(1))
                    {
                        region.StartTime -= _metadata.ChartOffset - value.ChartOffset;
                    }
                }
                
                RebuildSnapLineSets();
            }
            
            _metadata = value;
        }
    }

    public static string AudioFileName = "";
    public static int BeatSnap = 1;

    public static string ChartFolderName { get; private set; } = "";
    public static string ChartFileName { get; private set; } = "";
    
    public static double Length => _mediaPlayer.Media.Duration - AdjustedOffset;

    public static double AdjustedOffset => Metadata.ChartOffset + Config.HardChartOffset;

    private static double _currentTime = 0;
    public static double CurrentTime
    {
        get => _currentTime;
        private set
        {
            _currentTime = value;
            
            foreach (var region in _bpmRegions)
            {
                if (CurrentTime >= region.StartTime && CurrentTime <= region.EndTime)
                {
                    App.MainWindowViewModel.SongBpmText = region.Bpm.ToString("0.00");
                }
            }

            if (!Playing)
            {
                App.MainWindowViewModel.UpdatePriorityListEntries(GetNotesAtTime(CurrentTime));
                // _songPlayer.Time = value + AdjustedOffset;
            }
        }
    }

    private static bool _songLoaded = false;
    public static bool SongLoaded
    {
        get => _songLoaded;
        private set
        {
            _songLoaded = value;
            App.MainWindowViewModel.SongLoaded = value;
            App.MainWindowViewModel.EditorUiEnabled = true;
        }
    }
    
    private static int _songVolume = 100;
    public static int SongVolume
    {
        get => _songVolume;
        set
        {
            _songVolume = value;
            if (SongLoaded)
            {
                _mediaPlayer.Volume = value;
                //Trace.WriteLine($"song volume changed to {value}");
            }
        }
    }
    
    private static int _sfxVolume = 100;
    public static int SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = value;
            SfxEngine.Volume = value / 100.0f;
            //Trace.WriteLine($"sfx volume changed to {value}");
        }
    }
    
    public static int PlaySpeed
    {
        get => (int)(_mediaPlayer.Rate * 100);
        set
        {
            if (SongLoaded)
            {
                _mediaPlayer.SetRate(value / 100.0f);
            }
            // Trace.WriteLine($"Play speed changed to {value}");
        }
    }

    private static bool _playing = false;
    public static bool Playing
    {
        get => _playing;
        private set
        {
            _playing = value;
            App.MainWindowViewModel.EditorUiEnabled = !value && SongLoaded;
            if (value)
            {
                App.MainWindowViewModel.ClearPriorityListEntries();
            }
        }
    }

    private static LibVLC _libVlc = null!;
    private static MediaPlayer _mediaPlayer = null!;

    private static MediaPlayer _hitSoundMediaPlayer = null!;
    // private static ChartSongPlayer? _songPlayer = null!;
    
    private static CachedSound? _hitSound = null;
    
    // used for keeping track of the song's actual play position
    private static Stopwatch _stopwatch = null!;

    // private static double _lastSongTime = 0;
    private static double _lastStopwatchTime = 0;
    // no field for camera swaps because camera swaps do nothing if you give them a duration
    
    private static int _beatSnapIndex = 0;
    
    // timestamps for where every line appears for every snap setting, updated whenever bpm regions
    // change
    private static readonly Dictionary<int, List<double>> SnapLineSets = new();
    private static List<double> _currentSnapLineSet = [];
    private static int _currentSnapLineSetIndex = 0;
    
    [GeneratedRegex(@"\d+,\d+.\d+,\d+,\d+,\d+,\d+,\d+,\d+")]
    private static partial Regex TimingPointRegex();

    [GeneratedRegex(@".*\[(.+)\].*")]
    private static partial Regex DifficultySlotRegex();

    // cursed regex because chart tags allow double quotes in a string so i can't just use json
    [GeneratedRegex("""{"Level":(\d+),"FlavorText":"(.*)","SongLength":(?:\d+?(?:\.\d+)),"CoverArt":"(.*)"}""")]
    private static partial Regex TagRegex();

    private static double _temp = 0;

    /// <summary>
    /// Initializes everything. This must be called before any other methods are used!
    /// </summary>
    public static void Init()
    {
        _libVlc = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVlc);
        _hitSoundMediaPlayer = new MediaPlayer(_libVlc);
        _mediaPlayer.EndReached += MediaPlayer_EndReached;
        
        try
        {
            _hitSound = new CachedSound(
                Path.Combine(Environment.CurrentDirectory, "Assets/hitSound.wav"));
        }
        catch (Exception e)
        {
            if (e is FileNotFoundException or DirectoryNotFoundException)
            {
                _hitSound = null;
                Trace.WriteLine(
                    "Hit sound (Assets/hitSound.wav) not found. Hit sounds are disabled.");
            }
            else
            {
                throw;
            }
        }
        
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public static async void AsyncInit()
    {
        try
        {
            var hitSoundMedia =
                new Media(_libVlc,
                          Path.Combine(Environment.CurrentDirectory, "Assets/hitSound.wav"));
            _hitSoundMediaPlayer.Media = hitSoundMedia;
            await hitSoundMedia.Parse();
        }
        catch (FileNotFoundException)
        {
            _hitSoundMediaPlayer.Media = null;
            Trace.WriteLine(
                "Hit sound (Assets/hitSound.wav) not found. Hit sounds are disabled.");
        }
    }

    public static void PlayOrPauseSong()
    {
        if (SongLoaded)
        {
            if (Playing)
            {
                PauseSong();
            }
            else
            {
                PlaySong();
            }
        }
    }

    public static void SetBeatSnapIndex(int index)
    {
        if (index < 0 || index >= Config.BeatSnaps.Count)
        {
            throw new ArgumentOutOfRangeException($"Beat snap index {index} out of range.");
        }
        
        _beatSnapIndex = index;
        BeatSnap = Config.BeatSnaps[_beatSnapIndex];
        App.MainWindow.BeatSnapText.Text = BeatSnap.ToString();
        _currentSnapLineSet = SnapLineSets[BeatSnap];
        Trace.WriteLine(BeatSnap);
        SetTimeToNearestSnap();
    }
    
    public static void IncreaseBeatSnap()
    {
        ++_beatSnapIndex;
        if (_beatSnapIndex >= Config.BeatSnaps.Count)
        {
            _beatSnapIndex = 0;
        }
        SetBeatSnapIndex(_beatSnapIndex);
    }
    
    public static void DecreaseBeatSnap()
    {
        --_beatSnapIndex;
        if (_beatSnapIndex < 0)
        {
            _beatSnapIndex = Config.BeatSnaps.Count - 1;
        }
        SetBeatSnapIndex(_beatSnapIndex);
    }

    public static void MoveToPreviousSnap()
    {
        if (_currentSnapLineSetIndex > 0)
        {
            --_currentSnapLineSetIndex;
            CurrentTime = _currentSnapLineSet[_currentSnapLineSetIndex];
        }
    }

    public static void MoveToNextSnap()
    {
        if (_currentSnapLineSetIndex < _currentSnapLineSet.Count - 1)
        {
            ++_currentSnapLineSetIndex;
            CurrentTime = _currentSnapLineSet[_currentSnapLineSetIndex];
        }
    }

    public static double GetPreviousSnapTime() => _currentSnapLineSetIndex > 0 ?
        _currentSnapLineSet[_currentSnapLineSetIndex - 1] : _currentSnapLineSet[0];
    
    public static double GetNextSnapTime() =>
        _currentSnapLineSetIndex < _currentSnapLineSet.Count - 1 ?
        _currentSnapLineSet[_currentSnapLineSetIndex + 1] : _currentSnapLineSet[^1];

    public static void MoveToNextLabel()
    {
        if (CurrentTime >= Length)
        {
            return;
        }
        
        var nextLabel = _labels.FirstOrDefault(
            l => l.Time > _currentTime && Math.Abs(l.Time - CurrentTime) > 1000);
        if (nextLabel != null)
        {
            CurrentTime = nextLabel.Time;
        }
        else
        {
            var lastNote = NonMarkerNotes[^1];
            CurrentTime = (lastNote.Instant ? lastNote.Time : lastNote.EndTime);
        }
        
        SetTimeToNearestSnap();
    }
    
    public static void MoveToPreviousLabel()
    {
        if (CurrentTime <= 0)
        {
            return;
        }
        
        var previousLabel = _labels.LastOrDefault(
            l => l.Time < _currentTime && Math.Abs(l.Time - CurrentTime) > 1000);
        if (previousLabel != null)
        {
            CurrentTime = previousLabel.Time;
        }
        else
        {
            CurrentTime = 0;
        }
        
        SetTimeToNearestSnap();
    }

    /// <summary>
    /// Called once per frame, used to update chart time while the song is "playing" but the song
    /// time is actually negative due to offset.
    /// </summary>
    public static void PerTickUpdate()
    {
        if (SongLoaded && Playing)
        {
            var prevTime = CurrentTime;
            CurrentTime += (_stopwatch.ElapsedMilliseconds - _lastStopwatchTime) * PlaySpeed / 100;
            if (CurrentTime + AdjustedOffset >= 0 && !_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.SeekTo(new TimeSpan(0));
                _mediaPlayer.Play();
                CurrentTime = AdjustedOffset;
            }
            
            foreach (var note in Notes)
            {
                if (note.ShouldPlayHitSound(prevTime - Config.HitSoundOffset,
                                            CurrentTime - Config.HitSoundOffset) is { } offset)
                {
                    if (_hitSound != null)
                    {
                        Trace.WriteLine($"play hit sound, {note.Time}, {CurrentTime}");
                        SfxEngine.Play(_hitSound, offset);
                    }
                    break;
                }
            }
        }

        // _temp += _stopwatch.ElapsedMilliseconds - _lastStopwatchTime;
        //
        // if (_temp >= 500)
        // {
        //     _temp -= 500;
        //     SfxEngine.Play(_hitSound);
        // }
        
        _lastStopwatchTime = _stopwatch.ElapsedMilliseconds;
        
    }

    public static void PlayHitSound()
    {
        if (_hitSound != null)
        {
            SfxEngine.Play(_hitSound, 0);
        }
    }
    
    /// <summary>
    /// Attempts to load a .wav or .mp3 file and create a new chart with empty metadata.
    /// </summary>
    /// <returns>Whether the audio file could be loaded.</returns>
    public static async Task<bool> TryCreateChartFromAudio(string path)
    {
        SongLoaded = false;

        if (!(await TryLoadAudioFile(path)))
        {
            return false;
        }
        
        Trace.WriteLine("Creating chart from audio file...");
        
        App.MainWindow.PlaySpeedSlider.Value = PlaySpeed;
        
        Metadata.SongName = "";
        Metadata.ArtistName = "";
        Metadata.CoverArtistName  = "";
        Metadata.DifficultySlot = DifficultySlot.BEGINNER;
        Metadata.DifficultyLevel = 0;
        Metadata.DifficultyName = "Beginner";
        Metadata.FlavorText = "";
        Metadata.CharterName = "";
        Metadata.ChartOffset = 0;
        _notes = [];
        ChartBuilder.ClearSelection();
        ChartBuilder.DeleteBreakpoint(false);
        _labels = [];
            
        _bpmRegions = [new BpmRegion(0, 60)];
        RebuildSnapLineSets();
        SetBeatSnapIndex(0);
        SetTimeToNearestSnap();

        NoteViewer.SetZoom(1.0);
        CurrentTime = 0;
        ChartFileName = "";
        ChartFolderName = Path.GetFileName(Path.GetDirectoryName(path));
            
        App.MainWindowViewModel.SongBpmText = _bpmRegions[0].Bpm.ToString("0.000");
        App.MainWindowViewModel.PlaySpeed = 100;
        App.MainWindowViewModel.CanSave = false;

        SongLoaded = true;
        UserData.LastOpenedChartFile = ""; 
        return true;
    }

    /// <summary>
    /// Tries to load a chart file (either a standard .txt file from the official editor or an
    /// UNBUGGABLE .beat.txt file) and set up notes, markers, BPM changes, etc.
    /// </summary>
    /// <returns>Whether all data could be loaded.</returns>
    public static async Task<bool> TryLoadChartFile(string path)
    {
        SongLoaded = false;
        Trace.WriteLine($"Loading chart file: {path}");
        if (!File.Exists(path))
        {
            Trace.WriteLine("File not found.");
            return false;
        }
        
        var dirName = Path.GetFullPath(path);
        var folderPath = dirName[..dirName.LastIndexOf('\\')];
        var chartData = (await File.ReadAllTextAsync(path)).Split("\n");
        string? audioPath = null;
        (double, int, double)? lastEditorState = null;
        
        Metadata = new MetadataContainer();
        _labels = [];
        _notes = [];
        ChartBuilder.ClearSelection();
        _bpmRegions = [];
        for (var i = 0; i < chartData.Length; i++)
        {
            var line = chartData[i].Trim();
            var temp = 0;
            switch (line)
            {
                case "[General]":
                    Trace.WriteLine("Parsing general data...");
                    temp = TryParseGeneralChartData(chartData, i, folderPath, out audioPath);
                    break;
                case "[Editor]":
                    Trace.WriteLine("Parsing official editor data...");
                    temp = TryParseOfficialEditorData(chartData, i);
                    break;
                case "[UNBUGGABLE]":
                    Trace.WriteLine("Parsing UNBUGGABLE data...");
                    temp = TryParseUnbuggableData(chartData, i, out lastEditorState);
                    break;
                case "[Metadata]":
                    Trace.WriteLine("Parsing metadata...");
                    temp = TryParseMetadata(chartData, i);
                    // see??? do you see how easy it would be to make the official editor save star
                    // charts correctly??? why would you not do this???
                    Metadata.DifficultySlot =
                        DifficultySlotRegex().Match(path).Groups[1].Value switch
                    {
                        "Beginner" => DifficultySlot.BEGINNER,
                        "Normal" => DifficultySlot.NORMAL,
                        "Hard" => DifficultySlot.HARD,
                        "Expert" => DifficultySlot.EXPERT,
                        "UNBEATABLE" => DifficultySlot.UNBEATABLE,
                        _ => DifficultySlot.STAR
                    };
                    break;
                // there are also [Difficulty] and [Events] sections here but they do nothing
                case "[TimingPoints]":
                    Trace.WriteLine("Parsing timing points...");
                    temp = TryParseTimingPoints(chartData, i);
                    break;
                case "[HitObjects]":
                    Trace.WriteLine("Parsing hit objects (notes)...");
                    temp = TryParseHitObjects(chartData, i);
                    break;
            }
            
            if (temp == -1)
            {
                _mediaPlayer.Media = null; // disables the editor
                return false;
            }

            i += temp;
        }

        if (await TryLoadAudioFile(audioPath))
        {
            RebuildSnapLineSets();
            SetBeatSnapIndex(0);
            ChartFileName = GetChartFileName();
            ChartFolderName = Directory.GetParent(path)?.Name ?? "";
            SongLoaded = true;
            UserData.LastOpenedChartFile = path;

            // last editor state never makes the load fail (and the data for it may not even exist)
            if (lastEditorState != null)
            {
                var time = lastEditorState.Value.Item1;
                var beatSnap = lastEditorState.Value.Item2;
                var zoom = lastEditorState.Value.Item3;

                if (time >= 0 && time <= Length)
                {
                    CurrentTime = time;
                }

                Trace.WriteLine(
                    $"Restoring last editor state: {time} ms, snap {beatSnap}, {zoom}x zoom");
                for (var i = 0; i < Config.BeatSnaps.Count; ++i)
                {
                    if (beatSnap == Config.BeatSnaps[i])
                    {
                        SetBeatSnapIndex(i);
                        break;
                    }
                }

                if (zoom >= Config.MinZoom && zoom <= Config.MaxZoom)
                {
                    NoteViewer.SetZoom(zoom);
                }
            }

            ChartBuilder.CheckExistingBreakpoint();
            SetTimeToNearestSnap();
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Saves the chart to a .beat.txt with extra UNBUGGABLE data.
    /// </summary>
    public static async Task SaveToBeatPath(string path)
    {
        await using var writer = new StreamWriter(path, false);
        writer.AutoFlush = true;
        await writer.WriteLineAsync("// Output from NPC's UNBUGGABLE editor\n" +
                                    "// based on TaroNuke's unity editor");
        
        await writer.WriteLineAsync("");
        await WriteGeneralChartData(writer);
        
        await writer.WriteLineAsync("");
        await WriteOfficialEditorData(writer);
        
        await writer.WriteLineAsync("");
        await WriteUnbuggableData(writer);
        
        await writer.WriteLineAsync("");
        await WriteMetadata(writer);
        
        // for some reason there are just some random empty sections here (probably from osu)
        await writer.WriteLineAsync("\n[Difficulty]\n\n[Events]\n");
        
        await WriteTimingPoints(writer);
        
        await writer.WriteLineAsync("");
        await WriteHitObjects(writer, false);
        
        UserData.LastOpenedChartFile = path;
    }
    
    /// <summary>
    /// Saves the chart to a standard .txt file.
    /// </summary>
    public static async Task SaveToStandardPath(string path)
    {
        await using var writer = new StreamWriter(path, false);
        writer.AutoFlush = true;
        await writer.WriteLineAsync("// Output from NPC's UNBUGGABLE editor\n" +
                                    "// based on TaroNuke's unity editor\n");
        
        await writer.WriteLineAsync("");
        await WriteGeneralChartData(writer);
        
        await writer.WriteLineAsync("");
        await WriteOfficialEditorData(writer);
        
        await writer.WriteLineAsync("");
        await WriteMetadata(writer);
        
        await writer.WriteLineAsync("");
        await WriteTimingPoints(writer);
        
        await writer.WriteLineAsync("");
        await WriteHitObjects(writer, true);
        
        UserData.LastOpenedChartFile = path;
    }

    /// <summary>
    /// Returns the timestamps (in milliseconds) of every single full beat in some range.
    /// </summary>
    public static IEnumerable<double> GetBeatTimesInRange(double start, double end)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, Length);
        foreach (var beatTime in SnapLineSets[1]) 
        {
            if (beatTime >= start && beatTime <= end)
            {
                yield return beatTime;
            }
        }
    }
    
    /// <summary>
    /// Returns every timestamp (in milliseconds) in some range that can be snapped to.
    /// </summary>
    public static IEnumerable<double> GetSnapTimesInRange(double start, double end)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, Length);
        foreach (var beatTime in _currentSnapLineSet) 
        {
            if (beatTime >= start && beatTime <= end)
            {
                yield return beatTime;
            }
        }
    }

    /// <summary>
    /// Adds a marker to the chart if one doesn't already exist at that time. This is only used
    /// during chart file loading.
    /// </summary>
    /// <param name="time">The time of the marker, in milliseconds.</param>
    /// <param name="type">From 0-2, determines the color of the marker. The default types are
    ///     green, yellow, and purple, in that order.</param>
    public static void TryAddMarker(double time, int type)
    {
        if (GetNotesAtTime(time).Find(note => note.Item1 is MarkerDummyNote)
                                .Item1 is not MarkerDummyNote)
        {
            AddNote(new MarkerDummyNote(time, type));
        }
    }

    /// <summary>
    /// Returns a list of every non-marker note that exists at a timestamp (in milliseconds). List
    /// elements are formatted as <c>(note, index in the main note list)</c>.
    /// </summary>
    public static List<(NoteBase, int)> GetNotesAtTime(double time)
    {
        List<(NoteBase, int)> notes = [];
        foreach (NoteBase note in NonMarkerNotes)
        {
            if (note.Time.SoftEquals(time))
            {
                notes.Add((note, NonMarkerNotes.IndexOf(note)));
            }
            else if (note.Time > time)
            {
                break;
            }
        }
        return notes;
    }
    
    /// <summary>
    /// Returns the note in a specific lane at a specific time, or null if that note does not exist.
    /// </summary>
    public static NoteBase? GetNote(double time, NoteLane lane)
        => _notes.FirstOrDefault(n => n.Time.SoftEquals(time) && n.Lane == lane);
    
    /// <summary>
    /// Returns the (non-instant) note in a specific lane that <i>ends</i> at a specific time, or
    /// null if that note does not exist.
    /// </summary>
    public static NoteBase? GetNoteFromEnd(double time, NoteLane lane) =>
        _notes.FirstOrDefault(n => !n.Instant && n.EndTime.SoftEquals(time) && n.Lane == lane);

    public static NoteBase? GetPreviousNote(NoteBase note)
    {
        var index = NonMarkerNotes.IndexOf(note);
        return index > 0 ? NonMarkerNotes[index - 1] : null;
    }
    
    public static NoteBase? GetNextNote(NoteBase note)
    {
        var index = NonMarkerNotes.IndexOf(note);
        return index < NonMarkerNotes.Count - 1 ? NonMarkerNotes[index + 1] : null;
    }
    
    public static int GetNoteIndex(NoteBase note) => Notes.IndexOf(note);

    /// <summary>
    /// Returns all the notes between a start and end time.
    /// </summary>
    /// <param name="lanes">Restricts the region to notes only in certain lanes. Omit this to
    ///                     get notes in every lane (except markers).</param>
    /// <returns></returns>
    public static List<NoteBase> GetNoteRegion(double start, double end,
        List<NoteLane>? lanes = null)
    {
        lanes ??= [NoteLane.TOP, NoteLane.BOTTOM, NoteLane.CENTER, NoteLane.CAMERA];
        return _notes.Where(n => n.Time >= start && n.Time <= end && lanes.Contains(n.Lane))
                     .ToList();
    }

    /// <summary>
    /// Adds a note. If one or more notes already exist at that timestamp, the new note will be
    /// placed in the note list after the existing notes.
    /// </summary>
    public static void AddNote(NoteBase note)
    {
        if (_notes.Count == 0 || _notes[^1].Time <= note.Time)
        {
            _notes.Add(note);
        }
        else if (_notes[0].Time > note.Time)
        {
            _notes.Insert(0, note);
        }
        else
        {
            var i = _notes.FindIndex(x => x.Time > Math.Round(note.Time));
            _notes.Insert(i, note);
        }
        
        App.MainWindowViewModel.UpdatePriorityListEntries(GetNotesAtTime(CurrentTime));
    }

    public static void RemoveNote(NoteBase note)
    {
        _notes.Remove(note);
        App.MainWindowViewModel.UpdatePriorityListEntries(GetNotesAtTime(CurrentTime));
    }
    
    /// <summary>
    /// Replaces a note, preserving placement priority.
    /// </summary>
    public static void ReplaceNote(NoteBase oldNote, NoteBase newNote)
    {
        _notes[_notes.IndexOf(oldNote)] = newNote;
        App.MainWindowViewModel.UpdatePriorityListEntries(GetNotesAtTime(CurrentTime));
    }

    /// <summary>
    /// Sets which note is at a specific index the note list.
    /// </summary>
    public static void SetNoteOrder(List<NoteBase> notes)
    {
        List<int> indices = [];
        foreach (var note in notes)
        {
            indices.Add(_notes.IndexOf(note));
        }

        indices.Sort();
        for (var i = 0; i < notes.Count; ++i)
        {
            _notes[indices[i]] = notes[i];
        }
    }

    /// <summary>
    /// Returns the label at a specific time, or null if it doesn't exist.
    /// </summary>
    public static Label? GetLabel(double time) =>
        _labels.FirstOrDefault(x => x.Time.SoftEquals(time));
    
    public static void AddLabel(Label label)
    {
        if (_labels.Count == 0 || _labels[^1].Time <= label.Time)
        {
            _labels.Add(label);
        }
        else if (_labels[0].Time > label.Time)
        {
            _labels.Insert(0, label);
        }
        else
        {
            _labels.Insert(_labels.FindIndex(x => x.Time > label.Time), label);
        }
    }

    public static void RemoveLabel(Label label)
    {
        _labels.Remove(label);
    }

    /// <summary>
    /// Returns the BPM region that starts at a specific time, or null if it doesn't exist.
    /// </summary>
    public static BpmRegion? GetBpmRegion(double time)
    {
        foreach (var region in _bpmRegions)
        {
            if (region.StartTime.SoftEquals(time))
            {
                return region;
            }
            
            if (region.StartTime > time)
            {
                return null;
            }
        }

        return null;
    }

    public static void AddBpmRegion(BpmRegion region)
    {
        if (_bpmRegions[0].StartTime > region.StartTime)
        {
            _bpmRegions.Insert(0, region);
            region.Next = _bpmRegions[1];
            _bpmRegions[1].Previous = region;
        }
        else if (_bpmRegions[^1].StartTime < region.StartTime)
        {
            _bpmRegions.Add(region);
            _bpmRegions[^2].Next = region;
            region.Previous = _bpmRegions[^2];
        }
        else
        {
            for (var i = 0; i < _bpmRegions.Count; ++i)
            {
                if (_bpmRegions[i].StartTime < region.StartTime &&
                    _bpmRegions[i + 1].StartTime > region.StartTime)
                {
                    _bpmRegions.Insert(i + 1, region);
                    region.Next = _bpmRegions[i + 2];
                    _bpmRegions[i - 1].Next = region;
                    region.Previous = _bpmRegions[i - 1];
                    _bpmRegions[i + 2].Previous = region;
                    break;
                }
            }
        }
        
        RebuildSnapLineSets();
    }

    public static void RemoveBpmRegion(BpmRegion region)
    {
        if (region.Next != null)
        {
            region.Next.Previous = region.Previous;
        }

        if (region.Previous != null)
        {
            region.Previous.Next = region.Next;
        }

        _bpmRegions.Remove(region);
        RebuildSnapLineSets();
    }

    public static void EditBpmRegion(BpmRegion region, double newBpm)
    {
        if (newBpm <= 0)
        {
            throw new InvalidOperationException("Bpm must be positive");
        }
        
        region.Bpm = newBpm;
        RebuildSnapLineSets();
    }

    /// <summary>
    /// Recalculates where every snap line is for every bpm setting. This must be called any time
    /// that a BPM region changes.
    /// </summary>
    public static void RebuildSnapLineSets()
    {
        var sortedSnapValues = Config.BeatSnaps.OrderByDescending(x => x).ToList();
        foreach (var snapValue in sortedSnapValues)
        {
            List<double> snapLineSet = [0];
            double time = 0;
            var bpmRegion = _bpmRegions[0];
            while (time <= Length)
            {
                var nextTime = time + (bpmRegion.MsPerBeat / snapValue);
                // handle bpm changes between snap lines
                if (nextTime >= bpmRegion.EndTime)
                {
                    if (bpmRegion.Next == null)
                    {
                        break;
                    }
                        
                    var snapFractionBeforeRegion =
                        (bpmRegion.EndTime - time) / (bpmRegion.MsPerBeat / snapValue);
                    var snapFractionAfterRegion = 1 - snapFractionBeforeRegion;
                    nextTime = (bpmRegion.EndTime +
                                     bpmRegion.Next.MsPerBeat / snapValue *
                                     snapFractionAfterRegion);
                    bpmRegion = bpmRegion.Next;
                }
                time = nextTime;
                snapLineSet.Add(time);
            }
            
            Trace.WriteLine(
                $"Snap line set for snap value {snapValue} has {snapLineSet.Count} lines");
            SnapLineSets[snapValue] = snapLineSet;
        }
        
        _currentSnapLineSet = SnapLineSets[BeatSnap];
        SetTimeToNearestSnap();
    }

    private static void PlaySong()
    {
        if (CurrentTime + AdjustedOffset >= Length)
        {
            return;
        }
        
        Trace.WriteLine(_mediaPlayer.Media.State);
        Playing = true;
        if (CurrentTime + AdjustedOffset >= 0)
        {
            if (_mediaPlayer.Media.State == VLCState.Ended)
            {
                _mediaPlayer.Play(_mediaPlayer.Media);
            }
            else
            {
                _mediaPlayer.Play();
            }
            _mediaPlayer.SeekTo(TimeSpan.FromMilliseconds(CurrentTime + AdjustedOffset));
        }
    }
    
    private static void PauseSong()
    {
        Playing = false;
        _mediaPlayer.Pause();
        SetTimeToNearestSnap();
        Trace.WriteLine(_mediaPlayer.Time);
        //Trace.WriteLine($"PauseSong: {CurrentTime}, {_mediaPlayer.Time}");
    }

    private static void MediaPlayer_EndReached(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            PauseSong();
            // throw this in a separate thread because otherwise it'll block itself and freeze
            // ThreadPool.QueueUserWorkItem(_ =>
            // {
            //     PauseSong();
            //     // _mediaPlayer.SeekTo(new TimeSpan(0));
            //     // _mediaPlayer.Play();
            // });
        });
    }

    /// <summary>
    /// Parses metadata and returns the suggested file name (without extension) for the chart,
    /// accounting for Unicode characters and brackets/parentheses.
    /// </summary>
    private static string GetChartFileName()
    {
        return $"{SanitizeString(Metadata.ArtistName)} - {SanitizeString(Metadata.SongName)} " +
               $"({SanitizeString(Metadata.CharterName)}) " +
               $"[{SanitizeString(Metadata.DifficultySlot.ToString())}]";
    }

    private static string SanitizeString(string str)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var pattern = "[" + Regex.Escape(new string(invalid)) + @"]|[^\x00-\x7F]";
        return Regex.Replace(str, pattern, "_").Replace("(", "_").Replace(")", "_")
                    .Replace("[", "_").Replace("]", "_");
    }
    
    private static async Task<bool> TryLoadAudioFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                Trace.WriteLine($"Could not load audio file: File not found or invalid format.");
                return false;
            }
            
            var media = new Media(_libVlc, path);
            _mediaPlayer.Media = media;
            _mediaPlayer.SeekTo(TimeSpan.FromMilliseconds(-AdjustedOffset));
            // DisposeSongPlayer();
            // _songPlayer = new ChartSongPlayer(path);
            AudioFileName = Path.GetFileName(path);
            
            await media.Parse();
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Could not load audio file: {e.Message}");
            return false;
        }
        
        return true;
    }
    
    private static void SetTimeToNearestSnap()
    {
        // Trace.WriteLine($"{_currentSnapLineSetIndex} {_currentSnapLineSet.Count}");
        for (var i = 0; i < _currentSnapLineSet.Count - 1; ++i)
        {
            var currentSnap = _currentSnapLineSet[i];
            var nextSnap = _currentSnapLineSet[i + 1];
            if (CurrentTime >= currentSnap && CurrentTime <= nextSnap)
            {
                CurrentTime = currentSnap;
                _currentSnapLineSetIndex = i;
                return;
            }
        }
    }

    private static int TryParseGeneralChartData(string[] lines, int index, string folderPath,
        out string? audioPath)
    {
        if (lines[index + 1].StartsWith("AudioFilename:"))
        {
            AudioFileName = lines[index + 1]["AudioFilename: ".Length..].Trim();
            audioPath = Path.GetFullPath($"{folderPath}/{AudioFileName}");
            Trace.WriteLine($"Audio file path: {audioPath}");
            return 1;
        }

        audioPath = null;
        return -1;
    }

    private static int TryParseOfficialEditorData(string[] lines, int index)
    {
        // technically theres a "Bookmarks" line above this one, but it just stores the timestamp of
        // every label without the text
        if (lines[index + 2].StartsWith("BookmarksPlus:"))
        {
            Trace.WriteLine("Parsing labels...");
            var labelData = lines[index + 2]["BookmarksPlus: ".Length..].Trim().Split(',');
            foreach (var label in labelData)
            {
                var split = label.Split('`');
                if (split.Length != 2)
                {
                    break;
                }
                if (double.TryParse(split[0], out var time))
                {
                    Trace.WriteLine($"Adding label at {time} with text \"{split[1]}\"");
                    _labels.Add(new Label(time, split[1]));
                }
                else
                {
                    return -1;
                }
            }
            return 3;
        }

        // editor data will be empty if there are no labels
        return 1;
    }
    
    private static int TryParseUnbuggableData(string[] lines, int index,
        out (double, int, double)? lastEditorState)
    {
        lastEditorState = null;

        var i = 1;
        if (lines[index + i].StartsWith("LastEditorState:"))
        {
            var split = lines[index + 1]["LastEditorState:".Length..].Trim().Split(',');
            if (split.Length == 3 && double.TryParse(split[0], out var time) &&
                int.TryParse(split[1], out var beatSnap) &&
                double.TryParse(split[2], out var zoom))
            {
                lastEditorState = (time, beatSnap, zoom);
            }

            ++i;
        }

        if (lines[index + i].StartsWith("Markers:"))
        {
            ++i;
            for (; index + i < lines.Length; ++i)
            {
                if (lines[index + i] == "" || lines[index + i] == "\r" ||
                    lines[index + i] == "\n" || lines[index + i] == "\r\n")
                {
                    break;
                }

                var markerData = lines[index + i].Trim().Split(',');
                foreach (var marker in markerData)
                {
                    var split = marker.Split('`');
                    if (double.TryParse(split[0], out var time))
                    {
                        Trace.WriteLine($"Adding marker at {time} with type {split[1]}");
                        TryAddMarker(time, int.Parse(split[1]));
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return i;
    }
    
    private static int TryParseMetadata(string[] lines, int index)
    {
        var hasTitle = false;
        var hasArtist = false;
        var hasCharterName = false;
        var hasDifficulty = false;
        var hasLevelTag = false;
        var hasFlavorTextTag = false;
        var hasCoverArtTag = false;
        Metadata = new MetadataContainer();
        var i = 1;
        for (; index + i < lines.Length; i++)
        {
            var line = lines[index + i].Trim();
            if (line.StartsWith("TitleUnicode:"))
            {
                Metadata.SongName = line["TitleUnicode:".Length..].Trim();
                Trace.WriteLine($"Song name: {Metadata.SongName}");
                hasTitle = true;
            }
            else if (line.StartsWith("ArtistUnicode:"))
            {
                Metadata.ArtistName = line["ArtistUnicode:".Length..].Trim();
                Trace.WriteLine($"Artist name: {Metadata.ArtistName}");
                hasArtist = true;
            }
            else if (line.StartsWith("Creator:"))
            {
                Metadata.CharterName = line["Creator:".Length..].Trim();
                Trace.WriteLine($"Charter name: {Metadata.CharterName}");
                hasCharterName = true;
            }
            else if (line.StartsWith("Version:"))
            {
                // the version is only used for the in-game difficulty name; difficulty slot is
                // determined by the filename
                Metadata.DifficultyName = line["Version:".Length..].Trim();
                Trace.WriteLine($"Difficulty name: {Metadata.DifficultyName}");
                hasDifficulty = true;
            }
            else if (line.StartsWith("Tags:"))
            {
                try
                {
                    var match = TagRegex().Match(line);
                    if (match.Success)
                    {
                        Metadata.DifficultyLevel = int.Parse(match.Groups[1].Value);
                        Trace.WriteLine($"Difficulty level: {Metadata.DifficultyLevel}");
                        hasLevelTag = true;
                        
                        Metadata.FlavorText = Regex.Unescape(match.Groups[2].Value);
                        Trace.WriteLine($"Flavor text: {Metadata.FlavorText}");
                        hasFlavorTextTag = true;
                        
                        Metadata.CoverArtistName = Regex.Unescape(match.Groups[3].Value);
                        Trace.WriteLine($"Cover artist: {Metadata.CoverArtistName}");
                        hasCoverArtTag = true;
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"Could not parse tags: {e.Message}");
                }
            }
            // i have no idea what the difference between Title/Artist and
            // TitleUnicode/ArtistUnicode is, but AFAIK they are always the same
            else if (!line.StartsWith("Title:") && !line.StartsWith("Artist:"))
            {
                break;
            }
        }

        if (!hasTitle || !hasArtist || !hasCharterName || !hasDifficulty || !hasLevelTag ||
            !hasFlavorTextTag || !hasCoverArtTag)
        {
            var errorMessage = new StringBuilder(
                "Chart is missing one or more required metadata fields (or their values were  +" +
                "invalid): ");
            if (!hasTitle)
            {
                errorMessage.Append("Title, ");
            }

            if (!hasArtist)
            {
                errorMessage.Append("Artist, ");
            }

            if (!hasCharterName)
            {
                errorMessage.Append("Charter Name, ");
            }

            if (!hasDifficulty)
            {
                errorMessage.Append("Difficulty name (uses the Version field), ");
            }

            if (!hasLevelTag)
            {
                errorMessage.Append("Difficulty level (in the Tags object), ");
            }
            
            if (!hasFlavorTextTag)
            {
                errorMessage.Append("Flavor text (in the Tags object), ");
            }

            if (!hasCoverArtTag)
            {
                errorMessage.Append("Cover artist (in the Tags object), ");
            }

            Trace.WriteLine(errorMessage.ToString());
            return -1;
        }
        
        App.MainWindowViewModel.SongNameText = Metadata.SongName;
        App.MainWindowViewModel.ArtistNameText = Metadata.ArtistName;
        App.MainWindowViewModel.DifficultyText = $"{Metadata.DifficultyName} " +
                                                 $"{Metadata.DifficultyLevel}";
        App.MainWindowViewModel.CanSave = (_metadata.SongName != "" &&
                                           _metadata.ArtistName != "" &&
                                           _metadata.CharterName != "");
        return i;
    }
    
    private static int TryParseTimingPoints(string[] lines, int index)
    {
        var i = 1;
        _bpmRegions = [];
        for (; index + i < lines.Length; i++)
        {
            var line = lines[index + i].Trim();
            if (TimingPointRegex().IsMatch(line))
            {
                // timing points have 8 numbers, but most of them are osu!-specific and UNBEATABLE
                // only uses the first two
                var numbers = line.Split(',').ToList();
                double regionStart;
                double msPerBeat;
                try
                {
                    regionStart = double.Parse(numbers[0]);
                    msPerBeat = double.Parse(numbers[1]);
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"Could not parse timing point: {e.Message}");
                    return -1;
                }
                
                // the start time of the first region determines chart offset
                if (_bpmRegions.Count == 0)
                {
                    Metadata.ChartOffset = regionStart;
                    _bpmRegions.Add(new BpmRegion(regionStart, (int)Math.Round(60000 / msPerBeat)));
                }
                else
                {
                    _bpmRegions.Add(new BpmRegion(regionStart - Metadata.ChartOffset,
                                                  (int)Math.Round(60000 / msPerBeat)));
                }
                
                if (_bpmRegions.Count > 1)
                {
                    _bpmRegions[^2].Next = _bpmRegions[^1];
                    _bpmRegions[^1].Previous = _bpmRegions[^2];
                }
            }
            else
            {
                break;
            }
        }

        if (_bpmRegions.Count == 0)
        {
            Trace.WriteLine($"Could not parse timing points: Chart has no timing points.");
            return -1;
        }
        
        Trace.WriteLine($"Chart has {_bpmRegions.Count} BPM regions.");
        return i;
    }

    private static int TryParseHitObjects(string[] lines, int index)
    {
        var i = 1;
        for (; index + i < lines.Length; ++i)
        {
            if (lines[index + i] == "" || lines[index + i] == "\r" || lines[index + i] == "\n" ||
                lines[index + i] == "\r\n")
            {
                break;
            }
            
            var note = NoteBase.FromHitObjectString(lines[index + i].Trim(), out var errorMessage);
            if (note != null)
            {
                AddNote(note);
            }
            else if (errorMessage != "marker")
            {
                Trace.WriteLine($"Could not parse note: {errorMessage}");
                return -1;
            }
        }
        
        Trace.WriteLine($"Chart has {_notes.Count} notes.");
        return i;
    }
    
    private static async Task WriteGeneralChartData(StreamWriter writer)
    {
        await writer.WriteLineAsync("[General]");
        await writer.WriteLineAsync($"AudioFilename: {AudioFileName}");
    }
    
    private static async Task WriteOfficialEditorData(StreamWriter writer)
    {
        await writer.WriteLineAsync("[Editor]");
        if (_labels.Count == 0)
        {
            return;
        }
        
        List<string> bookmarks = [];
        List<string> bookmarksPlus = [];
        foreach (var label in _labels)
        {
            var time = Math.Floor(label.Time);
            bookmarks.Add(time.ToString(CultureInfo.InvariantCulture));
            bookmarksPlus.Add($"{time}`{label.Text}");
        }
        await writer.WriteLineAsync($"Bookmarks: {string.Join(",", bookmarks)}");
        await writer.WriteLineAsync($"BookmarksPlus: {string.Join(",", bookmarksPlus)}");
    }

    private static async Task WriteUnbuggableData(StreamWriter writer)
    {
        await writer.WriteLineAsync("[UNBUGGABLE]");
        await writer.WriteLineAsync(
            $"LastEditorState:{CurrentTime},{BeatSnap},{NoteViewer.CurrentZoom}");
        
        List<string> markerStrings = [];
        foreach (var marker in MarkerNotes)
        {
            markerStrings.Add($"{marker.Time}`{(marker as MarkerDummyNote).ColorId}");
        }
        
        await writer.WriteLineAsync("Markers:");
        
        // wrap every 20 markers to keep lines from being obnoxiously long
        for (var i = 0; i < markerStrings.Count; i += 20)
        {
            await writer.WriteLineAsync(
                $"{string.Join(",", markerStrings.GetRange(
                                   i, Math.Min(20, markerStrings.Count - i)))}");
        }
    }
    
    private static async Task WriteMetadata(StreamWriter writer)
    {
        await writer.WriteLineAsync("[Metadata]");
        await writer.WriteLineAsync($"Title:{Metadata.SongName}");
        await writer.WriteLineAsync($"TitleUnicode:{Metadata.SongName}");
        await writer.WriteLineAsync($"Artist:{Metadata.ArtistName}");
        await writer.WriteLineAsync($"ArtistUnicode:{Metadata.ArtistName}");
        await writer.WriteLineAsync($"Creator:{Metadata.CharterName}");
        await writer.WriteLineAsync($"Version:{Metadata.DifficultyName}");
        
        var tags = new JsonObject
        {
            {"Level", Metadata.DifficultyLevel},
            {"FlavorText", Metadata.FlavorText},
            {"SongLength", Length / 1000},
            {"CoverArt", Metadata.CoverArtistName}
        };
        await writer.WriteLineAsync($"Tags:{tags.ToJsonString()}");
    }
    
    private static async Task WriteTimingPoints(StreamWriter writer)
    {
        await writer.WriteLineAsync("[TimingPoints]");
        var lines = new StringBuilder();
        var first = true;
        foreach (var bpmRegion in _bpmRegions)
        {
            // why does this use 9 decimal places???
            var line = $"{bpmRegion.StartTime},{bpmRegion.MsPerBeat:0.000000000}";
            // more osu stuff, presumably
            line += ",4,2,0,100,1" + (first ? ",0" : ",8");
            first = false;
            lines.AppendLine(line);
        }
        await writer.WriteLineAsync(lines.ToString());
    }
    
    private static async Task WriteHitObjects(StreamWriter writer, bool isStandardFile)
    {
        await writer.WriteLineAsync("[HitObjects]");
        var isFirst = true;
        foreach (var note in _notes)
        {
            var str = note.ToHitObjectString(isFirst, isStandardFile);
            if (str != "")
            {
                await writer.WriteLineAsync(str);
            }
            isFirst = false;
        }
    }
}