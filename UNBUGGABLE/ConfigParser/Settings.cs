using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UNBUGGABLE.Resources;


// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class Debug
{
    [JsonRequired][JsonPropertyName("enabled")]
    public bool Enabled = false;

    [JsonRequired][JsonPropertyName("commandStacks")]
    public bool CommandStacks = true;

    [JsonRequired][JsonPropertyName("inputData")]
    public bool InputData = true;

    [JsonRequired][JsonPropertyName("mediaPlayer")]
    public bool MediaPlayer = true;

    [JsonRequired][JsonPropertyName("noteTimestamps")]
    public bool NoteTimeStamps = true;
    
    public string GetFormattedString()
    {
        return $"""
                    - command stacks: {CommandStacks}
                    - input data: {InputData}
                    - media player: {MediaPlayer}
                    - note timestamps: {NoteTimeStamps}
                """;
    }
}

public class HitSounds
{
    [JsonRequired][JsonPropertyName("single")]
    public bool Single = true;

    [JsonRequired][JsonPropertyName("spike")]
    public bool Spike = true;

    [JsonRequired][JsonPropertyName("freestyle")]
    public bool Freestyle = true;

    [JsonRequired][JsonPropertyName("holdStart")]
    public bool HoldStart = true;

    [JsonRequired][JsonPropertyName("holdEnd")]
    public bool HoldEnd = true;

    [JsonRequired][JsonPropertyName("doubleStart")]
    public bool DoubleStart = true;

    [JsonRequired][JsonPropertyName("doubleEnd")]
    public bool DoubleEnd = true;

    [JsonRequired][JsonPropertyName("mashStart")]
    public bool MashStart = true;

    [JsonRequired][JsonPropertyName("mashEnd")]
    public bool MashEnd = true;

    [JsonRequired][JsonPropertyName("copSingle")]
    public bool CopSingle = true;

    [JsonRequired][JsonPropertyName("copHoldStart")]
    public bool CopHoldStart = true;

    [JsonRequired][JsonPropertyName("copHoldEnd")]
    public bool CopHoldEnd = true;

    [JsonRequired][JsonPropertyName("copMashStart")]
    public bool CopMashStart = true;

    [JsonRequired][JsonPropertyName("copMashEnd")]
    public bool CopMashEnd = true;

    [JsonRequired][JsonPropertyName("cameraChange")]
    public bool CameraChange = true;

    [JsonRequired][JsonPropertyName("marker1")]
    public bool Marker1 = false;

    [JsonRequired][JsonPropertyName("marker2")]
    public bool Marker2 = false;

    [JsonRequired][JsonPropertyName("marker3")]
    public bool Marker3 = false;
    
    public string GetFormattedString()
    {
        return $"""

                    - single: {Single}
                    - spike: {Spike}
                    - freestyle: {Freestyle}
                    - hold start: {HoldStart}
                    - hold end: {HoldEnd}
                    - double start: {DoubleStart}
                    - double end: {DoubleEnd}
                    - mash start: {MashStart}
                    - mash end: {MashEnd}
                    - cop single: {CopSingle}
                    - cop hold start: {CopHoldStart}
                    - cop hold end: {CopHoldEnd}
                    - cop mash start: {CopMashStart}
                    - cop mash end: {CopMashEnd}
                    - camera change: {CameraChange}
                    - marker 1: {Marker1}
                    - marker 2: {Marker2}
                    - marker 3: {Marker3}
                """;
    }
}

public class Settings
{
    [JsonRequired][JsonPropertyName("colorTheme")]
    public string ColorTheme = "default";

    [JsonRequired][JsonPropertyName("useBeatFiles")]
    public bool DefaultSaveToBeatFiles = true;

    [JsonRequired][JsonPropertyName("enhancedPreview")]
    public bool EnhancedPreview = true;

    [JsonRequired] [JsonPropertyName("alwaysShowAllNoteFlags")]
    public bool AlwaysShowAllFlags = false;

    [JsonRequired] [JsonPropertyName("enableBreakpoints")]
    public bool EnableBreakpoints = true;

    [JsonRequired][JsonPropertyName("useLane2AsMarkers")]
    public bool Lane2Markers = true;

    [JsonRequired] [JsonPropertyName("saveMarkersAsLane2Notes")]
    public bool SaveMarkersInLane2 = false;

