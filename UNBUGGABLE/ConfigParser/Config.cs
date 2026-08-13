using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Tmds.DBus.Protocol;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Input;
using UNBUGGABLE.Keybinds;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UNBUGGABLE.Resources;

/// <summary>
/// Loads color themes and user settings when the app starts.
/// </summary>
public static class Config
{
    public static Dictionary<string, Color> CurrentTheme { get; private set; } = new()
    {
        { "Accent", Color.Parse("#FF4B7E") },
        { "WindowBackgroundPrimary", Color.Parse("#FAF7D6") },
        { "WindowBackgroundSecondary", Color.Parse("#C9C9A5") },
        { "EditorBackground", Color.Parse("#1B1F21") },
        { "TextPrimary", Color.Parse("#FFFFFF") },
        { "TextSecondary", Color.Parse("#D0D0D0") },
        { "TextDark", Color.Parse("#161616") },
        { "SingleNote", Color.Parse("#9C999C") },
        { "Spike", Color.Parse("#FFCC00") },
        { "DoubleNote", Color.Parse("#65CCFF") },
        { "Freestyle", Color.Parse("#FF9A9A") },
        { "NoteOutline", Color.Parse("#000000") },
        { "SelectedNoteOverlay", Color.Parse("#FFFFFF") },
        { "SetpieceNoteOverlay", Color.Parse("#FF0000") },
        { "SelectDragOverlay", Color.Parse("#FFFFFF") },
        { "DeleteDragOverlay", Color.Parse("#FF0000") },
        { "CameraChange", Color.Parse("#FBB7DE") },
        { "ViewableArea", Color.Parse("#FBB7DE") },
        { "FullBeatSnapLine", Color.Parse("#E0E0E0") },
        { "SubBeatSnapLine", Color.Parse("#808080") },
        { "CurrentTimeLine", Color.Parse("#FF0000") },
        { "Breakpoint", Color.Parse("#FF0000") },
        { "Marker1", Color.Parse("#40DB11") },
        { "Marker2", Color.Parse("#0979EA") },
        { "Marker3", Color.Parse("#B609EA") },
        { "BpmChange", Color.Parse("#0981EA") },
        { "Label", Color.Parse("#EADF09") },
        { "Cop1", Color.Parse("#3259E5") },
        { "Cop2", Color.Parse("#ED4964") },
        { "Cop3", Color.Parse("#44F430") },
        { "Cop4", Color.Parse("#F4E430") },
    };

    public static Settings Settings { get; private set; } = new();

    public static bool PracticeModInstalled { get; private set; } = false;
    public static string PracticeModConfigPath { get; private set; } = "";

    public static Keybinds Keybinds { get; private set; } = new();

    public static bool LoadError { get; private set; } = false;

    /// <summary>
    /// Default starting location for saving and loading files.
    /// </summary>
    public static string CustomSongsDirectory { get; private set; } = "";

    private static readonly string ConfigFilePath = Path.Combine(Environment.CurrentDirectory,
                                                                 "configs/config.json");

    private static readonly string KeybindFilePath = Path.Combine(Environment.CurrentDirectory,
                                                                  "configs/keybinds.json");

    private static readonly string ThemesFolderPath = Path.Combine(Environment.CurrentDirectory,
                                                                   "themes");

    private static readonly Dictionary<string, ColorTheme> ColorThemes = new();
    
