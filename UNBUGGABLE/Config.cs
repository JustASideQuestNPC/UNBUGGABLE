using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication.ExtendedProtection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Media;

namespace UNBUGGABLE.Resources;

/// <summary>
/// Loads color themes and user settings when the app starts.
/// </summary>
public static class Config
{
    public static readonly Dictionary<string, Dictionary<string, Color>> ColorThemes = new();

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
        { "Marker2", Color.Parse("#EADF09") },
        { "Marker3", Color.Parse("#B609EA") },
        { "BpmChange", Color.Parse("#0981EA") },
        { "Label", Color.Parse("#EADF09") },
        { "Cop1", Color.Parse("#3259E5") },
        { "Cop2", Color.Parse("#ED4964") },
        { "Cop3", Color.Parse("#44F430") },
        { "Cop4", Color.Parse("#F4E430") },
    };

    /// <summary>
    /// Holds whether hit sounds are enabled for each type of note.
    /// </summary>
    public static class HitSounds
    {
        private static readonly List<string> JsonKeyNames = [
            "single", "spike", "freestyle", "holdStart", "holdEnd", "doubleStart", "doubleEnd",
            "mashStart", "mashEnd", "copSingle", "copHoldStart", "copHoldEnd", "copMashStart",
            "copMashEnd", "cameraChange", "marker"
        ];
        
        public static bool Single { get; private set; } = true;
        public static bool Spike { get; private set; } = true;
        public static bool Freestyle { get; private set; } = true;
        public static bool HoldStart { get; private set; } = true;
        public static bool HoldEnd { get; private set; } = true;
        public static bool DoubleStart { get; private set; } = true;
        public static bool DoubleEnd { get; private set; } = true;
        public static bool MashStart { get; private set; } = true;
        public static bool MashEnd { get; private set; } = true;
        public static bool CopSingle { get; private set; } = true;
        public static bool CopHoldStart { get; private set; } = true;
        public static bool CopHoldEnd { get; private set; } = true;
        public static bool CopMashStart { get; private set; } = true;
        public static bool CopMashEnd { get; private set; } = true;
        public static bool CameraChange { get; private set; } = false;
        public static bool Marker1 { get; private set; } = false;
        public static bool Marker2 { get; private set; } = false;
        public static bool Marker3 { get; private set; } = false;

        public static void TryParseJson(JsonObject json)
        {
            var valid = true;
            foreach (var keyName in JsonKeyNames)
            {
                if (json[keyName]?.AsValue().GetValueKind() is not JsonValueKind.True and not
                    JsonValueKind.False)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                Single = json["single"]?.AsValue().GetValue<bool>() ?? true;
                Spike = json["spike"]?.AsValue().GetValue<bool>() ?? false;
                Freestyle = json["freestyle"]?.AsValue().GetValue<bool>() ?? false;
                HoldStart = json["holdStart"]?.AsValue().GetValue<bool>() ?? true;
                HoldEnd = json["holdEnd"]?.AsValue().GetValue<bool>() ?? true;
                DoubleStart = json["doubleStart"]?.AsValue().GetValue<bool>() ?? true;
                DoubleEnd = json["doubleEnd"]?.AsValue().GetValue<bool>() ?? true;
                MashStart = json["mashStart"]?.AsValue().GetValue<bool>() ?? true;
                MashEnd = json["mashEnd"]?.AsValue().GetValue<bool>() ?? true;
                CopSingle = json["copSingle"]?.AsValue().GetValue<bool>() ?? true;
                CopHoldStart = json["copHoldStart"]?.AsValue().GetValue<bool>() ?? true;
                CopHoldEnd = json["copHoldEnd"]?.AsValue().GetValue<bool>() ?? true;
                CopMashStart = json["copMashStart"]?.AsValue().GetValue<bool>() ?? true;
                CopMashEnd = json["copMashEnd"]?.AsValue().GetValue<bool>() ?? true;
                CameraChange = json["cameraChange"]?.AsValue().GetValue<bool>() ?? false;
                Marker1 = json["marker1"]?.AsValue().GetValue<bool>() ?? false;
                Marker2 = json["marker2"]?.AsValue().GetValue<bool>() ?? false;
                Marker3 = json["marker3"]?.AsValue().GetValue<bool>() ?? false;
            }
        }
    }

    /// <summary>
    /// If true, the in-game preview shows where doubles will land and gives mash notes a tail to
    /// indicate how long they last.
    /// </summary>
    public static bool EnhancedPreview { get; private set; } = true;

    /// <summary>
    /// If true, Ctrl+s and the save button will save to an UNBUGGABLE .beat.txt file.
    /// </summary>
    public static bool DefaultSaveToBeatFiles { get; private set; } = true;
    
    /// <summary>
    /// If true, the note viewer will always show the letters for all note flags on all notes.
    /// Normally, flags that determine the note type are not shown (for example, spikes and doubles
    /// always have the Whistle flag, so that flag is not shown for them).
    /// </summary>
    public static bool AlwaysShowAllFlags { get; private set; } = false;
    
    /// <summary>
    /// Whether to integrate with Stefy's Practice Mod and enable an option to make the chart start
    /// at a breakpoint midway through the song.
    /// </summary>
    public static bool EnableBreakpoints { get; private set; } = true;
    
    /// <summary>
    /// If true, freestyles that are sub notes will appear smaller in the note viewer.
    /// </summary>
    public static bool ShowSubFreestylesInNoteViewer { get; private set; } = true;
    
    public static bool PracticeModInstalled { get; private set; } = false;
    
    public static string PracticeModConfigPath { get; private set; } = "";
    
    /// <summary>
    /// Constant offset applied to all charts, to match up with the hard-coded offset in the
    /// official editor. DO NOT TOUCH THIS NUMBER UNLESS YOU KNOW WHAT YOU'RE DOING.
    /// </summary>
    public static int HardChartOffset { get; private set; } = 60;
    
    /// <summary>
    /// Offset applied to hit sounds. Positive values play hit sounds later.
    /// </summary>
    public static int HitSoundOffset { get; private set; } = -30;
    
    /// <summary>
    /// All possible beat snap values. Beat snap can be changed with the left and right arrow keys.
    /// For some reason, the official editor multiplies all snap values by 4. The default snap
    /// values for UNBUGGABLE match the ones in the official editor.
    /// </summary>
    public static List<int> BeatSnaps { get; private set; } = [
        1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 20, 5, 9, 11, 13];

    /// <summary>
    /// If true, loading a chart file will convert all notes that are in lane 2 into UNBUGGABLE
    /// markers (unless a marker there already exists).
    /// </summary>
    public static bool Lane2Markers { get; private set; } = true;

    /// <summary>
    /// If true, all markers are saved in a chart file as a lane 2 note. Markers are always saved in
    /// the UNBUGGABLE section of the chart file, regardless of this setting.
    /// </summary>
    public static bool SaveMarkersInLane2 { get; private set; } = false;
    
    /// <summary>
    /// If true, a custom name can be specified for any difficulty slot (not just Star difficulty).
    /// </summary>
    public static bool AlwaysEnableCustomDifficultyName { get; private set; } = false;

    /// <summary>
    /// Whether to allow placing cop mashes on the top lane. In-game, cop mashes are always on the
    /// bottom lane, regardless of what lane they were placed in.
    /// </summary>
    public static bool AllowTopLaneCopMashes { get; private set; } = false;

    /// <summary>
    /// Which order the note editor displays lanes in, from left to right:
    /// 0 = Top lane
    /// 1 = Center lane
    /// 2 = Bottom lane
    /// 3 = Camera lane
    /// </summary>
    public static List<NoteLane> LaneOrder { get; private set; } = [
        NoteLane.TOP,
        NoteLane.CENTER,
        NoteLane.BOTTOM,
        NoteLane.CAMERA
    ];

    /// <summary>
    /// If true, pasting notes somewhere will automatically select those notes.
    /// </summary>
    public static bool AutoSelectPastedNotes { get; private set; } = true;
    
    public static double MinZoom { get; private set; } = 0.5;
    public static double MaxZoom { get; private set; } = 7.5;
    public static double ZoomIncrement { get; private set; } = 0.25;
    
    /// <summary>
    /// Default starting location for saving and loading files.
    /// </summary>
    public static string CustomSongsDirectory { get; private set; } = "";
    
    /// <summary>
    /// Path to the file with user settings.
    /// </summary>
    private const string ConfigFileName = "config.json";

    /// <summary>
    /// Path to the file with all color themes.
    /// </summary>
    private const string ColorThemeListFileName = "themes.json";
    
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
    /// Verifies that a JSON object has the correct keys and types.
    /// </summary>
    /// <param name="json">The JSON value to check.</param>
    /// <param name="types">A dictionary with the correct keys and types. Allowed types are</param>
    /// <param name="allowMissing">Whether the object can be missing one or more of the required
    ///                            keys. Useful for JSON objects that have default values.</param>
    private static bool VerifyJsonObject(JsonObject json, Dictionary<string, JsonValueKind> types, 
        bool allowMissing = false)
    {
        // why is there no JSON.tryParse?
        try
        {
            foreach (var (themeName, _) in json)
            {
                if (!types.ContainsKey(themeName))
                {
                    return false;
                }
            }

            foreach (var (themeName, type) in types)
            {
                if (!json.ContainsKey(themeName) && !allowMissing)
                {
                    return false;
                }
                
                if (json[themeName]!.GetValueKind() != type)
                {
                    return false;
                }
            }
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Could not parse JSON: {e.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Loads and parses user settings and color themes.
    /// <param name="resources">The resource dictionary to add theme brushes to.</param>
    /// </summary>
    public static void LoadFiles(IResourceDictionary resources)
    {
        LoadThemes(resources);
        LoadConfig();
    }

    public static void LoadThemes(IResourceDictionary resources)
    {
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
                            Console.WriteLine($"Theme {themeName} is null, skipping it.");
                            continue;
                        }

                        var themeJson = themeValue.AsObject();
                        if (VerifyJsonObject(themeJson, ColorThemePropertyTypes))
                        {
                            var theme = new Dictionary<string, Color>();
                            foreach (var (colorName, colorString) in themeJson)
                            {
                                var brushColor = Color.Parse(colorString!.GetValue<string>());
                                var brushName = colorName[0].ToString().ToUpper() + colorName[1..];
                                theme[$"{brushName}"] = brushColor;
                            }
                            ColorThemes.Add(themeName, theme);
                            Console.WriteLine($"Loaded theme {themeName}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Could not parse color themes: file is empty.");
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Could not parse color themes: {e.Message}");
            }
            Console.WriteLine("Loaded color themes.");
        }
        else
        {
            Console.WriteLine("Color theme file not found.");
        }
    }

    public static void LoadConfig()
    {
         var configPath = Path.Combine(Environment.CurrentDirectory, ConfigFileName);
        if (File.Exists(configPath))
        {
            try
            {
                var configJsonNode = JsonSerializer.Deserialize<JsonNode>(
                    File.ReadAllText(configPath));
                if (configJsonNode != null)
                {
                    Console.WriteLine($"Config file found at {configPath}");
                    var settingsJson = configJsonNode.AsObject();
                    // I have to check value kinds manually because for some ungodly reason,
                    // there is no `JsonValueKind.Bool`. There is `JsonValueKind.True` and
                    // `JsonValueKind.False`, but no `JsonValueKind.Bool`.
                    if (settingsJson["colorTheme"]?.AsValue()
                                                    .GetValueKind() == JsonValueKind.String)
                    {
                        var setting = settingsJson["colorTheme"]!.GetValue<string>();
                        if (ColorThemes.TryGetValue(setting, out var theme))
                        {
                            CurrentTheme = theme;
                            Console.WriteLine($"color theme: {setting}");
                        }
                    }
                    
                    if (settingsJson["enhancedPreview"]?.AsValue().GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting = settingsJson["enhancedPreview"]!.GetValue<bool>();
                        EnhancedPreview = setting;
                        Console.WriteLine($"enhanced preview: {setting}");
                    }
                    
                    if (settingsJson["useBeatFiles"]?.AsValue().GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting = settingsJson["useBeatFiles"]!.GetValue<bool>();
                        DefaultSaveToBeatFiles = setting;
                        Console.WriteLine($"default save to .beat.txt files: {setting}");
                    }
                    
                    if (settingsJson["alwaysShowAllNoteFlags"]?.AsValue().GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting = settingsJson["alwaysShowAllNoteFlags"]!.GetValue<bool>();
                        AlwaysShowAllFlags = setting;
                        Console.WriteLine($"show note flags: {setting}");
                    }
                    
                    if (settingsJson["enableBreakpoints"]?.AsValue().GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting = settingsJson["enableBreakpoints"]!.GetValue<bool>();
                        EnableBreakpoints = setting;
                        Console.WriteLine($"enable breakpoints: {setting}");
                    }
                    
                    if (settingsJson["hardChartOffset"]?.AsValue().GetValueKind()
                        is JsonValueKind.Number)
                    {
                        var setting = settingsJson["hardChartOffset"]!.GetValue<int>();
                        HardChartOffset = setting;
                        Console.WriteLine($"hard chart offset: {setting}");
                    }
                    
                    if (settingsJson["hitSoundOffset"]?.AsValue().GetValueKind()
                        is JsonValueKind.Number)
                    {
                        var setting = settingsJson["hitSoundOffset"]!.GetValue<int>();
                        HitSoundOffset = setting;
                        Console.WriteLine($"hit sound offset: {setting}");
                    }
                    
                    if (settingsJson["minZoom"]?.AsValue().GetValueKind()
                        is JsonValueKind.Number)
                    {
                        var setting = settingsJson["minZoom"]!.GetValue<double>();
                        MinZoom = setting;
                        Console.WriteLine($"min zoom: {setting}");
                    }
                    
                    if (settingsJson["maxZoom"]?.AsValue().GetValueKind()
                        is JsonValueKind.Number)
                    {
                        var setting = settingsJson["maxZoom"]!.GetValue<double>();
                        MaxZoom = setting;
                        Console.WriteLine($"max zoom: {setting}");
                    }
                    
                    if (settingsJson["zoomIncrement"]?.AsValue().GetValueKind()
                        is JsonValueKind.Number)
                    {
                        var setting = settingsJson["zoomIncrement"]!.GetValue<double>();
                        ZoomIncrement = setting;
                        Console.WriteLine($"zoom increment: {setting}");
                    }
                    
                    if (settingsJson["beatSnaps"]?.AsArray() is { Count: > 0 } snaps)
                    {
                        var isValid = true;
                        var buffer = new List<int>();
                        foreach (var snap in snaps)
                        {
                            if (snap?.GetValueKind() != JsonValueKind.Number ||
                                snap.GetValue<double>() % 1 != 0 || snap.GetValue<int>() < 1)
                            {
                                isValid = false;
                                break;
                            }
                            buffer.Add(snap.GetValue<int>());
                        }
                        if (isValid)
                        {
                            Console.WriteLine($"beat snaps: {string.Join(", ", buffer)}");
                            BeatSnaps = buffer;
                        }
                    }

                    if (settingsJson["useLane2AsMarkers"]?.AsValue().GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting = settingsJson["useLane2AsMarkers"]!.GetValue<bool>();
                        Lane2Markers = setting;
                        Console.WriteLine($"use lane 2 as markers: {setting}");
                    }
                    
                    if (settingsJson["saveMarkersAsLane2Notes"]?.AsValue().GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting = settingsJson["saveMarkersAsLane2Notes"]!.GetValue<bool>();
                        SaveMarkersInLane2 = setting;
                        Console.WriteLine($"save markers in lane 2: {setting}");
                    }
                    
                    if (settingsJson["alwaysEnableCustomDifficultyName"]?.AsValue()
                                                                         .GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting =
                            settingsJson["alwaysEnableCustomDifficultyName"]!.GetValue<bool>();
                        AlwaysEnableCustomDifficultyName = setting;
                        Console.WriteLine($"always enable custom difficulty name: {setting}");
                    }
                    
                    if (settingsJson["allowTopLaneCopMashes"]?.AsValue().GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting = settingsJson["allowTopLaneCopMashes"]!.GetValue<bool>();
                        AllowTopLaneCopMashes = setting;
                        Console.WriteLine($"allow top lane cop mashes: {setting}");
                    }
                    
                    if (settingsJson["autoSelectPastedNotes"]?.AsValue().GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting = settingsJson["autoSelectPastedNotes"]!.GetValue<bool>();
                        AutoSelectPastedNotes = setting;
                        Console.WriteLine($"auto select pasted notes: {setting}");
                    }

                    if (settingsJson["showFreestyleSubNotesWhilePlacing"]?.AsValue().GetValueKind()
                        is JsonValueKind.True or JsonValueKind.False)
                    {
                        var setting = settingsJson["showFreestyleSubNotesWhilePlacing"]!
                            .GetValue<bool>();
                        ShowSubFreestylesInNoteViewer = setting;
                        Console.WriteLine($"show freestyle subnotes while placing: {setting}");
                    }
                    
                    if (settingsJson["laneOrder"]?.AsArray() is { Count: 4 } laneOrder)
                    {
                        List<int> order = [-1, -1, -1, -1];
                        var isValid = true;
                        for (var i = 0; i < laneOrder.Count; ++i)
                        {
                            var lane = laneOrder[i];
                            if (lane?.GetValueKind() != JsonValueKind.String)
                            {
                                isValid = false;
                                break;
                            }
                            
                            var laneName = lane.GetValue<string>();
                            if (laneName != "top" && laneName != "center" &&
                                laneName != "bottom" && laneName != "camera")
                            {
                                isValid = false;
                                break;
                            }
                            
                            var laneNumber = laneName switch
                            {
                                "top" => 0,
                                "center" => 2,
                                "bottom" => 1,
                                _ => 3
                            };
                            
                            // check for duplicates
                            if (order.Contains(laneNumber))
                            {
                                isValid = false;
                                break;
                            }
                            
                            order[i] = laneNumber;
                        }

                        if (isValid)
                        {
                            LaneOrder = order.ConvertAll(x => (NoteLane)x);
                            Console.WriteLine($"Lane order: {string.Join(", ", LaneOrder)}");
                        }
                    }
                    
                    if (settingsJson["hitSounds"]?.AsObject() is { Count: > 0 } hitSounds)
                    {
                        HitSounds.TryParseJson(hitSounds);
                    }
                }
                else
                {
                    Console.WriteLine("Could not parse config file: file is empty.");
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Could not parse color theme file: {e.Message}");
            }
            Console.WriteLine("Loaded settings.");
        }
        else
        {
            Console.WriteLine("Config file not found.");
        }
        
        // look for your custom songs directory
        var gameDataDirectory = Path.GetFullPath(
            "../LocalLow/D-CELL GAMES/UNBEATABLE",
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        CustomSongsDirectory = (Directory.Exists(Path.Combine(gameDataDirectory, "CustomSongs"))
            ? Path.Combine(gameDataDirectory, "CustomSongs")
            : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        
        // breakpoints also require stefy's practice mod to be installed
        if (EnableBreakpoints)
        {
            PracticeModConfigPath = Path.Combine(gameDataDirectory, "practice-mode-settings.txt");
            if (File.Exists(PracticeModConfigPath))
            {
                Console.WriteLine("Found Practice Mod, enabling breakpoints.");
                PracticeModInstalled = true;
            }
            else
            {
                Console.WriteLine("Install Practice Mod to enable breakpoints.");
                PracticeModInstalled = false;
            }
        }
    }
}