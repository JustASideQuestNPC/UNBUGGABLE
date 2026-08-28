using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
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
    public static Settings Settings { get; private set; } = new();

    public static bool PracticeModInstalled { get; private set; } = false;
    public static string PracticeModConfigPath { get; private set; } = "";

    public static Keybinds Keybinds { get; private set; } = new();

    public static bool LoadError { get; private set; } = false;

    /// <summary>
    /// Default starting location for saving and loading files.
    /// </summary>
    public static string CustomSongsDirectory { get; private set; } = "";

    public static string ConfigFilePath { get; set; } = Path.Combine(Environment.CurrentDirectory,
                                                                     "configs/config.json");

    public static string KeybindFilePath { get; set; } = Path.Combine(Environment.CurrentDirectory,
                                                                      "configs/keybinds.json");
    
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private static string ThemesFolderPath { get; set; } =
        Path.Combine(Environment.CurrentDirectory, "themes");

    private static readonly Dictionary<string, ColorTheme> ColorThemes = new();

    public static void ApplyCurrentTheme()
    {
        if (ColorThemes.TryGetValue(Settings.ColorTheme, out var theme))
        {
            ThemeManager.ApplyTheme(theme);
            Logger.Debug("applied theme \"{0}\"", Settings.ColorTheme);
        }
        else
        {
            ThemeManager.ApplyTheme(ColorThemes["default"]);
            Logger.Warn("color theme \"{0}\" does not exist, falling back to default",
                        Settings.ColorTheme);
        }
    }
    
    /// <summary>
    /// Checks whether the settings and keybinds files need to be updated, then loads the settings
    /// file. This <i>does not</i> apply the current color theme.
    /// </summary>
    public static void CheckConfigFileUpdate()
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
                    "config file does not exist (or was invalid), fully copying updated file");
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
                    "keybinds file does not exist (or was invalid), fully copying updated file");
                File.Move(updatedKeybindsPath, KeybindFilePath);
            }
            
            File.Delete(updatedKeybindsPath);
        }
    }

    public static void TryReloadAllConfigs()
    {
        Logger.Info("-- Reloading Configs --");
        
        var loadError = !TryLoadColorThemes();
        if (!loadError)
        {
            loadError = !TryLoadKeybinds();
        }
        if (!loadError)
        {
            loadError = !TryLoadSettings();
        }

        // load errors and some other things are skipped on the first load -- without this, it'll
        // try to spawn ui components that can't exist and the entire thing will crash and burn
        App.MainWindowViewModel.SliderIncrement = Settings.SliderIncrement;
        if (loadError)
        {
            LoadError = true;
            
            ThemeManager.ApplyTheme(ColorThemes["default"]);
            Logger.Error("Config loading failed, falling back to default color theme");
            return;
        }
        
        ApplyCurrentTheme();

        if (Chart.SongLoaded)
        {
            Chart.RebuildJumpTargets();
        }
        
        LoadError = false;
    }
    
    public static bool TryLoadSettings()
    {
        Logger.Info("Loading settings from \"{0}\"", ConfigFilePath);
        
        var loadError = false;
        try
        {
            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(ConfigFilePath));
            if (settings != null)
            {
                if (settings.MinZoom <= 0)
                {
                    Logger.Warn("min zoom must be > 0");
                    settings.MinZoom = 0.5;
                    loadError = true;
                }

                if (settings.MaxZoom <= 0)
                {
                    Logger.Warn("max zoom must be > 0");
                    settings.MaxZoom = 7.5;
                    loadError = true;
                }

                if (settings.MinZoom > settings.MaxZoom)
                {
                    Logger.Warn("min zoom must be <= max zoom");
                    settings.MinZoom = 0.5;
                    settings.MaxZoom = 7.5;
                    loadError = true;
                }

                if (settings.ZoomIncrement == 0)
                {
                    Logger.Warn("zoom increment must be nonzero");
                    settings.ZoomIncrement = 0.25;
                    loadError = true;
                }
                
                if (settings.QuickScrollBeats <= 0)
                {
                    Logger.Warn("quick scroll beats must be > 0");
                    settings.QuickScrollBeats = 5;
                    loadError = true;
                }
                
                if (settings.SliderIncrement <= 0)
                {
                    Logger.Warn("slider increment must be > 0");
                    settings.SliderIncrement = 5;
                    loadError = true;
                }

                if (settings.BeatSnaps.Count == 0)
                {
                    Logger.Warn("no beat snaps");
                    settings.BeatSnaps = [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 20, 5, 9, 11, 13];
                    loadError = true;
                }

                if (settings.BeatSnaps.Any(snap => snap <= 0))
                {
                    Logger.Warn("beat snaps must be > 0");
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
                    Logger.Warn("Invalid lane order");
                    settings.LaneOrder = ["top", "center", "bottom", "camera"];
                    loadError = true;
                }

                if (settings.JumpTargets.Count == 0)
                {
                    Logger.Warn("No jump targets.");
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
                        "secondLastNote",
                        "firstMarker",
                        "lastMarker",
                        "breakpoint"
                    ];
                    
                    foreach (var target in settings.JumpTargets)
                    {
                        if (!allowedTargets.Contains(target))
                        {
                            invalidJumpTarget = true;
                            Logger.Warn($"Invalid jump target \"{target}\"");
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
                    Logger.Warn("Invalid paste overwrite setting: must be \"none\", " +
                                "\"notes\", or \"region\".");
                    settings.PasteBehavior = "region";
                    loadError = true;
                }

                if (settings.AutoSelectBehavior != "none" &&
                    settings.AutoSelectBehavior != "pasted" && settings.AutoSelectBehavior != "all")
                {
                    Logger.Warn("Invalid auto select setting: must be \"none\", " +
                                "\"pasted\", or \"all\".");
                    settings.AutoSelectBehavior = "pasted";
                    loadError = true;
                }

                if (settings.HoldTailSelect != "first" && settings.HoldTailSelect != "last" &&
                    settings.HoldTailSelect != "all" && settings.HoldTailSelect != "none")
                {
                    Logger.Warn("Invalid hold tail select settings: must be \"first\", " +
                                "\"last\", \"all\", or \"none\"");
                    settings.HoldTailSelect = "all";
                    loadError = true;
                }

                if (settings.QuickScrollBeats <= 0)
                {
                    Logger.Warn("Quick scroll beats must be > 0");
                    settings.QuickScrollBeats = 5;
                    loadError = true;
                }

                if (settings.SliderIncrement <= 0)
                {
                    Logger.Warn("Slider increment must be > 0");
                    settings.SliderIncrement = 5;
                    loadError = true;
                }

                if (settings.HoldExtensionSearchThreshold < 0)
                {
                    Logger.Warn("Hold extension search threshold must be >= 0");
                    settings.HoldExtensionSearchThreshold = 2;
                    loadError = true;
                }

                if (settings.DefaultDifficulty != "beginner" &&
                    settings.DefaultDifficulty != "normal" &&
                    settings.DefaultDifficulty != "hard" &&
                    settings.DefaultDifficulty != "expert" &&
                    settings.DefaultDifficulty != "unbeatable" &&
                    settings.DefaultDifficulty != "UNBEATABLE" &&
                    settings.DefaultDifficulty != "star")
                {
                    Logger.Warn("Invalid default difficulty: Should be \"beginner\"," +
                                "\"normal\", \"hard\", \"expert\", \"UNBEATABLE\", or" +
                                "\"star\"");
                    settings.DefaultDifficulty = "beginner";
                    loadError = true;
                }
                else
                {
                    settings.DefaultDifficulty = settings.DefaultDifficulty.ToLower();
                }

                Settings = settings;
                Logger.Info("Loaded settings:");
            }
            else
            {
                Logger.Error("Could not parse config: file is empty.");
                loadError = true;
            }
        }
        catch (JsonException e)
        {
            Logger.Error(e, "JSON error while parsing config");
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
                Logger.Info("Found Practice Mod, enabling breakpoints.");
                PracticeModInstalled = true;
            }
            else
            {
                Logger.Warn("Install Practice Mod to enable breakpoints.");
                PracticeModInstalled = false;
            }
        }

        if (!loadError)
        {
            Logger.Info("Config loaded successfully:\r\n{0}", Settings.ToString());
        }
        return !loadError;
    }

    public static bool TryLoadKeybinds()
    {
        Logger.Info("Loading keybinds from \"{0}\"", KeybindFilePath);
        
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
            Logger.Error(e, "JSON error while parsing keybinds");
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

        Logger.Info("Keybinds loaded successfully!");
        return true;
    }

    public static bool TryLoadColorThemes()
    {
        List<string> errors = [];
        
        if (Directory.Exists(ThemesFolderPath))
        {
            ColorThemes.Clear();
            foreach (var file in Directory.EnumerateFiles(ThemesFolderPath))
            {
                Logger.Debug("Loading color theme file \"{0}\"", Path.GetFileName(file));
                var themeName = Path.GetFileNameWithoutExtension(file);
                try
                {
                    var themeJson = JsonSerializer.Deserialize<ColorThemeJson>(
                        File.ReadAllText(file));
                    if (themeJson != null)
                    {
                        errors = [];
                        var theme = new ColorTheme(themeJson, ref errors);
                        
                        if (errors.Count > 0)
                        {
                            // Logger.Error("Errors encountered while loading theme:");

                            var errorString = new StringBuilder();
                            foreach (var error in errors)
                            {
                                errorString.Append($"- {error}\r\n");
                            }
                            
                            Logger.Error("Errors encountered while loading theme \"{0}\":\r\n{1}",
                                         Path.GetFileName(file), errorString.ToString().Trim());
                        }
                        else
                        {
                            ColorThemes[themeName] = theme;
                            Logger.Debug("Load successful!\n");
                        }
                    }
                }
                catch (JsonException e)
                {
                    Logger.Error(e, "JSON error while parsing theme \"{0}\"", themeName);
                }
            }
            
            // ensure that the default theme exists -- this is guaranteed to work because it's using
            // the hard-coded default json values
            if (!ColorThemes.ContainsKey("default"))
            {
                errors = [];
                ColorThemes["default"] = new ColorTheme(new ColorThemeJson(), ref errors);
            }
            
            return true;
        }
        
        errors = [];
        ColorThemes["default"] = new ColorTheme(new ColorThemeJson(), ref errors);
        
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
                Logger.Error("Invalid keybind \"\" for input action \"{0}\": keybind is empty",
                             bindName);
                return false;
            }

            var split = str.Split('+').ToList();
            if (split.Count > 4)
            {
                Logger.Error("Invalid keybind \"\" for input action \"{0}\": too many keys",
                             bindName);
                return false;
            }

            if (split.Distinct().Count() != split.Count)
            {
                Logger.Error("Invalid keybind \"\" for input action \"{0}\": duplicate keys",
                             bindName);
                return false;
            }

            if (split.Count > 1)
            {
                for (var i = 0; i < split.Count - 1; ++i)
                {
                    if (split[i] != "ctrl" && split[i] != "shift" && split[i] != "alt")
                    {
                        Logger.Error("Invalid keybind \"\" for input action \"{0}\": invalid " +
                                     "modifier", bindName);
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
                Logger.Error("Invalid keybind \"\" for input action \"{0}\": invalid primary key",
                             bindName);
                return false;
            }
        }       

        return true;
    }
}