    /// <summary>
    /// Loads and parses user settings and color themes.
    /// </summary>
    public static void InitialLoadAllConfigFiles()
    {
        // updatedConfig and updatedKeybinds are temporary files used for preserving existing
        // settings after an update
        var updatedConfigPath = Path.Combine(Environment.CurrentDirectory,
                                             "configs/updatedConfig.json");
        if (File.Exists(updatedConfigPath))
        {
            Trace.WriteLine("updating configs");
            var fullCopy =
                !File.Exists(ConfigFilePath) ||
                !JsonHelper.TryMergeFiles(ConfigFilePath, updatedConfigPath, ConfigFilePath);
            if (fullCopy)
            {
                Trace.WriteLine(
                    "Keybinds.json does not exist (or was invalid), fully copying updated file");
                File.Move(updatedConfigPath, ConfigFilePath);
            }

            File.Delete(updatedConfigPath);
        }

        var updatedKeybindsPath = Path.Combine(Environment.CurrentDirectory,
                                               "configs/updatedKeybinds.json");
        if (File.Exists(updatedKeybindsPath))
        {
            Trace.WriteLine("updating keybinds");
            var fullCopy =
                !File.Exists(KeybindFilePath) ||
                !JsonHelper.TryMergeFiles(KeybindFilePath, updatedKeybindsPath, KeybindFilePath);
            if (fullCopy)
            {
                Trace.WriteLine(
                    "Keybinds.json does not exist (or was invalid), fully copying updated file");
                File.Move(updatedKeybindsPath, KeybindFilePath);
            }
            
            File.Delete(updatedKeybindsPath);
        }
        
        TryReloadConfig(false);
    }

    public static void TryReloadConfig(bool mainWindowInitialized = true)
    {
        Trace.WriteLine("\n-- Reloading Config --");
        
        var loadError = !TryLoadColorThemes();
        if (!loadError)
        {
            loadError = !TryLoadKeybinds();
        }
        if (!loadError)
        {
            loadError = !TryLoadConfig();
        }

        // load errors and some other things are skipped on the first load -- without this, it'll
        // try to spawn ui components that can't exist and the entire thing will crash and burn
        if (mainWindowInitialized)
        {
            App.MainWindowViewModel.SliderIncrement = Settings.SliderIncrement;
            if (loadError)
            {
                LoadError = true;
                // line break between the end of config loading and everything else
                Trace.WriteLine("\n");
                return;
            }
        }
        
        ThemeManager.ApplyTheme(ColorThemes[Settings.ColorTheme]);
        Trace.WriteLine($"applied theme \"{Settings.ColorTheme}\"\n");

        if (Chart.SongLoaded)
        {
            Chart.RebuildJumpTargets();
        }
        
        LoadError = false;
    }