    [JsonRequired][JsonPropertyName("alwaysEnableCustomDifficultyName")]
    public bool AlwaysEnableCustomDifficultyName = false;

    [JsonRequired][JsonPropertyName("autoSelectPastedNotes")]
    public bool AutoSelectPastedNotes = true;

    [JsonRequired][JsonPropertyName("allowTopLaneCopMashes")]
    public bool AllowTopLaneCopMashes = false;

    [JsonRequired] [JsonPropertyName("showFreestyleSubNotesWhilePlacing")]
    public bool ShowSubFreestylesInNoteViewer = true;

    [JsonRequired][JsonPropertyName("enableNegativeMashConversion")]
    public bool NegativeMashConversion = true;

    [JsonRequired][JsonPropertyName("beatSnaps")]
    public List<int> BeatSnaps = [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 20, 5, 9, 11, 13];

    [JsonRequired][JsonPropertyName("pasteOverwrites")]
    public string PasteBehavior = "notes";

    [JsonRequired][JsonPropertyName("preserveNoiszFlag")]
    public bool PreserveNoiszFlag = true;

    [JsonRequired] [JsonPropertyName("minZoom")]
    public double MinZoom = 0.5;

    [JsonRequired] [JsonPropertyName("maxZoom")]
    public double MaxZoom = 7.5;

    [JsonRequired] [JsonPropertyName("zoomIncrement")]
    public double ZoomIncrement = 0.25;

    [JsonRequired][JsonPropertyName("laneOrder")]
    public List<string> LaneOrder = ["top", "center", "bottom", "camera"];

    [JsonRequired][JsonPropertyName("doublePreviewAlpha")]
    public double DoublePreviewAlpha = 0.5;

    [JsonRequired][JsonPropertyName("currentTimePosition")]
    public int CurrentTimePosition = 175;

    [JsonRequired][JsonPropertyName("hitSoundOffset")]
    public int HitSoundOffset = -30;

    [JsonRequired][JsonPropertyName("hardChartOffset")]
    public int HardChartOffset = -60;

    [JsonRequired][JsonPropertyName("hitSoundTickRate")]
    public int HitSoundTickRate = 180;

    [JsonRequired][JsonPropertyName("maxConcurrentHitSounds")]
    public int MaxConcurrentSfx = 16;

    [JsonRequired][JsonPropertyName("autosaveInterval")]
    public int AutosaveInterval = 600;

    [JsonRequired][JsonPropertyName("hitSounds")]
    public HitSounds HitSounds = new();

    [JsonRequired][JsonPropertyName("debug")]
    public Debug DebugToggles = new();
    
    public void PrintSettings()
    {
        Trace.WriteLine($"""
                         color theme: {ColorTheme}
                         save to .beat.txt: {DefaultSaveToBeatFiles}
                         enhanced preview: {EnhancedPreview}
                         always show all note flags: {AlwaysShowAllFlags}
                         enable breakpoints: {EnableBreakpoints}
                         lane 2 markers: {Lane2Markers}
                         save markers as lane 2 notes: {SaveMarkersInLane2}
                         always enable custom difficulty name: {AlwaysEnableCustomDifficultyName}
                         auto select pasted notes: {AutoSelectPastedNotes}
                         allow top lane cop mashes: {AllowTopLaneCopMashes}
                         show sub freestyles while placing: {ShowSubFreestylesInNoteViewer}
                         negative mash conversion: {NegativeMashConversion}
                         double preview alpha: {DoublePreviewAlpha}
                         autosave interval: {AutosaveInterval} seconds
                         beat snaps: [{string.Join(", ", BeatSnaps)}]
                         min zoom: {MinZoom}
                         max zoom: {MaxZoom}
                         zoom increment: {ZoomIncrement}
                         lane order: [{string.Join(", ", LaneOrder)}]
                         hit sound offset: {HitSoundOffset}
                         hard chart offset: {HardChartOffset}
                         hit sound tick rate: {HitSoundTickRate}
                         max concurrent hit sounds: {MaxConcurrentSfx}
                         hit sounds: {HitSounds.GetFormattedString()}
                         debug mode: {DebugToggles.Enabled}
                         {DebugToggles.GetFormattedString()}
                         """);
    }
}