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
    public static string LoadErrorMessage { get; private set; } = "";
    
    /// <summary>
    /// Default starting location for saving and loading files.
    /// </summary>
    public static string CustomSongsDirectory { get; private set; } = "";
    
    private static readonly string ConfigFilePath = Path.Combine(Environment.CurrentDirectory,
                                                                 "configs/config.json");

    private static readonly string KeybindFilePath = Path.Combine(Environment.CurrentDirectory,
                                                                  "configs/keybinds.json");
    private static readonly string ThemesFilePath = Path.Combine(Environment.CurrentDirectory,
                                                                 "configs/themes.json");

    /// <summary>
    /// Path to the file with all color themes.
    /// </summary>
    private const string ColorThemeListFileName = "configs/themes.json";
    
    private static readonly Dictionary<string, Dictionary<string, Color>> ColorThemes = new();
    
    // JSON types for setting and color theme objects
    private static readonly Dictionary<string, JsonValueKind> ColorThemePropertyTypes = new()
    {
        { "accent", JsonValueKind.String },
        { "windowBackgroundPrimary", JsonValueKind.String },
        { "windowBackgroundSecondary", JsonValueKind.String },
        { "editorBackground", JsonValueKind.String },
        { "textPrimary", JsonValueKind.String },
        { "textSecondary", JsonValueKind.String },
        { "textDark", JsonValueKind.String },
        { "singleNote", JsonValueKind.String },
        { "spike", JsonValueKind.String },
        { "doubleNote", JsonValueKind.String },
        { "freestyle", JsonValueKind.String },
        { "noteOutline", JsonValueKind.String },
        { "selectedNoteOverlay", JsonValueKind.String },
        { "selectDragOverlay", JsonValueKind.String },
        { "deleteDragOverlay", JsonValueKind.String },
        { "cameraChange", JsonValueKind.String },
        { "viewableArea", JsonValueKind.String },
        { "fullBeatSnapLine", JsonValueKind.String },
        { "subBeatSnapLine", JsonValueKind.String },
        { "currentTimeLine", JsonValueKind.String },
        { "breakpoint", JsonValueKind.String },
        { "marker1", JsonValueKind.String },
        { "marker2", JsonValueKind.String },
        { "marker3", JsonValueKind.String },
        { "bpmChange", JsonValueKind.String },
        { "label", JsonValueKind.String },
        { "cop1", JsonValueKind.String },
        { "cop2", JsonValueKind.String },
        { "cop3", JsonValueKind.String },
        { "cop4", JsonValueKind.String }
    };
    
    /// <summary>
    /// Loads and parses user settings and color themes.
    /// <param name="resources">The resource dictionary to add theme brushes to.</param>
    /// </summary>
    public static void LoadAllConfigFiles()
    {
        // updatedConfig and updatedKeybinds are temporary files used for preserving existing
        // settings after an update
        var updatedConfigPath = Path.Combine(Environment.CurrentDirectory,
                                               "configs/updatedConfig.json");
        if (File.Exists(updatedConfigPath))
        {
            Trace.WriteLine("updating configs");
            var fullCopy = (
                !File.Exists(ConfigFilePath) ||
                !JsonHelper.TryMergeFiles(ConfigFilePath, updatedConfigPath, ConfigFilePath));
            if (fullCopy)
            {
                Trace.WriteLine(
                    "Keybinds.json does not exist (or was invalid), fully copying updated file");
                File.Move(updatedConfigPath, ConfigFilePath);
            }
        }
        var updatedKeybindsPath = Path.Combine(Environment.CurrentDirectory,
                                               "configs/updatedKeybinds.json");
        if (File.Exists(updatedKeybindsPath))
        {
            Trace.WriteLine("updating configs");
            var fullCopy = (
                !File.Exists(KeybindFilePath) ||
                !JsonHelper.TryMergeFiles(KeybindFilePath, updatedKeybindsPath, KeybindFilePath));
            if (fullCopy)
            {
                Trace.WriteLine(
                    "Keybinds.json does not exist (or was invalid), fully copying updated file");
                File.Move(updatedKeybindsPath, KeybindFilePath);
            }
        }
        
        LoadError = !TryLoadThemes(out var errorMessage);
        if (!LoadError)
        {
            LoadError = !TryLoadKeybinds(out errorMessage);
        }
        if (!LoadError)
        {
            LoadError = !TryLoadConfig(out errorMessage);
        }
        LoadErrorMessage = errorMessage;
    }

    public static void TryReloadConfig()
    {
        LoadError = !TryLoadThemes(out var errorMessage);
        if (!LoadError)
        {
            LoadError = !TryLoadKeybinds(out errorMessage);
        }
        if (!LoadError)
        {
            LoadError = !TryLoadConfig(out errorMessage);
        }
        LoadErrorMessage = errorMessage;
    }

    private static bool TryLoadKeybinds(out string errorMessage)
    {
        try
        {
            var keybinds = JsonSerializer.Deserialize<Keybinds>(File.ReadAllText(KeybindFilePath));
            if (keybinds != null && VerifyKeybindStrings(keybinds.Undo) &&
                                    VerifyKeybindStrings(keybinds.Redo) &&
                                    VerifyKeybindStrings(keybinds.SaveFile) &&
                                    VerifyKeybindStrings(keybinds.OpenFile) &&
                                    VerifyKeybindStrings(keybinds.ResetPlaySpeed) &&
                                    VerifyKeybindStrings(keybinds.ReloadConfig) &&
                                    VerifyKeybindStrings(keybinds.MoveForward) &&
                                    VerifyKeybindStrings(keybinds.MoveBack) &&
                                    VerifyKeybindStrings(keybinds.QuickScrollModifier) &&
                                    VerifyKeybindStrings(keybinds.PlayPause) &&
                                    VerifyKeybindStrings(keybinds.ZoomIn) &&
                                    VerifyKeybindStrings(keybinds.ZoomOut) &&
                                    VerifyKeybindStrings(keybinds.PrevLabel) &&
                                    VerifyKeybindStrings(keybinds.NextLabel) &&
                                    VerifyKeybindStrings(keybinds.PrevNoteSnap) &&
                                    VerifyKeybindStrings(keybinds.NextNoteSnap) &&
                                    VerifyKeybindStrings(keybinds.PlaceTopLane) &&
                                    VerifyKeybindStrings(keybinds.PlaceBottomLane) &&
                                    VerifyKeybindStrings(keybinds.PlaceCameraLane) &&
                                    VerifyKeybindStrings(keybinds.PlaceCenterLane) &&
                                    VerifyKeybindStrings(keybinds.SelectAll) &&
                                    VerifyKeybindStrings(keybinds.SelectTopLane) &&
                                    VerifyKeybindStrings(keybinds.SelectBottomLane) &&
                                    VerifyKeybindStrings(keybinds.SelectCameraLane) &&
                                    VerifyKeybindStrings(keybinds.SelectCenterLane) &&
                                    VerifyKeybindStrings(keybinds.Cut) &&
                                    VerifyKeybindStrings(keybinds.Copy) &&
                                    VerifyKeybindStrings(keybinds.Paste) &&
                                    VerifyKeybindStrings(keybinds.ClearSelection) &&
                                    VerifyKeybindStrings(keybinds.DeleteSelection) &&
                                    VerifyKeybindStrings(keybinds.MirrorSelection) &&
                                    VerifyKeybindStrings(keybinds.MoveSelectionForward) &&
                                    VerifyKeybindStrings(keybinds.MoveSelectionBack) &&
                                    VerifyKeybindStrings(keybinds.SetFinishFlag) &&
                                    VerifyKeybindStrings(keybinds.LockFinishFlag) &&
                                    VerifyKeybindStrings(keybinds.SetWhistleFlag) &&
                                    VerifyKeybindStrings(keybinds.LockWhistleFlag) &&
                                    VerifyKeybindStrings(keybinds.SetClapFlag) &&
                                    VerifyKeybindStrings(keybinds.LockClapFlag) &&
                                    VerifyKeybindStrings(keybinds.SetNoiszFlag) &&
                                    VerifyKeybindStrings(keybinds.LockNoiszFlag) &&
                                    VerifyKeybindStrings(keybinds.CopId0) &&
                                    VerifyKeybindStrings(keybinds.CopId1) &&
                                    VerifyKeybindStrings(keybinds.CopId2) &&
                                    VerifyKeybindStrings(keybinds.CopId3) &&
                                    VerifyKeybindStrings(keybinds.CopId4) &&
                                    VerifyKeybindStrings(keybinds.PrevCop) &&
                                    VerifyKeybindStrings(keybinds.NextCop) &&
                                    VerifyKeybindStrings(keybinds.AddBpmChange) &&
                                    VerifyKeybindStrings(keybinds.RemoveBpmChange) &&
                                    VerifyKeybindStrings(keybinds.AddLabel) &&
                                    VerifyKeybindStrings(keybinds.RemoveLabel) &&
                                    VerifyKeybindStrings(keybinds.AddMarker1) &&
                                    VerifyKeybindStrings(keybinds.AddMarker2) &&
                                    VerifyKeybindStrings(keybinds.AddMarker3) &&
                                    VerifyKeybindStrings(keybinds.SetBreakpoint) &&
                                    VerifyKeybindStrings(keybinds.RemoveBreakpoint) &&
                                    VerifyKeybindStrings(keybinds.EmergencyReload) &&
                                    VerifyKeybindStrings(keybinds.NudgeForward) &&
                                    VerifyKeybindStrings(keybinds.NudgeBack) &&
                                    VerifyKeybindStrings(keybinds.NudgeTailForward) &&
                                    VerifyKeybindStrings(keybinds.NudgeTailBack))
            {
                Keybinds = keybinds;
            }
            else
            {
                errorMessage = "Could not parse keybinds: some keybind strings are invalid";
                Trace.WriteLine(errorMessage);
                return false;
            }
        }
        catch (JsonException e)
        {
            errorMessage = $"Could not parse keybinds: {e.Message}";
            Trace.WriteLine(errorMessage);
            return false;
        }
        
        InputManager.Actions = [
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
            new EmergencyReloadAction(Keybinds.EmergencyReload)
        ];
        
        Trace.WriteLine("Loaded keybinds");
        errorMessage = "";
        return true;
    }
    
    private static bool TryLoadConfig(out string errorMessage)
    {
        errorMessage = "";
        var loadSuccessful = true;
        try
        {
            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(ConfigFilePath));
            if (settings != null)
            {
                loadSuccessful = !(settings.MinZoom <= 0 || settings.MaxZoom <= 0 || 
                                   settings.MinZoom > settings.MaxZoom ||
                                   settings.ZoomIncrement == 0 || settings.BeatSnaps.Count == 0 ||
                                   settings.BeatSnaps.Any(snap => snap <= 0));
                
                settings.BeatSnaps = settings.BeatSnaps.Distinct().ToList();
                // TONS of things in the chart code depend on the first beat snap being one beat
                if (settings.BeatSnaps[0] != 1)
                {
                    settings.BeatSnaps.Remove(1);
                    settings.BeatSnaps.Insert(0, 1);
                }

                if (settings.LaneOrder.Count != 4 ||
                    settings.LaneOrder.Count != settings.LaneOrder.Distinct().Count())
                {
                    errorMessage = "Invalid lane order";
                    loadSuccessful = false;
                }
                
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

                if (!hasTop || !hasBottom || !hasCamera || !hasCenter)
                {
                    errorMessage = "Invalid lane order";
                    loadSuccessful = false;
                }

                if (settings.PasteBehavior != "none" && settings.PasteBehavior != "notes" &&
                    settings.PasteBehavior != "region")
                {
                    errorMessage = "Invalid paste overwrite setting: must be \"none\", " +
                                   "\"notes\", or \"region\".";
                    loadSuccessful = false;
                }

                if (settings.AutoSelectBehavior != "none" &&
                    settings.AutoSelectBehavior != "pasted" && settings.AutoSelectBehavior != "all")
                {
                    errorMessage = "Invalid auto select setting: must be \"none\", " +
                                   "\"pasted\", or \"all\".";
                    loadSuccessful = false;
                }

                if (settings.QuickScrollBeats <= 0)
                {
                    errorMessage = "Quick scroll beats must be > 0";
                    loadSuccessful = false;
                }

                if (loadSuccessful)
                {
                    Settings = settings;
                    Trace.WriteLine("Loaded settings:");
                    Settings.PrintSettings();
                }
                else
                {
                    Trace.WriteLine("Invalid settings, using default values.");
                }
            }
            else
            {
                errorMessage = "Config file is empty";
                loadSuccessful = false;
                Trace.WriteLine("Could not parse config: file is empty.");
            }
        }
        catch (JsonException e)
        {
            errorMessage = $"Could not parse config file: {e.Message}";
            loadSuccessful = false;
            Trace.WriteLine(errorMessage);
        }

        CurrentTheme = ColorThemes.TryGetValue(Settings.ColorTheme, out var theme) ?
            theme : ColorThemes["Default"];
        
        Trace.WriteLine("Loaded config");
        Settings.PrintSettings();
        
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
        
        return loadSuccessful;
    }

    private static bool TryLoadThemes(out string errorMessage)
    {
        errorMessage = "";
        ColorThemes.Clear();
        
        var themeFilePath = Path.Combine(Environment.CurrentDirectory, ColorThemeListFileName);
        if (File.Exists(themeFilePath))
        {
            try
            {
                var colorThemeListJsonNode = JsonSerializer.Deserialize<JsonNode>(
                    File.ReadAllText(themeFilePath));
                if (colorThemeListJsonNode != null)
                {
                    var colorThemeListJson = colorThemeListJsonNode.AsObject();
                    foreach (var (themeName, themeValue) in colorThemeListJson)
                    {
                        if (themeValue is null)
                        {
                            Trace.WriteLine($"Theme {themeName} is null, skipping it.");
                            continue;
                        }

                        var themeJson = themeValue.AsObject();
                        if (JsonHelper.VerifyJsonObject(themeJson, ColorThemePropertyTypes))
                        {
                            var theme = new Dictionary<string, Color>();
                            foreach (var (colorName, colorString) in themeJson)
                            {
                                var brushColor = Color.Parse(colorString!.GetValue<string>());
                                var brushName = colorName[0].ToString().ToUpper() + colorName[1..];
                                theme[$"{brushName}"] = brushColor;
                            }
                            ColorThemes.Add(themeName, theme);
                            Trace.WriteLine($"Loaded theme \"{themeName}\"");
                        }
                    }
                }
                else
                {
                    errorMessage = "Could not parse color themes: file is empty.";
                    Trace.WriteLine(errorMessage);
                }
            }
            catch (JsonException e)
            {
                errorMessage = $"Could not parse color themes: {e.Message}";
                Trace.WriteLine(errorMessage);
            }
            Trace.WriteLine("Loaded color themes.");
        }
        else
        {
            errorMessage = "Color theme file not found.";
            Trace.WriteLine(errorMessage);
        }

        return errorMessage == "";
    }

    private static bool VerifyKeybindStrings(List<string>? keybindStrings)
    {
        if (keybindStrings == null)
        {
            return false;
        }
        
        foreach (var str in keybindStrings)
        {
            if (str == "")
            {
                Trace.WriteLine("Invalid keybind \"\": keybind is empty");
                return false;
            }

            var split = str.Split('+').ToList();
            if (split.Count > 4)
            {
                Trace.WriteLine($"Invalid keybind \"{str}\": too many keys");
                return false;
            }

            if (split.Distinct().Count() != split.Count)
            {
                Trace.WriteLine($"Invalid keybind \"{str}\": duplicate keys");
                return false;
            }

            if (split.Count > 1)
            {
                for (var i = 0; i < split.Count - 1; ++i)
                {
                    if (split[i] != "ctrl" && split[i] != "shift" && split[i] != "alt")
                    {
                        Trace.WriteLine($"Invalid keybind \"{str}\": invalid modifier");
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
                Trace.WriteLine($"Invalid keybind \"{str}\": invalid primary key");
                return false;
            }
        }       

        return true;
    }
}