    private static bool TryLoadKeybinds()
    {
        Trace.WriteLine("\n-- Loading Keybinds --");
        try
        {
            var keybinds = JsonSerializer.Deserialize<Keybinds>(File.ReadAllText(KeybindFilePath));
            if (keybinds != null &&
                VerifyKeybindStrings(keybinds.Undo, "undo") &&
                VerifyKeybindStrings(keybinds.Redo, "redo") &&
                VerifyKeybindStrings(keybinds.SaveFile, "saveFile") &&
                VerifyKeybindStrings(keybinds.OpenFile, "openFile") &&
                VerifyKeybindStrings(keybinds.ResetPlaySpeed, "resetPlaySpeed") &&
                VerifyKeybindStrings(keybinds.ReloadConfig, "reloadConfig") &&
                VerifyKeybindStrings(keybinds.MoveForward, "moveForward") &&
                VerifyKeybindStrings(keybinds.MoveBack, "moveBack") &&
                VerifyKeybindStrings(keybinds.QuickScrollModifier, "quickScrollModifier") &&
                VerifyKeybindStrings(keybinds.PlayPause, "playPause") &&
                VerifyKeybindStrings(keybinds.ZoomIn, "zoomIn") &&
                VerifyKeybindStrings(keybinds.ZoomOut, "zoomOut") &&
                VerifyKeybindStrings(keybinds.PrevLabel, "prevLabel") &&
                VerifyKeybindStrings(keybinds.NextLabel, "nextLabel") &&
                VerifyKeybindStrings(keybinds.PrevNoteSnap, "prevNoteSnap") &&
                VerifyKeybindStrings(keybinds.NextNoteSnap, "nextNoteSnap") &&
                VerifyKeybindStrings(keybinds.PlaceTopLane, "placeTopLane") &&
                VerifyKeybindStrings(keybinds.PlaceBottomLane, "placeBottomLane") &&
                VerifyKeybindStrings(keybinds.PlaceCameraLane, "placeCameraLane") &&
                VerifyKeybindStrings(keybinds.PlaceCenterLane, "placeCenterLane") &&
                VerifyKeybindStrings(keybinds.SelectAll, "selectAll") &&
                VerifyKeybindStrings(keybinds.SelectNonMarker, "selectNonMarker") &&
                VerifyKeybindStrings(keybinds.SelectTopLane, "selectTopLane") &&
                VerifyKeybindStrings(keybinds.SelectBottomLane, "selectBottomLane") &&
                VerifyKeybindStrings(keybinds.SelectCameraLane, "selectCameraLane") &&
                VerifyKeybindStrings(keybinds.SelectCenterLane, "selectCenterLane") &&
                VerifyKeybindStrings(keybinds.Cut, "cut") &&
                VerifyKeybindStrings(keybinds.Copy, "copy") &&
                VerifyKeybindStrings(keybinds.Paste, "paste") &&
                VerifyKeybindStrings(keybinds.ClearSelection, "clearSelection") &&
                VerifyKeybindStrings(keybinds.DeleteSelection, "deleteSelection") &&
                VerifyKeybindStrings(keybinds.MirrorSelection, "mirrorSelection") &&
                VerifyKeybindStrings(keybinds.MoveSelectionForward, "moveSelectionForward") &&
                VerifyKeybindStrings(keybinds.MoveSelectionBack, "moveSelectionBack") &&
                VerifyKeybindStrings(keybinds.SetFinishFlag, "setFinishFlag") &&
                VerifyKeybindStrings(keybinds.LockFinishFlag, "lockFinishFlag") &&
                VerifyKeybindStrings(keybinds.SetWhistleFlag, "setWhistleFlag") &&
                VerifyKeybindStrings(keybinds.LockWhistleFlag, "lockWhistleFlag") &&
                VerifyKeybindStrings(keybinds.SetClapFlag, "setClapFlag") &&
                VerifyKeybindStrings(keybinds.LockClapFlag, "lockClapFlag") &&
                VerifyKeybindStrings(keybinds.SetNoiszFlag, "setNoiszSpawn") &&
                VerifyKeybindStrings(keybinds.LockNoiszFlag, "lockNoiszSpawn") &&
                VerifyKeybindStrings(keybinds.CopId0, "copId0") &&
                VerifyKeybindStrings(keybinds.CopId1, "copId1") &&
                VerifyKeybindStrings(keybinds.CopId2, "copId2") &&
                VerifyKeybindStrings(keybinds.CopId3, "copId3") &&
                VerifyKeybindStrings(keybinds.CopId4, "copId4") &&
                VerifyKeybindStrings(keybinds.PrevCop, "prevCop") &&
                VerifyKeybindStrings(keybinds.NextCop, "nextCop") &&
                VerifyKeybindStrings(keybinds.AddBpmChange, "addBpmChange") &&
                VerifyKeybindStrings(keybinds.RemoveBpmChange, "removeBpmChange") &&
                VerifyKeybindStrings(keybinds.AddLabel, "addLabel") &&
                VerifyKeybindStrings(keybinds.RemoveLabel, "removeLabel") &&
                VerifyKeybindStrings(keybinds.AddMarker1, "addMarker1") &&
                VerifyKeybindStrings(keybinds.AddMarker2, "addMarker2") &&
                VerifyKeybindStrings(keybinds.AddMarker3, "addMarker3") &&
                VerifyKeybindStrings(keybinds.SetBreakpoint, "setBreakpoint") &&
                VerifyKeybindStrings(keybinds.RemoveBreakpoint, "removeBreakpoint") &&
                VerifyKeybindStrings(keybinds.JumpToBreakpoint, "jumpToBreakpoint") &&
                VerifyKeybindStrings(keybinds.EmergencyReload, "emergencyReload") &&
                VerifyKeybindStrings(keybinds.NudgeForward, "nudgeForward") &&
                VerifyKeybindStrings(keybinds.NudgeBack, "nudgeBack") &&
                VerifyKeybindStrings(keybinds.NudgeTailForward, "nudgeTailForward") &&
                VerifyKeybindStrings(keybinds.NudgeTailBack, "nudgeTailBack"))
            {
                Keybinds = keybinds;
            }
            else
            {
                return false;
            }
        }
        catch (JsonException e)
        {
            Trace.WriteLine($"JSON parse error: {e.Message}");
            return false;
        }

        InputManager.Actions =
        [
            new UndoAction(Keybinds.Undo),
            new RedoAction(Keybinds.Redo),
            new SaveFileAction(Keybinds.SaveFile),
            new OpenFileAction(Keybinds.OpenFile),
            new ResetPlaySpeedAction(Keybinds.ResetPlaySpeed),
            new ReloadConfigCommand(Keybinds.ReloadConfig),
            new ZoomInAction(Keybinds.ZoomIn),
            new ZoomOutAction(Keybinds.ZoomOut),
            new MoveForwardAction(Keybinds.MoveForward),
            new MoveBackAction(Keybinds.MoveBack),
            new QuickScrollModifierAction(Keybinds.QuickScrollModifier),
            new PlayPauseAction(Keybinds.PlayPause),
            new PrevLabelAction(Keybinds.PrevLabel),
            new NextLabelAction(Keybinds.NextLabel),
            new PrevNoteSnapAction(Keybinds.PrevNoteSnap),
            new NextNoteSnapAction(Keybinds.NextNoteSnap),
            new PlaceTopLaneAction(Keybinds.PlaceTopLane),
            new PlaceBottomLaneAction(Keybinds.PlaceBottomLane),
            new PlaceCameraLaneAction(Keybinds.PlaceCameraLane),
            new PlaceCenterLaneAction(Keybinds.PlaceCenterLane),
            new NudgeAction(Keybinds.NudgeForward, true, 1),
            new NudgeAction(Keybinds.NudgeBack, true, -1),
            new NudgeAction(Keybinds.NudgeTailForward, false, 1),
            new NudgeAction(Keybinds.NudgeTailBack, false, -1),
            new SelectAllAction(Keybinds.SelectAll),
            new SelectNonMarkerAction(Keybinds.SelectNonMarker),
            new SelectLaneAction(Keybinds.SelectTopLane, NoteLane.TOP),
            new SelectLaneAction(Keybinds.SelectBottomLane, NoteLane.BOTTOM),
            new SelectLaneAction(Keybinds.SelectCameraLane, NoteLane.CAMERA),
            new SelectLaneAction(Keybinds.SelectCenterLane, NoteLane.CENTER),
            new CutAction(Keybinds.Cut),
            new CopyAction(Keybinds.Copy),
            new PasteAction(Keybinds.Paste),
            new ClearSelectionAction(Keybinds.ClearSelection),
            new DeleteSelectionAction(Keybinds.DeleteSelection),
            new MirrorSelectionAction(Keybinds.MirrorSelection),
            new MoveSelectionForwardAction(Keybinds.MoveSelectionForward),
            new MoveSelectionBackAction(Keybinds.MoveSelectionBack),
            new SetNoteFlagAction(Keybinds.SetFinishFlag, 'f'),
            new SetNoteFlagAction(Keybinds.SetWhistleFlag, 'w'),
            new SetNoteFlagAction(Keybinds.SetClapFlag, 'c'),
            new SetNoteFlagAction(Keybinds.SetNoiszFlag, 'n'),
            new LockNoteFlagAction(Keybinds.LockFinishFlag, 'f'),
            new LockNoteFlagAction(Keybinds.LockWhistleFlag, 'w'),
            new LockNoteFlagAction(Keybinds.LockClapFlag, 'c'),
            new LockNoteFlagAction(Keybinds.LockNoiszFlag, 'n'),
            new CopId0Action(Keybinds.CopId0),
            new CopId1Action(Keybinds.CopId1),
            new CopId2Action(Keybinds.CopId2),
            new CopId3Action(Keybinds.CopId3),
            new CopId4Action(Keybinds.CopId4),
            new PrevCopAction(Keybinds.PrevCop),
            new NextCopAction(Keybinds.NextCop),
            new AddBpmChangeAction(Keybinds.AddBpmChange),
            new RemoveBpmChangeAction(Keybinds.RemoveBpmChange),
            new AddLabelAction(Keybinds.AddLabel),
            new RemoveLabelAction(Keybinds.RemoveLabel),
            new AddMarkerAction(Keybinds.AddMarker1, 0),
            new AddMarkerAction(Keybinds.AddMarker2, 1),
            new AddMarkerAction(Keybinds.AddMarker3, 2),
            new SetBreakpointAction(Keybinds.SetBreakpoint),
            new RemoveBreakpointAction(Keybinds.RemoveBreakpoint),
            new JumpToBreakpointCommand(Keybinds.JumpToBreakpoint),
            new EmergencyReloadAction(Keybinds.EmergencyReload)
        ];

        Trace.WriteLine("Keybinds loaded successfully!");
        return true;
    }

