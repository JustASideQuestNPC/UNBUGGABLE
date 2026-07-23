using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Media;
using UNBEATABLEChartEditor;
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
        { "Marker2", Color.Parse("#EADF09") },
        { "Marker3", Color.Parse("#B609EA") },
        { "BpmChange", Color.Parse("#0981EA") },
        { "Label", Color.Parse("#EADF09") },
        { "Cop1", Color.Parse("#3259E5") },
        { "Cop2", Color.Parse("#ED4964") },
        { "Cop3", Color.Parse("#44F430") },
        { "Cop4", Color.Parse("#F4E430") },
    };
    
    public static Settings Settings { get; private set; } = new()
    {
        ColorTheme = "Default",
        DefaultSaveToBeatFiles = true,
        EnhancedPreview = true,
        AlwaysShowAllFlags = false,
        EnableBreakpoints = true,
        Lane2Markers = true,
        SaveMarkersInLane2 = false,
        AlwaysEnableCustomDifficultyName = false,
        AutoSelectPastedNotes = true,
        AllowTopLaneCopMashes = false,
        ShowSubFreestylesInNoteViewer = true,
        BeatSnaps = [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 20, 5, 9, 11, 13],
        MinZoom = 0.5,
        MaxZoom = 0.75,
        ZoomIncrement = 0.25,
        LaneOrder = ["top", "center", "bottom", "camera"],
        HitSoundOffset = 0,
        HardChartOffset = -60,
        HitSoundTickRate = 150,
        CurrentTimePosition = 175,
        DebugMode = false,
        HitSounds = new()
        {
            Single = true,
            Spike = true,
            Freestyle = true,
            HoldStart = true,
            HoldEnd = true,
            DoubleStart = true,
            DoubleEnd = true,
            MashStart = true,
            MashEnd = true,
            CopSingle = true,
            CopHoldStart = true,
            CopHoldEnd = true,
            CopMashStart = true,
            CopMashEnd = true,
            CameraChange = false,
            Marker1 = false,
            Marker2 = false,
            Marker3 = false
        }
    };
    
    public static bool PracticeModInstalled { get; private set; } = false;
    public static string PracticeModConfigPath { get; private set; } = "";
    
    /// <summary>
    /// Default starting location for saving and loading files.
    /// </summary>
    public static string CustomSongsDirectory { get; private set; } = "";
    
    /// <summary>
    /// Path to the file with user settings.
    /// </summary>
    private const string ConfigFileName = "config.yaml";

    /// <summary>
    /// Path to the file with all color themes.
    /// </summary>
    private const string ColorThemeListFileName = "themes.json";
    
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
            Trace.WriteLine($"Could not parse JSON: {e.Message}");
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

    public static void LoadConfig()
    {
        var deserializer = new DeserializerBuilder()
                           .WithNamingConvention(CamelCaseNamingConvention.Instance)
                           .IgnoreUnmatchedProperties()
                           .Build();
        
        var configPath = Path.Combine(Environment.CurrentDirectory, ConfigFileName);
        if (File.Exists(configPath))
        {
            try
            {
                var settings = deserializer.Deserialize<Settings>(File.ReadAllText(configPath));
                Settings = settings;
                
                var valid = !(settings.MinZoom <= 0 || settings.MaxZoom <= 0 ||
                              settings.MinZoom > settings.MaxZoom || settings.ZoomIncrement == 0 ||
                              settings.BeatSnaps.Count == 0 ||
                              settings.BeatSnaps.Any(snap => snap <= 0));

                if (settings.LaneOrder.Count != 4 ||
                    settings.LaneOrder.Count != settings.LaneOrder.Distinct().Count())
                {
                    valid = false;
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
                    valid = false;
                }

                if (valid)
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
            catch (YamlException e)
            {
                Trace.WriteLine($"Could not parse config file: {e.Message}");
            }
        }
        else
        {
            Trace.WriteLine("Config file not found.");
        }
        
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
    }

    private static void LoadThemes(IResourceDictionary resources)
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
                            Trace.WriteLine($"Theme {themeName} is null, skipping it.");
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
                            Trace.WriteLine($"Loaded theme {themeName}");
                        }
                    }
                }
                else
                {
                    Trace.WriteLine("Could not parse color themes: file is empty.");
                }
            }
            catch (JsonException e)
            {
                Trace.WriteLine($"Could not parse color themes: {e.Message}");
            }
            Trace.WriteLine("Loaded color themes.");
        }
        else
        {
            Trace.WriteLine("Color theme file not found.");
        }
    }
}