using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
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
using NAudio.Wave;
using Tmds.DBus.Protocol;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Audio;
using UNBEATABLEChartEditor.Input;
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

public class ChartDebugInfo
{
    public required bool SongLoaded;
    public required bool Playing;
    public required long MediaPlayerTime;
    public required VLCState MediaPlayerState;
    public required string LastVlcOutput;
    public required double ChartTime;
    public required double PlaySpeed;
}

public static partial class Chart
{
    /// <summary>
    /// A large named label.
    /// </summary>
    public class Label(long time, string text)
    {
        public long Time { get; set; } = time;
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
        public long ChartOffset = 0;

        public override string ToString()
        {
            return $"""
                    - Song name: {SongName}
                    - Artist name: {ArtistName}
                    - Cover artist name: {CoverArtistName}
                    - Charter name: {CharterName}
                    - Flavor text: {FlavorText}
                    - Difficulty slot: {DifficultySlot}
                    - Difficulty name: {DifficultyName}
                    - Difficulty level: {DifficultyLevel}
                    - Offset: {ChartOffset} ms
                    """;
        }
    }
    
    public static ChartDebugInfo DebugInfo => new()
    {
        Playing = Playing,
        SongLoaded = SongLoaded,
        MediaPlayerTime = _mediaPlayer.Time,
        MediaPlayerState = _mediaPlayer.State,
        LastVlcOutput = _lastVlcConsoleOutput,
        ChartTime = CurrentTimeRaw,
        PlaySpeed = PlaySpeed
    };

    private static List<NoteBase> _notes = [];
    /// <summary>
    /// All notes in the chart, including markers.
    /// </summary>
    public static ReadOnlyCollection<NoteBase> Notes => _notes.AsReadOnly();

    public static ReadOnlyCollection<NoteBase> NonMarkerNotes =>
        _notes.Where(n => n is not MarkerNote).ToList().AsReadOnly();
    
    public static ReadOnlyCollection<NoteBase> MarkerNotes =>
        _notes.Where(n => n is MarkerNote).ToList().AsReadOnly();
    
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
            UnsavedChanges = _metadata.SongName != value.SongName ||
                             _metadata.ArtistName != value.ArtistName ||
                             _metadata.CoverArtistName != value.CoverArtistName ||
                             _metadata.CharterName != value.CharterName ||
                             _metadata.FlavorText != value.FlavorText ||
                             _metadata.DifficultySlot != value.DifficultySlot ||
                             _metadata.DifficultyName != value.DifficultyName ||
                             _metadata.DifficultyLevel != value.DifficultyLevel ||
                             _metadata.ChartOffset != value.ChartOffset;
            
            // metadata was unchanged, so updates can be skipped
            if (!UnsavedChanges)
            {
                return;
            }
            
            // technically the chart can only be saved if it has a difficulty slot, but the way the
            // enum is set up makes it impossible to not have one
            var canSave = (_metadata.SongName != "" && _metadata.ArtistName != "" &&
                           _metadata.CharterName != "");
            App.MainWindowViewModel.CanSave = canSave;

            // charts can't be autosaved unless they've been saved with their current file name at
            // least once (or were loaded from a chart file and the metadata hasn't changed)
            _canAutosave = false;

            if (_metadata.ChartOffset != value.ChartOffset)
            {
                var delta = value.ChartOffset - _metadata.ChartOffset;
                
                if (_bpmRegions.Count != 0)
                {
                    foreach (var region in _bpmRegions)
                    {
                        region.StartTime += delta;
                    }
                
                    RebuildSnapLineSets();
                }

                if (_labels.Count != 0)
                {
                    foreach (var label in _labels)
                    {
                        label.Time -= delta;
                    }
                }
                
                _jumpTargetsOutOfDate = true;
            }
            
            _metadata = value;
            
            Logger.Info("updated chart metadata:\n{0}", _metadata.ToString());
            
            if (canSave)
            {
                ChartFileName = GetChartFileName();
                Logger.Info("chart file name: \"{0}\"", ChartFileName);
            }
            