    private static bool TryLoadConfig()
    {
        Trace.WriteLine("\n-- Loading Config --");
        
        var loadError = false;
        try
        {
            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(ConfigFilePath));
            if (settings != null)
            {
                if (settings.MinZoom <= 0)
                {
                    Trace.WriteLine("min zoom must be > 0");
                    settings.MinZoom = 0.5;
                    loadError = true;
                }

                if (settings.MaxZoom <= 0)
                {
                    Trace.WriteLine("max zoom must be > 0");
                    settings.MaxZoom = 7.5;
                    loadError = true;
                }

                if (settings.MinZoom > settings.MaxZoom)
                {
                    Trace.WriteLine("min zoom must be <= max zoom");
                    settings.MinZoom = 0.5;
                    settings.MaxZoom = 7.5;
                    loadError = true;
                }

                if (settings.ZoomIncrement == 0)
                {
                    Trace.WriteLine("zoom increment must be nonzero");
                    settings.ZoomIncrement = 0.25;
                    loadError = true;
                }
                
                if (settings.QuickScrollBeats <= 0)
                {
                    Trace.WriteLine("quick scroll beats must be > 0");
                    settings.QuickScrollBeats = 5;
                    loadError = true;
                }
                
                if (settings.SliderIncrement <= 0)
                {
                    Trace.WriteLine("slider increment must be > 0");
                    settings.SliderIncrement = 5;
                    loadError = true;
                }

                if (settings.BeatSnaps.Count == 0)
                {
                    Trace.WriteLine("no beat snaps");
                    settings.BeatSnaps = [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 20, 5, 9, 11, 13];
                    loadError = true;
                }

                if (settings.BeatSnaps.Any(snap => snap <= 0))
                {
                    Trace.WriteLine("beat snaps must be > 0");
                    settings.BeatSnaps = [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 20, 5, 9, 11, 13];
                    loadError = true;
                }

                settings.BeatSnaps = settings.BeatSnaps.Distinct().ToList();
                // TONS of things in the chart code depend on the first beat snap being one beat
                if (settings.BeatSnaps[0] != 1)
                {
                    settings.BeatSnaps.Remove(1);
                    settings.BeatSnaps.Insert(0, 1);
                }

                var invalidLaneOrder = (settings.LaneOrder.Count != 4 ||
                                        settings.LaneOrder.Count !=
                                        settings.LaneOrder.Distinct().Count());

                if (!invalidLaneOrder)
                {
                    bool hasTop = false, hasBottom = false, hasCamera = false, hasCenter = false;
                    foreach (var lane in settings.LaneOrder)
                    {
                        switch (lane)
                        {
                            case "top":
                                hasTop = true;
                                break;
                            case "bottom":
                                hasBottom = true;
                                break;
                            case "camera":
                                hasCamera = true;
                                break;
                            case "center":
                                hasCenter = true;
                                break;
                        }
                    }
                    
                    invalidLaneOrder = !hasTop || !hasBottom || !hasCamera || !hasCenter;
                }

                if (invalidLaneOrder)
                {
                    Trace.WriteLine("Invalid lane order");
                    settings.LaneOrder = ["top", "center", "bottom", "camera"];
                    loadError = true;
                }

                if (settings.JumpTargets.Count == 0)
                {
                    Trace.WriteLine("No jump targets.");
                    settings.JumpTargets = [
                        "labels",
                        "bpmChanges",
                        "firstNote",
                        "lastNote",
                        "chartStart",
                        "chartEnd"
                    ];
                    loadError = true;
                }
                else
                {
                    settings.JumpTargets = settings.JumpTargets.Distinct().ToList();
                    var invalidJumpTarget = false;
                    List<string> allowedTargets = [
                        "labels",
                        "bpmChanges",
                        "firstNote",
                        "lastNote",
                        "firstMarker",
                        "lastMarker",
                        "chartStart",
                        "chartEnd",
                        "breakpoint"
                    ];
                    
                    foreach (var target in settings.JumpTargets)
                    {
                        if (!allowedTargets.Contains(target))
                        {
                            invalidJumpTarget = true;
                            Trace.WriteLine($"Invalid jump target \"{target}\"");
                        }
                    }

                    if (invalidJumpTarget)
                    {
                        settings.JumpTargets = [
                            "labels",
                            "bpmChanges",
                            "firstNote",
                            "lastNote",
                            "chartStart",
                            "chartEnd"
                        ];
                        loadError = true;
                    }
                }

                if (settings.PasteBehavior != "none" && settings.PasteBehavior != "notes" &&
                    settings.PasteBehavior != "region")
                {
                    Trace.WriteLine("Invalid paste overwrite setting: must be \"none\", " +
                                      "\"notes\", or \"region\".");
                    settings.PasteBehavior = "region";
                    loadError = true;
                }

                if (settings.AutoSelectBehavior != "none" &&
                    settings.AutoSelectBehavior != "pasted" && settings.AutoSelectBehavior != "all")
                {
                    Trace.WriteLine("Invalid auto select setting: must be \"none\", " +
                                      "\"pasted\", or \"all\".");
                    settings.AutoSelectBehavior = "pasted";
                    loadError = true;
                }

                if (settings.HoldTailSelect != "first" && settings.HoldTailSelect != "last" &&
                    settings.HoldTailSelect != "all" && settings.HoldTailSelect != "none")
                {
                    Trace.WriteLine("Invalid hold tail select settings: must be \"first\", " +
                                      "\"last\", \"all\", or \"none\"");
                    settings.HoldTailSelect = "all";
                    loadError = true;
                }

                if (settings.QuickScrollBeats <= 0)
                {
                    Trace.WriteLine("Quick scroll beats must be > 0");
                    loadError = true;
                }

                Settings = settings;
                // Trace.WriteLine("Loaded settings:");
                // Settings.PrintSettings();
            }
            else
            {
                Trace.WriteLine("Could not parse config: file is empty.");
                loadError = true;
            }
        }
        catch (JsonException e)
        {
            Trace.WriteLine($"JSON parse error: {e.Message}");
            loadError = true;
        }
        // Settings.PrintSettings();