            UpdateWindowTitle();
        }
    }

    public static string AudioFileName = "";
    public static int BeatSnap = 1;

    public static string ChartFolderName { get; private set; } = "";
    public static string ChartFileName { get; private set; } = "";
    
    public static long Length => _mediaPlayer.Media != null ? 
        _mediaPlayer.Media.Duration - AdjustedOffset : -1;

    public static long AdjustedOffset => Metadata.ChartOffset + Config.Settings.HardChartOffset;

    public static bool UnsavedChanges { get; private set; } = false;

    private static double _currentTimeRaw = 0;
    public static double CurrentTimeRaw
    {
        get => _currentTimeRaw;
        private set
        {
            _currentTimeRaw = value;
            
            foreach (var region in _bpmRegions)
            {
                if (CurrentTimeRaw >= region.StartTime && CurrentTimeRaw <= region.EndTime)
                {
                    App.MainWindowViewModel.SongBpmText = region.Bpm.ToString("0.00");
                }
            }

            if (!Playing)
            {
                App.MainWindowViewModel.UpdatePriorityListEntries(GetNotesAtCurrentTime());
            }
        }
    }
    public static long CurrentTime => (long)Math.Round(CurrentTimeRaw);

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
                Logger.Debug("song volume changed to {0}", value);
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
            Logger.Debug("sfx volume changed to {0}", value);
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
            Logger.Debug("song volume changed to {0}", value);
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
            InputManager.ResetInputStates();
            if (value)
            {
                App.MainWindowViewModel.ClearPriorityListEntries();
            }
        }
    }
    
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    private static LibVLC _libVlc = null!;
    private static MediaPlayer _mediaPlayer = null!;

    private static MediaPlayer _hitSoundMediaPlayer = null!;
    // private static ChartSongPlayer? _songPlayer = null!;
    
    private static CachedSound? _hitSound = null;
    
    // used for keeping track of the song's actual play position
    private static Stopwatch _stopwatch = null!;

    private static double _lastStopwatchTime = 0;
    
    private static int _beatSnapIndex = 0;
    
    // timestamps for where every line appears for every snap setting, updated whenever bpm regions
    // change
    private static readonly Dictionary<int, List<long>> SnapLineSets = new();
    private static List<long> _currentSnapLineSet = [];
    private static int _currentSnapLineSetIndex = 0;
    
    // for debugging
    private static string _lastVlcConsoleOutput = "";
    
    private static bool _canAutosave = false;
    
    private static readonly JsonSerializerOptions MetadataJsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static List<long> _jumpTargets = [];
    private static bool _jumpTargetsOutOfDate = true;
    
    [GeneratedRegex(@"[0-9]+,[0-9]+.[0-9]+,[0-9]+,[0-9]+,[0-9]+,[0-9]+,[0-9]+,[0-9]+")]
    private static partial Regex TimingPointRegex();

    [GeneratedRegex(@".*\[(.+)\].*")]
    private static partial Regex DifficultySlotRegex();

    // cursed regex because chart tags allow double quotes in a string so i can't just use json
    [GeneratedRegex("""{"Level":([0-9]+),"FlavorText":"(.*)","SongLength":(?:[+-]?([0-9]*[.])?[0-9]+),"CoverArt":"(.*)"}""")]
    private static partial Regex TagRegex();

    /// <summary>
    /// Initializes everything. This must be called before any other methods are used!
    /// </summary>
    public static void Init()
    {
        _libVlc = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVlc);
        _hitSoundMediaPlayer = new MediaPlayer(_libVlc);
        _mediaPlayer.EndReached += MediaPlayer_EndReached;
        _libVlc.Log += (_, args) =>
        {
            _lastVlcConsoleOutput = args.Message;
        };
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
                Logger.Warn(
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
        if (index < 0 || index >= Config.Settings.BeatSnaps.Count)
        {
            throw new ArgumentOutOfRangeException($"Beat snap index {index} out of range.");
        }
        
        _beatSnapIndex = index;
        BeatSnap = Config.Settings.BeatSnaps[_beatSnapIndex];
        App.MainWindow.BeatSnapText.Text = BeatSnap.ToString();
        _currentSnapLineSet = SnapLineSets[BeatSnap];
        
        Logger.Debug("beat snap changed to {0} (index {1})", BeatSnap, _beatSnapIndex);
        SetTimeToNearestSnap();
    }
    
    public static void IncreaseBeatSnap()
    {
        ++_beatSnapIndex;
        if (_beatSnapIndex >= Config.Settings.BeatSnaps.Count)
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
            _beatSnapIndex = Config.Settings.BeatSnaps.Count - 1;
        }
        SetBeatSnapIndex(_beatSnapIndex);
    }

    public static void MoveToPreviousSnap()
    {
        if (_currentSnapLineSetIndex < _currentSnapLineSet.Count - 1)
        {
            ++_currentSnapLineSetIndex;
            CurrentTimeRaw = _currentSnapLineSet[_currentSnapLineSetIndex];
        }
    }

    public static void MoveToNextSnap()
    {
        if (_currentSnapLineSetIndex > 0)
        {
            --_currentSnapLineSetIndex;
            CurrentTimeRaw = _currentSnapLineSet[_currentSnapLineSetIndex];
        }
    }

    public static long GetPreviousSnapTime() => _currentSnapLineSetIndex > 0
        ? _currentSnapLineSet[_currentSnapLineSetIndex - 1]
        : _currentSnapLineSet[0];

    public static long GetNextSnapTime() =>
        _currentSnapLineSetIndex < _currentSnapLineSet.Count - 1 ?
        _currentSnapLineSet[_currentSnapLineSetIndex + 1] : _currentSnapLineSet[^1];
    
    public static void QuickScroll(int numBeats)
    {
        var lastSnapLineSet = _currentSnapLineSet;
        
        _currentSnapLineSet = SnapLineSets[1];
        SetTimeToNearestSnap();
        
        for (var i = 0; i < Math.Abs(numBeats); ++i)
        {
            if (numBeats > 0)
            {
                MoveToNextSnap();
            }
            else
            {
                MoveToPreviousSnap();
            }
        }
        
        _currentSnapLineSet = lastSnapLineSet;
        SetTimeToNearestSnap();
    }

    public static void MoveToNextLabel()
    {
        if (_jumpTargetsOutOfDate)
        {
            RebuildJumpTargets();
            _jumpTargetsOutOfDate = false;
        }
        
        if (CurrentTimeRaw >= Length)
        {
            return;
        }
        
        // skip labels within the next snap - this may end up jumping past the next label (but that
        // shouldn't be an issue unless you have multiple labels within a single beat) but prevents
        // an infinite loop if the next label isn't quite on a snap line
        if (_jumpTargets.Any(t => t > GetNextSnapTime()))
        {
            var time = _jumpTargets.Find( t => t > GetNextSnapTime());
            CurrentTimeRaw = time;
        }
        else
        {
            CurrentTimeRaw = Length;
        }
        
        SetTimeToNearestSnap();
        App.MainWindowViewModel.UpdatePriorityListEntries();
    }
    
    public static void MoveToPreviousLabel()
    {
        if (_jumpTargetsOutOfDate)
        {
            RebuildJumpTargets();
            _jumpTargetsOutOfDate = false;
        }
        
        if (CurrentTimeRaw <= 0)
        {
            return;
        }
        
        // skip labels within the previous snap - this may end up jumping past the actual previous
        // label (but that shouldn't be an issue unless you have multiple labels within a single
        // beat) but prevents an infinite loop if the previous label isn't quite on a snap line
        if (_jumpTargets.Any(t => t < GetPreviousSnapTime()))
        {
            var time = _jumpTargets.FindLast(t => t < GetPreviousSnapTime());
            CurrentTimeRaw = time;
        }
        else
        {
            CurrentTimeRaw = 0;
        }
        
        SetTimeToNearestSnap();
        App.MainWindowViewModel.UpdatePriorityListEntries();
    }

    public static void MoveToBreakpoint()
    {
        if (ChartBuilder.BreakpointTime != -1000)
        {
            CurrentTimeRaw = ChartBuilder.BreakpointTime;
            SetTimeToNearestSnap();
        }
    }

    /// <summary>
    /// Called once per tick, used to update chart time and play hit sounds.
    /// </summary>
    public static void PerTickUpdate()
    {
        if (SongLoaded && Playing)
        {
            var prevTime = CurrentTimeRaw;
            CurrentTimeRaw += (_stopwatch.ElapsedMilliseconds - _lastStopwatchTime) * PlaySpeed / 100;
            if (CurrentTimeRaw + AdjustedOffset >= 0 && !_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.SeekTo(TimeSpan.FromMilliseconds(CurrentTimeRaw + AdjustedOffset));
                _mediaPlayer.Play();
            }
            else
            {
                if (CurrentTimeRaw > Length)
                {
                    Playing = false;
                    CurrentTimeRaw = Length;
                }
            }
            
            foreach (var note in Notes)
            {
                if (note.ShouldPlayHitSound(prevTime - Config.Settings.HitSoundOffset,
                                            CurrentTimeRaw - Config.Settings.HitSoundOffset)
                    is { } offset)
                {
                    if (_hitSound != null)
                    {
                        SfxEngine.Play(_hitSound, offset);
                    }
                    break;
                }
            }
        }
        
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
    /// <returns>
    /// Whether the audio file could be loaded, followed by an error message (or an empty string if
    /// there was no error).
    /// </returns>
    public static async Task<(bool, string)> TryCreateChartFromAudio(string path)
    {
        SongLoaded = false;
        _canAutosave = false;
        _jumpTargetsOutOfDate = true;

        var result = await TryLoadAudioFile(path);
        if (!result.Item1)
        {
            ClearChart();
            UpdateWindowTitle();
            return (false, result.Item2);
        }
        
        Logger.Info("Creating chart from audio file...");
        
        App.MainWindow.PlaySpeedSlider.Value = PlaySpeed;

        Metadata = new MetadataContainer();

        if (Config.Settings.UseAudioFileMetadata)
        {
            var tFile = TagLib.File.Create(path);
            Logger.Info("""
                        Audio metadata:
                            Title: {0}
                            Artist: {1}
                        """, tFile.Tag.Title, tFile.Tag.FirstPerformer);
        
            if (tFile.Tag.Title != "")
            {
                Metadata.SongName = tFile.Tag.Title;
            }

            if (tFile.Tag.FirstPerformer != "")
            {
                Metadata.ArtistName = tFile.Tag.FirstPerformer;
            }

            if (tFile.Tag.FirstPerformer != "")
            {
                Metadata.ArtistName = tFile.Tag.FirstPerformer;
            }
        }

        Metadata.CharterName = Config.Settings.DefaultCharterName;
        Metadata.DifficultySlot = Config.Settings.DefaultDifficulty switch
        {
            "beginner" => DifficultySlot.BEGINNER,
            "easy" => DifficultySlot.NORMAL,
            "normal" => DifficultySlot.HARD,
            "hard" => DifficultySlot.EXPERT,
            "unbeatable" => DifficultySlot.UNBEATABLE,
            _ => DifficultySlot.STAR
        };

        if (Metadata.DifficultySlot == DifficultySlot.STAR)
        {
            Metadata.DifficultyName = "Star";
        }
        
        _notes = [];
        _labels = [];
        ChartBuilder.ClearSelection();
        ChartBuilder.TryRemoveBreakpoint();
            
        _bpmRegions = [new BpmRegion(0, 60)];
        RebuildSnapLineSets();
        SetBeatSnapIndex(0);
        SetTimeToNearestSnap();

        NoteViewer.SetZoom(1.0);
        CurrentTimeRaw = 0;
        ChartFileName = "";
        ChartFolderName = Path.GetFileName(Path.GetDirectoryName(path));
            
        App.MainWindowViewModel.SongBpmText = _bpmRegions[0].Bpm.ToString("0.000");
        App.MainWindowViewModel.PlaySpeed = 100;
        App.MainWindowViewModel.CanSave = false;

        SongLoaded = true;
        UnsavedChanges = false;
        UserData.LastOpenedChartFile = ""; 
        UpdateWindowTitle();
        return (true, "");
    }

    /// <summary>
    /// Tries to load a chart file (either a standard .txt file from the official editor or an
    /// UNBUGGABLE .beat.txt file) and set up notes, markers, BPM changes, etc.
    /// </summary>
    /// <returns>
    /// Whether all data could be loaded, followed by an error message (or an empty string if there
    /// was no error).
    /// </returns>
    public static async Task<(bool, string)> TryLoadChartFile(string path)
    {
        SongLoaded = false;
        _canAutosave = false;
        _jumpTargetsOutOfDate = true;
        
        Logger.Info("Loading chart file \"{0}\"", path);
        if (!File.Exists(path))
        {
            Logger.Error("File not found.");
            return (false, "File not found.");
        }
        
        var dirName = Path.GetFullPath(path);
        var folderPath = dirName[..dirName.LastIndexOf('\\')];
        var chartData = (await File.ReadAllTextAsync(path)).Split("\n");
        string? audioPath = null;
        (double, int, double, int)? lastEditorState = null;
        
        Metadata = new MetadataContainer();
        _labels = [];
        _notes = [];
        ChartBuilder.ClearSelection();
        _bpmRegions = [];
        for (var i = 0; i < chartData.Length; i++)
        {
            var line = chartData[i].Trim();
            var temp = 0;
            var errorMessage = "";
            switch (line)
            {
                case "[General]":
                    Logger.Debug("Parsing general data...");
                    temp = TryParseGeneralChartData(chartData, i, folderPath, out audioPath,
                                                    out errorMessage);
                    break;
                case "[Editor]":
                    Logger.Debug("Parsing official editor data...");
                    temp = TryParseOfficialEditorData(chartData, i, out errorMessage);
                    break;
                case "[UNBUGGABLE]":
                    Logger.Debug("Parsing UNBUGGABLE data...");
                    temp = TryParseUnbuggableData(chartData, i, out lastEditorState,
                                                  out errorMessage);
                    break;
                case "[Metadata]":
                    Logger.Debug("Parsing metadata...");
                    temp = TryParseMetadata(chartData, i, out errorMessage);
                    // see??? do you see how easy it would be to make the official editor save star
                    // charts correctly??? why would you not do this???
                    Metadata.DifficultySlot =
                        DifficultySlotRegex().Match(path).Groups[1].Value switch
                    {
                        "Beginner" => DifficultySlot.BEGINNER,
                        "Easy" => DifficultySlot.NORMAL,
                        "Normal" => DifficultySlot.HARD,
                        "Hard" => DifficultySlot.EXPERT,
                        "UNBEATABLE" => DifficultySlot.UNBEATABLE,
                        _ => DifficultySlot.STAR
                    };
                    break;
                // there are also [Difficulty] and [Events] sections here but they do nothing
                case "[TimingPoints]":
                    Logger.Debug("Parsing timing points...");
                    temp = TryParseTimingPoints(chartData, i, out errorMessage);
                    break;
                case "[HitObjects]":
                    Logger.Debug("Parsing hit objects (notes)...");
                    temp = TryParseHitObjects(chartData, i, out errorMessage);
                    break;
            }
            
            if (temp == -1)
            {
                _mediaPlayer.Media = null; // disables the editor
                return (false, errorMessage);
            }

            i += temp;
        }

        // check for an alternate audio format
        var result = await TryLoadAudioFile(audioPath);
        if (result.Item1)
        {
            RebuildSnapLineSets();
            SetBeatSnapIndex(0);
            ChartFileName = GetChartFileName();
            ChartFolderName = Directory.GetParent(path)?.Name ?? "";
            SongLoaded = true;
            UnsavedChanges = false;
            UserData.LastOpenedChartFile = path;

            // last editor state never makes the load fail (and the data for it may not even exist)
            if (lastEditorState != null)
            {
                var time = lastEditorState.Value.Item1;
                var beatSnap = lastEditorState.Value.Item2;
                var zoom = lastEditorState.Value.Item3;
                ChartBuilder.SetCopId(lastEditorState.Value.Item4);

                if (time >= 0 && time <= Length)
                {
                    CurrentTimeRaw = time;
                }

                Logger.Debug(
                    $"Restoring last editor state: {time} ms, snap {beatSnap}, {zoom}x zoom");
                for (var i = 0; i < Config.Settings.BeatSnaps.Count; ++i)
                {
                    if (beatSnap == Config.Settings.BeatSnaps[i])
                    {
                        SetBeatSnapIndex(i);
                        break;
                    }
                }

                if (zoom >= Config.Settings.MinZoom && zoom <= Config.Settings.MaxZoom)
                {
                    NoteViewer.SetZoom(zoom);
                }
            }
            // clear the editor state from the last file that was open
            else
            {
                CurrentTimeRaw = 0;
                SetBeatSnapIndex(0);
                NoteViewer.SetZoom(1);
            }

            var difficultySlotName = Metadata.DifficultySlot switch
            {
                DifficultySlot.BEGINNER => "Beginner",
                DifficultySlot.NORMAL => "Normal",
                DifficultySlot.HARD => "Hard",
                DifficultySlot.EXPERT => "Expert",
                DifficultySlot.UNBEATABLE => "UNBEATABLE",
                _ => "Star"
            };
            App.MainWindowViewModel.DifficultyText = $"{difficultySlotName} " +
                                                     $"{Metadata.DifficultyLevel}";
            
            ChartBuilder.CheckExistingBreakpoint();
            SetTimeToNearestSnap();
            _canAutosave = true;
            
            UpdateWindowTitle();
            
            Logger.Info("Chart loaded successfully.");
            return (true, "");
        }
        
        ClearChart();
        UpdateWindowTitle();
        return (false, result.Item2);
    }

    /// <summary>
    /// Saves the chart to a .beat.txt with extra UNBUGGABLE data.
    /// </summary>
    public static async Task<bool> SaveToBeatPath(string path)
    {
        await using var writer = await Utils.TryWaitForFileStream(path);
        if (writer == null)
        {
            Logger.Error("Save failed: file is not accessible");
            return false;
        }
        
        Logger.Info("saving chart to \"{0}\"", path);
        writer.AutoFlush = true;
        await writer.WriteLineAsync("// Output from NPC's UNBUGGABLE editor\n" +
                                    "// based on TaroNuke's unity editor");
        
        Logger.Debug("writing general chart data");
        await writer.WriteLineAsync("");
        await WriteGeneralChartData(writer);
        
        Logger.Debug("writing official editor data");
        await writer.WriteLineAsync("");
        await WriteOfficialEditorData(writer);
        
        Logger.Debug("writing unbuggable data");
        await writer.WriteLineAsync("");
        await WriteUnbuggableData(writer);

        Logger.Debug("writing metadata");   
        await writer.WriteLineAsync("");
        await WriteMetadata(writer);
        
        // for some reason there are just some random empty sections here (probably from osu)
        await writer.WriteLineAsync("\n[Difficulty]\n\n[Events]\n");

        Logger.Debug("writing timing points");
        await WriteTimingPoints(writer);
        
        Logger.Debug("writing hit objects");   
        await writer.WriteLineAsync("");
        await WriteHitObjects(writer, false);
        
        UserData.LastOpenedChartFile = path;
        UnsavedChanges = false;

        Logger.Info("Chart saved successfully.");
        return true;
    }
    
    /// <summary>
    /// Saves the chart to a standard .txt file.
    /// </summary>
    public static async Task<bool> SaveToStandardPath(string path)
    {
        await using var writer = await Utils.TryWaitForFileStream(path);
        if (writer == null)
        {
            Logger.Error("Save failed: file is not accessible");
            return false;
        }
        
        Logger.Info("saving chart to \"{0}\"", path);
        writer.AutoFlush = true;
        await writer.WriteLineAsync("// Output from NPC's UNBUGGABLE editor\n" +
                                    "// based on TaroNuke's unity editor");
        
        Logger.Debug("writing general chart data");
        await writer.WriteLineAsync("");
        await WriteGeneralChartData(writer);
        
        Logger.Debug("writing official editor data");
        await writer.WriteLineAsync("");
        await WriteOfficialEditorData(writer);

        Logger.Debug("writing metadata");   
        await writer.WriteLineAsync("");
        await WriteMetadata(writer);
        
        // for some reason there are just some random empty sections here (probably from osu)
        await writer.WriteLineAsync("\n[Difficulty]\n\n[Events]\n");

        Logger.Debug("writing timing points");
        await WriteTimingPoints(writer);
        
        Logger.Debug("writing hit objects");   
        await writer.WriteLineAsync("");
        await WriteHitObjects(writer, false);
        
        UserData.LastOpenedChartFile = path;
        UnsavedChanges = false;

        Logger.Info("Chart saved successfully.");
        return true;
    }

    /// <summary>
    /// Returns the timestamps (in milliseconds) of every single full beat in some range, along with
    /// their beat number.
    /// </summary>
    public static IEnumerable<(double, int)> GetBeatTimesInRange(double start, double end)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, Length);
        for (var i = 0; i < SnapLineSets[1].Count; ++i)
        {
            var beatTime = SnapLineSets[1][i];
            if (beatTime >= start && beatTime <= end)
            {
                yield return (beatTime, i);
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
    public static void TryAddMarker(long time)
    {
        var marker = MarkerNotes.FirstOrDefault(n => n.Time == time);
        if (marker == null)
        {
            AddNote(new MarkerNote(time)
            {
                Color1 = true,
                Color2 = false,
                Color3 = false
            });
        }
    }

    /// <summary>
    /// Returns a list of every non-marker note that exists at a timestamp (in milliseconds). List
    /// elements are formatted as <c>(note, index in the main note list)</c>.
    /// </summary>
    public static List<(NoteBase, int)> GetNotesAtTime(long time)
    {
        List<(NoteBase, int)> notes = [];
        foreach (NoteBase note in NonMarkerNotes)
        {
            if (note.Time == time)
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
    
    public static List<(NoteBase, int)> GetNotesAtCurrentTime() => GetNotesAtTime(CurrentTime);

    /// <summary>
    /// Returns a list of every non-marker, non-instant note that ends at a timestamp
    /// (in milliseconds). List elements are formatted as <c>(note, index in the main note
    /// list)</c>.
    /// </summary>
    public static List<(NoteBase, int)> GetNoteEndsAtTime(long time)
    {
        List<(NoteBase, int)> notes = [];
        foreach (NoteBase note in NonMarkerNotes)
        {
            if (!note.Instant && note.EndTime == time)
            {
                notes.Add((note, NonMarkerNotes.IndexOf(note)));
            }
        }

        return notes;
    }
    
    /// <summary>
    /// Returns the note in a specific lane at a specific time, or null if that note does not exist.
    /// </summary>
    public static NoteBase? GetNote(long time, NoteLane lane, long maxDistance = 0)
        => _notes.FirstOrDefault(n => Math.Abs(n.Time - time) <= maxDistance && n.Lane == lane);
    
    /// <summary>
    /// Returns the (non-instant) note in a specific lane that <i>ends</i> at a specific time, or
    /// null if that note does not exist.
    /// </summary>
    public static NoteBase? GetNoteFromEnd(long time, NoteLane lane, long maxDistance = 0) =>
        _notes.FirstOrDefault(n => !n.Instant && Math.Abs(n.EndTime - time) <= maxDistance &&
                                   n.Lane == lane);

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
    
    public static NoteBase? GetLastNoteBeforeTime(long time) =>
        NonMarkerNotes.LastOrDefault(n => n.Time <= time);
    
    public static NoteBase? GetLastNoteBeforeTime(long time, NoteLane lane) =>
        NonMarkerNotes.LastOrDefault(n => n.Time <= time && n.Lane == lane);
    
    public static NoteBase? GetLastNoteBeforeTime(long time, List<NoteLane> lanes) =>
        NonMarkerNotes.LastOrDefault(n => n.Time <= time && lanes.Contains(n.Lane));

    public static NoteBase? GetFirstNoteAfterTime(long time) =>
        NonMarkerNotes.FirstOrDefault(n => n.Time >= time);
    
    public static NoteBase? GetFirstNoteAfterTime(long time, NoteLane lane) =>
        NonMarkerNotes.FirstOrDefault(n => n.Time >= time && n.Lane == lane);
    
    public static NoteBase? GetFirstNoteAfterTime(long time, List<NoteLane> lanes) =>
        NonMarkerNotes.FirstOrDefault(n => n.Time >= time && lanes.Contains(n.Lane));

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
            var i = _notes.FindIndex(x => x.Time > note.Time);
            _notes.Insert(i, note);
        }
        
        App.MainWindowViewModel.UpdatePriorityListEntries(GetNotesAtCurrentTime());
        UnsavedChanges = true;
        _jumpTargetsOutOfDate = true;
    }

    public static void RemoveNote(NoteBase note)
    {
        _notes.Remove(note);
        App.MainWindowViewModel.UpdatePriorityListEntries(GetNotesAtCurrentTime());
        UnsavedChanges = true;
        _jumpTargetsOutOfDate = true;
    }
    
    /// <summary>
    /// Replaces a note, preserving placement priority.
    /// </summary>
    public static void ReplaceNote(NoteBase oldNote, NoteBase newNote)
    {
        _notes[_notes.IndexOf(oldNote)] = newNote;
        App.MainWindowViewModel.UpdatePriorityListEntries(GetNotesAtCurrentTime());
        UnsavedChanges = true;
        _jumpTargetsOutOfDate = true;
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
        
        UnsavedChanges = true;
        _jumpTargetsOutOfDate = true;
    }

    public static void AddOrUpdateMarker(long time, bool color1, bool color2, bool color3)
    {
        var existing = MarkerNotes.FirstOrDefault(n => n.Time == time);
        if (existing != null)
        {
            var m = (MarkerNote)existing;
            if (color1)
            {
                m.Color1 = !m.Color1;
            }
            else if (color2)
            {
                m.Color2 = !m.Color2;
            }
            else if (color3)
            {
                m.Color3 = !m.Color3;
            }

            if (m is { Color1: false, Color2: false, Color3: false })
            {
                RemoveNote(existing);
                UnsavedChanges = true;
                _jumpTargetsOutOfDate = true;
            }
        }
        else
        {
            AddNote(new MarkerNote(time)
            {
                Color1 = color1,
                Color2 = color2,
                Color3 = color3
            });
            UnsavedChanges = true;
            _jumpTargetsOutOfDate = true;
        }
    }

    /// <summary>
    /// Returns the label at a specific time, or null if it doesn't exist.
    /// </summary>
    public static Label? GetLabel(long time) =>
        _labels.FirstOrDefault(x => x.Time == time + Metadata.ChartOffset);
    
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
        
        UnsavedChanges = true;
        _jumpTargetsOutOfDate = true;
    }

    public static void RemoveLabel(Label label)
    {
        _labels.Remove(label);
        UnsavedChanges = true;
        _jumpTargetsOutOfDate = true;
    }

    /// <summary>
    /// Returns the BPM region that starts at a specific time, or null if it doesn't exist.
    /// </summary>
    public static BpmRegion? GetBpmRegion(long time)
    {
        foreach (var region in _bpmRegions)
        {
            if (region.StartTime == time)
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
                    _bpmRegions[i + 2].Previous = region;

                    _bpmRegions[i].Next = region;
                    region.Previous = _bpmRegions[i];
                    break;
                }
            }
        }
        
        RebuildSnapLineSets();
        UnsavedChanges = true;
        _jumpTargetsOutOfDate = true;
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
        UnsavedChanges = true;
    }

    public static void EditBpmRegion(BpmRegion region, double newBpm)
    {
        if (newBpm <= 0)
        {
            throw new InvalidOperationException("Bpm must be positive");
        }
        
        region.Bpm = newBpm;
        RebuildSnapLineSets();
        UnsavedChanges = true;
    }

    /// <summary>
    /// Recalculates where every snap line is for every bpm setting. This must be called any time
    /// that a BPM region changes.
    /// </summary>
    public static void RebuildSnapLineSets()
    {
        var sortedSnapValues = Config.Settings.BeatSnaps.OrderByDescending(x => x).ToList();
        foreach (var snapValue in sortedSnapValues)
        {
            List<long> snapLineSet = [0];
            double time = 0;
            var bpmRegion = _bpmRegions[0];
            while (bpmRegion != null)
            {
                var nextTime = time + (bpmRegion.MsPerBeat / snapValue);
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
                snapLineSet.Add((long)Math.Round(time));
            }
            SnapLineSets[snapValue] = snapLineSet;
        }
        
        _currentSnapLineSet = SnapLineSets[BeatSnap];
        SetTimeToNearestSnap();
    }

    /// <summary>
    /// Recalculates every timestamp that label jumping can send you to.
    /// </summary>
    public static void RebuildJumpTargets()
    {
        _jumpTargets = [];
        if (Config.Settings.JumpTargets.Contains("labels"))
        {
            _jumpTargets.AddRange(_labels.Select(l => l.Time - Metadata.ChartOffset));
        }

        if (Config.Settings.JumpTargets.Contains("bpmChanges"))
        {
            _jumpTargets.AddRange(_bpmRegions.Select(r => r.StartTime));
        }

        if (Config.Settings.JumpTargets.Contains("firstNote") && NonMarkerNotes.Count > 0)
        {
            _jumpTargets.Add(NonMarkerNotes[0].Time);
        }
        
        if (Config.Settings.JumpTargets.Contains("lastNote") && NonMarkerNotes.Count > 0)
        {
            _jumpTargets.Add(NonMarkerNotes[^1].Time);
        }

        if (Config.Settings.JumpTargets.Contains("secondLastNote") && NonMarkerNotes.Count > 1)
        {
            _jumpTargets.Add(NonMarkerNotes[^2].Time);
        }
        
        if (Config.Settings.JumpTargets.Contains("firstMarker") && MarkerNotes.Count > 0)
        {
            _jumpTargets.Add(MarkerNotes[0].Time);
        }
        
        if (Config.Settings.JumpTargets.Contains("lastMarker") && MarkerNotes.Count > 0)
        {
            _jumpTargets.Add(MarkerNotes[^1].Time);
        }

        if (Config.Settings.JumpTargets.Contains("breakpoint") &&
            ChartBuilder.BreakpointTime != -1000)
        {
            _jumpTargets.Add(ChartBuilder.BreakpointTime);
        }
        
        _jumpTargets = _jumpTargets.Distinct().ToList();
        _jumpTargets.Sort();
    }

    private static void ClearChart()
    {
        Metadata = new MetadataContainer();
        _notes = [];
        _labels = [];
        ChartBuilder.ClearSelection();
        ChartBuilder.TryRemoveBreakpoint(false);
            
        _bpmRegions = [];
        _beatSnapIndex = 0;
        _currentSnapLineSet = [];
        SnapLineSets.Clear();

        NoteViewer.SetZoom(1.0);
        CurrentTimeRaw = 0;
        ChartFileName = "";
        ChartFolderName = "";
            
        App.MainWindowViewModel.SongBpmText = "";
        App.MainWindowViewModel.PlaySpeed = 100;
        App.MainWindowViewModel.CanSave = false;

        SongLoaded = false;
        UnsavedChanges = false;
    }

    public static async Task TryAutosave()
    {
        Logger.Info("Attempting to autosave chart...");
        if (!_canAutosave)
        {
            Logger.Warn("Autosave failed: Chart state is invalid.");
            return;
        }
        
        Logger.Info("saving to \"{0}.auto\"", UserData.LastOpenedChartFile);
        bool successful;
        if (UserData.LastOpenedChartFile.EndsWith(".beat.txt"))
        {
            successful = await SaveToBeatPath(UserData.LastOpenedChartFile + ".auto");
        }
        else
        {
            successful = await SaveToStandardPath(UserData.LastOpenedChartFile + ".auto");
        }

        if (successful)
        {
            Logger.Info("Autosave complete!");
        
            // reset the last opened file so we don't continuously add ".auto" to the end
            UserData.LastOpenedChartFile = UserData.LastOpenedChartFile.Replace(".auto", "");

            App.MainWindowViewModel.ShowEventIndicator(
                $"Autosaved to {Path.GetFileName(UserData.LastOpenedChartFile)}.auto");
        }
        else
        {
            Logger.Warn("Autosave failed.");
        }
    }

    private static void PlaySong()
    {
        if (CurrentTimeRaw + AdjustedOffset >= Length)
        {
            return;
        }
        
        Playing = true;
        if (CurrentTimeRaw + AdjustedOffset >= 0)
        {
            if (_mediaPlayer.Media.State == VLCState.Ended)
            {
                _mediaPlayer.Play(_mediaPlayer.Media);
            }
            else
            {
                _mediaPlayer.Play();
            }
            _mediaPlayer.SeekTo(TimeSpan.FromMilliseconds(CurrentTimeRaw + AdjustedOffset));
        }
    }
    
    private static void PauseSong()
    {
        Playing = false;
        _mediaPlayer.Pause();
        SetTimeToNearestSnap();
    }

    private static void MediaPlayer_EndReached(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            PauseSong();
        });
    }
    
    /// <summary>
    /// Parses metadata and returns the suggested file name (without extension) for the chart,
    /// accounting for Unicode characters and brackets/parentheses.
    /// </summary>
    private static string GetChartFileName()
    {
        // difficulty slots have different internal names for some reason
        var difficultyName = Metadata.DifficultySlot switch
        {
            DifficultySlot.BEGINNER => "Beginner",
            DifficultySlot.NORMAL => "Easy",
            DifficultySlot.HARD => "Normal",
            DifficultySlot.EXPERT => "Hard",
            DifficultySlot.UNBEATABLE => "UNBEATABLE",
            _ => "Star"
        };
        
        return $"{SanitizeString(Metadata.ArtistName)} - {SanitizeString(Metadata.SongName)} " +
               $"({SanitizeString(Metadata.CharterName)}) [{SanitizeString(difficultyName)}]";
    }

    private static void UpdateWindowTitle()
    {
        if (Metadata.SongName != "" && Metadata.ArtistName != "")
        {
            var difficultySlotName = Metadata.DifficultySlot switch
            {
                DifficultySlot.BEGINNER => "Beginner",
                DifficultySlot.NORMAL => "Normal",
                DifficultySlot.HARD => "Hard",
                DifficultySlot.EXPERT => "Expert",
                DifficultySlot.UNBEATABLE => "UNBEATABLE",
                _ => "Star"
            };
            App.MainWindow.Title = $"{_metadata.ArtistName} — {_metadata.SongName} " +
                                   $"({difficultySlotName}) — UNBUGGABLE";
        }
        else
        {
            App.MainWindow.Title = "UNBUGGABLE";
        }
    }

    private static string SanitizeString(string str)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var pattern = "[" + Regex.Escape(new string(invalid)) + @"]|[^\x00-\x7F]";
        return Regex.Replace(str, pattern, "_").Replace("(", "_").Replace(")", "_")
                    .Replace("[", "_").Replace("]", "_");
    }
    
    private static async Task<(bool, string)> TryLoadAudioFile(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                Logger.Error("Could not load audio file \"{0}\": File not found.", path);
                return (false, $"Could not load audio file \"{path}\": File not found.");
            }
            
            var media = new Media(_libVlc, path);
            _mediaPlayer.Media = media;
            _mediaPlayer.SeekTo(TimeSpan.FromMilliseconds(-AdjustedOffset));
            AudioFileName = Path.GetFileName(path);
            
            await media.Parse();
        }
        catch (Exception e)
        {
            Logger.Error("Could not load audio file \"{0}\": {1}", path, e.Message);
            return (false, $"Could not load audio file \"{path}\": {e.Message}");
        }
        
        return (true, "");
    }
    
    private static void SetTimeToNearestSnap()
    {
        for (var i = 0; i < _currentSnapLineSet.Count - 1; ++i)
        {
            var currentSnap = _currentSnapLineSet[i];
            var nextSnap = _currentSnapLineSet[i + 1];
            if (CurrentTimeRaw >= currentSnap && CurrentTimeRaw <= nextSnap)
            {
                if (Math.Abs(CurrentTimeRaw - currentSnap) < Math.Abs(CurrentTimeRaw - nextSnap))
                {
                    CurrentTimeRaw = currentSnap;
                    _currentSnapLineSetIndex = i;
                }
                else
                {
                    CurrentTimeRaw = nextSnap;
                    _currentSnapLineSetIndex = i + 1;
                }
                return;
            }
        }
        
        CurrentTimeRaw = _currentSnapLineSet[^1];
        _currentSnapLineSetIndex = _currentSnapLineSet.Count - 1;
    }

    private static int TryParseGeneralChartData(string[] lines, int index, string folderPath,
        out string? audioPath, out string errorMessage)
    {
        errorMessage = "";
        
        if (lines[index + 1].StartsWith("AudioFilename:"))
        {
            AudioFileName = lines[index + 1]["AudioFilename: ".Length..].Trim();
            audioPath = Path.GetFullPath($"{folderPath}/{AudioFileName}");
            Logger.Debug("Audio file path: {0}", audioPath);
            return 1;
        }

        audioPath = null;
        errorMessage = "No audio file path";
        return -1;
    }

    private static int TryParseOfficialEditorData(string[] lines, int index,
        out string errorMessage)
    {
        errorMessage = "";
        
        // technically theres a "Bookmarks" line above this one, but it just stores the timestamp of
        // every label without the text
        if (lines[index + 2].StartsWith("BookmarksPlus:"))
        {
            Logger.Debug("Parsing labels...");
            var labelData = lines[index + 2]["BookmarksPlus: ".Length..].Trim().Split(',');
            foreach (var label in labelData)
            {
                var split = label.Split('`');
                if (split.Length != 2)
                {
                    break;
                }
                if (long.TryParse(split[0], out var time))
                {
                    _labels.Add(new Label(time, split[1]));
                }
                else
                {
                    errorMessage = $"Invalid label \"{label}\"";
                    Logger.Warn(errorMessage);
                    return -1;
                }
            }
            
            return 3;
        }

        // editor data will be empty if there are no labels
        return 1;
    }
    
    private static int TryParseUnbuggableData(string[] lines, int index,
        out (double, int, double, int)? lastEditorState, out string errorMessage)
    {
        lastEditorState = null;
        errorMessage = "";

        var i = 1;
        if (lines[index + i].StartsWith("LastEditorState:"))
        {
            var split = lines[index + 1]["LastEditorState:".Length..].Trim().Split(',');
            
            // check for length 3 and 4 because older versions of the editor don't save the current
            // cop id as part of the editor state
            if (split.Length is 3 or 4 && double.TryParse(split[0], out var time) &&
                int.TryParse(split[1], out var beatSnap) &&
                double.TryParse(split[2], out var zoom))
            {
                if (split.Length == 3)
                {
                    lastEditorState = (time, beatSnap, zoom, 0);
                }
                else if (int.TryParse(split[3], out var copId))
                {
                    lastEditorState = (time, beatSnap, zoom, copId);
                }
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
                    if (long.TryParse(split[0], out var time))
                    {
                        // for compatibility with pre 0.13 charts with 1-color markers
                        var color1 = false;
                        var color2 = false;
                        var color3 = false;
                        if (split[1].Length == 1)
                        {
                            switch (split[1])
                            {
                                case "0":
                                    color1 = true;
                                    break;
                                case "1":
                                    color2 = true;
                                    break;
                                case "2":
                                    color3 = true;
                                    break;
                            }
                        }
                        else
                        {
                            color1 = split[1][0] == '1';
                            color2 = split[1][1] == '1';
                            color3 = split[1][2] == '1';
                        }
                        
                        AddOrUpdateMarker(time, color1, color2, color3);
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
    
    private static int TryParseMetadata(string[] lines, int index, out string errorMessage)
    {
        errorMessage = "";
        
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
                Logger.Debug("Song name: {0}", Metadata.SongName);
                hasTitle = true;
            }
            else if (line.StartsWith("ArtistUnicode:"))
            {
                Metadata.ArtistName = line["ArtistUnicode:".Length..].Trim();
                Logger.Debug("Artist name: {0}", Metadata.ArtistName);
                hasArtist = true;
            }
            else if (line.StartsWith("Creator:"))
            {
                Metadata.CharterName = line["Creator:".Length..].Trim();
                Logger.Debug("Charter name: {0}", Metadata.CharterName);
                hasCharterName = true;
            }
            else if (line.StartsWith("Version:"))
            {
                // the version is only used for the in-game difficulty name; difficulty slot is
                // determined by the filename
                Metadata.DifficultyName = line["Version:".Length..].Trim();
                Logger.Debug("Difficulty name: \"{0}\"", Metadata.DifficultyName);
                hasDifficulty = true;
            }
            else if (line.StartsWith("Tags:"))
            {
                try
                {
                    var match = TagRegex().Match(line);
                    Console.WriteLine(match.Success);
                    if (match.Success)
                    {
                        Metadata.DifficultyLevel = int.Parse(match.Groups[1].Value);
                        Logger.Debug("Difficulty level: {0}", Metadata.DifficultyLevel);
                        hasLevelTag = true;
                        
                        Metadata.FlavorText = Regex.Unescape(match.Groups[2].Value);
                        Logger.Debug("Flavor text: {0}", Metadata.FlavorText);
                        hasFlavorTextTag = true;
                        
                        Metadata.CoverArtistName = Regex.Unescape(match.Groups[4].Value);
                        Logger.Debug("Cover artist: {0}", Metadata.CoverArtistName);
                        hasCoverArtTag = true;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Could not parse tags");
                    errorMessage = $"Could not parse tags: {e.Message}";
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
            var errorMessageBuilder = new StringBuilder(
                "Chart is missing one or more required metadata fields (or their values were " +
                "invalid): ");
            if (!hasTitle)
            {
                errorMessageBuilder.Append("Title, ");
            }

            if (!hasArtist)
            {
                errorMessageBuilder.Append("Artist, ");
            }

            if (!hasCharterName)
            {
                errorMessageBuilder.Append("Charter Name, ");
            }

            if (!hasDifficulty)
            {
                errorMessageBuilder.Append("Difficulty name (uses the Version field), ");
            }

            if (!hasLevelTag)
            {
                errorMessageBuilder.Append("Difficulty level (in the Tags object), ");
            }
            
            if (!hasFlavorTextTag)
            {
                errorMessageBuilder.Append("Flavor text (in the Tags object), ");
            }

            if (!hasCoverArtTag)
            {
                errorMessageBuilder.Append("Cover artist (in the Tags object), ");
            }

            errorMessage = errorMessageBuilder.ToString();
            Logger.Error(errorMessage);
            return -1;
        }
        
        App.MainWindowViewModel.SongNameText = Metadata.SongName;
        App.MainWindowViewModel.ArtistNameText = Metadata.ArtistName;

        var difficultySlotName = Metadata.DifficultySlot switch
        {
            DifficultySlot.BEGINNER => "Beginner",
            DifficultySlot.NORMAL => "Normal",
            DifficultySlot.HARD => "Hard",
            DifficultySlot.EXPERT => "Expert",
            DifficultySlot.UNBEATABLE => "UNBEATABLE",
            _ => "Star"
        };
        App.MainWindowViewModel.DifficultyText = $"{difficultySlotName} " +
                                                 $"{Metadata.DifficultyLevel}";
        App.MainWindowViewModel.CanSave = (_metadata.SongName != "" &&
                                           _metadata.ArtistName != "" &&
                                           _metadata.CharterName != "");
        return i;
    }
    
    private static int TryParseTimingPoints(string[] lines, int index, out string errorMessage)
    {
        errorMessage = "";
        
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
                long regionStart;
                double msPerBeat;
                try
                {
                    regionStart = long.Parse(numbers[0]);
                    msPerBeat = double.Parse(numbers[1]);
                }
                catch (Exception e)
                {
                    errorMessage = $"Could not parse timing point: {e.Message}";
                    Logger.Error(e, "Could not parse timing point");
                    return -1;
                }
                
                // the start time of the first region determines chart offset
                if (_bpmRegions.Count == 0)
                {
                    Metadata.ChartOffset = regionStart;
                }

                _bpmRegions.Add(new BpmRegion(regionStart - Metadata.ChartOffset,
                                              60000 / msPerBeat));

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
            errorMessage = "Chart has no timing points.";
            Logger.Error(errorMessage);
            return -1;
        }
        
        Logger.Debug("Chart has {0} BPM regions/timing points.", _bpmRegions.Count);
        return i;
    }

    private static int TryParseHitObjects(string[] lines, int index, out string errorMessage)
    {
        errorMessage = "";
        
        var i = 1;
        for (; index + i < lines.Length; ++i)
        {
            if (lines[index + i] == "" || lines[index + i] == "\r" || lines[index + i] == "\n" ||
                lines[index + i] == "\r\n")
            {
                break;
            }
            
            var note = NoteBase.FromHitObjectString(lines[index + i].Trim(),
                                                    out var noteErrorMessage);
            if (note != null)
            {
                // merge stacked camera notes into a single note with both flags
                if (NonMarkerNotes.Count > 0 && note.Type == NoteType.CAMERA_ZOOM)
                {
                    var prevNote = NonMarkerNotes[^1];
                    if (prevNote.Type == NoteType.CAMERA_SWAP && prevNote.Time == note.Time - 1)
                    {
                        prevNote.Flags.C = true;
                        prevNote.Flags.W = true;
                    }
                    else
                    {
                        AddNote(note);
                    }
                }
                else
                {
                    AddNote(note);
                }
            }
            else if (noteErrorMessage != "marker")
            {
                errorMessage = $"Could not parse note: {noteErrorMessage}";
                Logger.Error("Could not parse note: {0}", noteErrorMessage);
                return -1;
            }
        }
        
        Logger.Debug("Chart has {0} notes.", _notes.Count);
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
            bookmarks.Add(label.Time.ToString());
            bookmarksPlus.Add($"{label.Time}`{label.Text}");
        }
        await writer.WriteLineAsync($"Bookmarks: {string.Join(",", bookmarks)}");
        await writer.WriteLineAsync($"BookmarksPlus: {string.Join(",", bookmarksPlus)}");
    }

    private static async Task WriteUnbuggableData(StreamWriter writer)
    {
        await writer.WriteLineAsync("[UNBUGGABLE]");
        await writer.WriteLineAsync(
            $"LastEditorState:{CurrentTime},{BeatSnap},{NoteViewer.CurrentZoom}," +
            $"{ChartBuilder.CopId}");
        
        List<string> markerStrings = [];
        foreach (var marker in MarkerNotes)
        {
            var m = (MarkerNote)marker;
            var colorStatesString = (m.Color1 ? "1" : "0") +
                                    (m.Color2 ? "1" : "0") +
                                    (m.Color3 ? "1" : "0");
            markerStrings.Add($"{m.Time}`{colorStatesString}");
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
        // this has to use its own serializer options because chart files don't load if the tag json
        // is pretty printed
        await writer.WriteLineAsync(
            $"Tags:{JsonSerializer.Serialize(tags, MetadataJsonSerializerOptions)}");
    }
    
    private static async Task WriteTimingPoints(StreamWriter writer)
    {
        await writer.WriteLineAsync("[TimingPoints]");
        var lines = new StringBuilder();
        var first = true;
        foreach (var bpmRegion in _bpmRegions)
        {
            var time = bpmRegion.StartTime + Metadata.ChartOffset;
            
            // why does this use 9 decimal places???
            var line = $"{time},{bpmRegion.MsPerBeat:0.000000000}";
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