        // look for your custom songs directory
        var gameDataDirectory = Path.GetFullPath(
            "../LocalLow/D-CELL GAMES/UNBEATABLE",
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        CustomSongsDirectory = (Directory.Exists(Path.Combine(gameDataDirectory, "CustomSongs"))
            ? Path.Combine(gameDataDirectory, "CustomSongs")
            : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        // breakpoints also require stefy's practice mod to be installed
        if (Settings.EnableBreakpoints)
        {
            PracticeModConfigPath = Path.Combine(gameDataDirectory, "practice-mode-settings.txt");
            if (File.Exists(PracticeModConfigPath))
            {
                Trace.WriteLine("Found Practice Mod, enabling breakpoints.");
                PracticeModInstalled = true;
            }
            else
            {
                Trace.WriteLine("Install Practice Mod to enable breakpoints.");
                PracticeModInstalled = false;
            }
        }

        if (!loadError)
        {
            Trace.WriteLine("Config loaded successfully!");
        }
        return !loadError;
    }

    private static bool TryLoadColorThemes()
    {
        if (Directory.Exists(ThemesFolderPath))
        {
            ColorThemes.Clear();
            foreach (var file in Directory.EnumerateFiles(ThemesFolderPath))
            {
                Trace.WriteLine($"-- Loading color theme file \"{Path.GetFileName(file)}\" --");
                var themeName = Path.GetFileNameWithoutExtension(file);
                try
                {
                    var themeJson = JsonSerializer.Deserialize<ColorThemeJson>(
                        File.ReadAllText(file));
                    if (themeJson != null)
                    {
                        List<string> errors = [];
                        var theme = new ColorTheme(themeJson, ref errors);
                        
                        if (errors.Count > 0)
                        {
                            Trace.WriteLine("Errors encountered while loading theme:");
                            foreach (var error in errors)
                            {
                                Trace.WriteLine($"- {error}");
                            }
                        }
                        else
                        {
                            ColorThemes[themeName] = theme;
                            Trace.WriteLine("Load successful!\n");
                        }
                    }
                }
                catch (JsonException e)
                {
                    Trace.WriteLine($"JSON parse error: {e.Message}");
                }
            }
            return true;
        }
        
        Trace.WriteLine("Color theme folder not found.");
        return false;
    }

    private static bool VerifyKeybindStrings(List<string>? keybindStrings, string bindName)
    {
        if (keybindStrings == null)
        {
            return false;
        }
        
        foreach (var str in keybindStrings)
        {
            if (str == "")
            {
                Trace.WriteLine(
                    "Invalid keybind \"\" for input action \"{bindName\": keybind is empty");
                return false;
            }

            var split = str.Split('+').ToList();
            if (split.Count > 4)
            {
                Trace.WriteLine(
                    $"Invalid keybind \"{str}\" for input action \"{{bindName\": too many keys");
                return false;
            }

            if (split.Distinct().Count() != split.Count)
            {
                Trace.WriteLine(
                    $"Invalid keybind \"{str}\" for input action \"{{bindName\": duplicate keys");
                return false;
            }

            if (split.Count > 1)
            {
                for (var i = 0; i < split.Count - 1; ++i)
                {
                    if (split[i] != "ctrl" && split[i] != "shift" && split[i] != "alt")
                    {
                        Trace.WriteLine(
                            $"Invalid keybind \"{str}\" for input action \"{{bindName\": invalid " +
                            $"modifier");
                        return false;
                    }
                }
            }

            var validPrimaryKey = false;
            // convert the first character to uppercase to match the avalonia enum
            foreach (var enumValue in Enum.GetValues(typeof(Key)))
            {
                if (enumValue.ToString() == char.ToUpper(split[^1][0]) + split[^1][1..])
                {
                    validPrimaryKey = true;
                    break;
                }
            }

            if (!validPrimaryKey)
            {
                validPrimaryKey = 
                    split[^1] is "leftMouse" or"rightMouse" or "middleMouse" or "scrollUp" or
                        "scrollDown";
            }

            if (!validPrimaryKey)
            {
                Trace.WriteLine(
                    $"Invalid keybind \"{str}\" for input action \"{{bindName\": invalid primary " +
                    $"key");
                return false;
            }
        }       

        return true;
    }
}