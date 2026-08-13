using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UNBUGGABLE.Resources;


// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class Debug
{
    [JsonRequired][JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonRequired][JsonPropertyName("commandStacks")]
    public bool CommandStacks { get; set; } = true;

    [JsonRequired][JsonPropertyName("inputData")]
    public bool InputData { get; set; } = true;

    [JsonRequired][JsonPropertyName("mediaPlayer")]
    public bool MediaPlayer { get; set; } = true;

    [JsonRequired][JsonPropertyName("noteTimestamps")]
    public bool NoteTimeStamps { get; set; } = true;
    
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
    public bool Single { get; set; } = true;

    [JsonRequired][JsonPropertyName("spike")]
    public bool Spike { get; set; } = true;

    [JsonRequired][JsonPropertyName("freestyle")]
    public bool Freestyle { get; set; } = true;

    [JsonRequired][JsonPropertyName("holdStart")]
    public bool HoldStart { get; set; } = true;

    [JsonRequired][JsonPropertyName("holdEnd")]
    public bool HoldEnd { get; set; } = true;

    [JsonRequired][JsonPropertyName("doubleStart")]
    public bool DoubleStart { get; set; } = true;

    [JsonRequired][JsonPropertyName("doubleEnd")]
    public bool DoubleEnd { get; set; } = true;

    [JsonRequired][JsonPropertyName("mashStart")]
    public bool MashStart { get; set; } = true;

    [JsonRequired][JsonPropertyName("mashEnd")]
    public bool MashEnd { get; set; } = true;

    [JsonRequired][JsonPropertyName("copSingle")]
    public bool CopSingle { get; set; } = true;

    [JsonRequired][JsonPropertyName("copHoldStart")]
    public bool CopHoldStart { get; set; } = true;

    [JsonRequired][JsonPropertyName("copHoldEnd")]
    public bool CopHoldEnd { get; set; } = true;

    [JsonRequired][JsonPropertyName("copMashStart")]
    public bool CopMashStart { get; set; } = true;

    [JsonRequired][JsonPropertyName("copMashEnd")]
    public bool CopMashEnd { get; set; } = true;

    [JsonRequired][JsonPropertyName("cameraChange")]
    public bool CameraChange { get; set; } = true;

    [JsonRequired][JsonPropertyName("marker1")]
    public bool Marker1 { get; set; } = false;

    [JsonRequired][JsonPropertyName("marker2")]
    public bool Marker2 { get; set; } = false;

    [JsonRequired][JsonPropertyName("marker3")]
    public bool Marker3 { get; set; } = false;
    
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
    public string ColorTheme { get; set; } = "default";

    [JsonRequired][JsonPropertyName("useBeatFiles")]
    public bool DefaultSaveToBeatFiles { get; set; } = true;

    [JsonRequired][JsonPropertyName("enhancedPreview")]
    public bool EnhancedPreview { get; set; } = true;

    [JsonRequired] [JsonPropertyName("alwaysShowAllNoteFlags")]
    public bool AlwaysShowAllFlags { get; set; } = false;

    [JsonRequired] [JsonPropertyName("enableBreakpoints")]
    public bool EnableBreakpoints { get; set; } = true;

    [JsonRequired][JsonPropertyName("useLane2AsMarkers")]
    public bool Lane2Markers { get; set; } = true;

    [JsonRequired] [JsonPropertyName("saveMarkersAsLane2Notes")]
    public bool SaveMarkersInLane2 { get; set; } = false;

    [JsonRequired][JsonPropertyName("alwaysEnableCustomDifficultyName")]
    public bool AlwaysEnableCustomDifficultyName { get; set; } = false;

    [JsonRequired][JsonPropertyName("autoSelect")]
    public string AutoSelectBehavior { get; set; } = "pasted";
    
    [JsonRequired][JsonPropertyName("selectHoldNotesFromTail")]
    public string HoldTailSelect { get; set; } = "all";

    [JsonRequired][JsonPropertyName("allowTopLaneCopMashes")]
    public bool AllowTopLaneCopMashes { get; set; } = false;

    [JsonRequired] [JsonPropertyName("showFreestyleSubNotesWhilePlacing")]
    public bool ShowSubFreestylesInNoteViewer { get; set; } = true;

    [JsonRequired][JsonPropertyName("enableNegativeMashConversion")]
    public bool NegativeMashConversion { get; set; } = true;
    
    [JsonRequired][JsonPropertyName("quickScrollBeats")]
    public int QuickScrollBeats { get; set; } = 5;
    
    [JsonRequired][JsonPropertyName("sliderIncrement")]
    public int SliderIncrement { get; set; } = 5;

    [JsonRequired][JsonPropertyName("beatSnaps")]
    public List<int> BeatSnaps { get; set; } =
        [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 20, 5, 9, 11, 13];
    
    [JsonRequired][JsonPropertyName("pasteOverwrites")]
    public string PasteBehavior { get; set; } = "notes";

    [JsonRequired][JsonPropertyName("preserveNoiszFlag")]
    public bool PreserveNoiszFlag { get; set; } = true;

    [JsonRequired] [JsonPropertyName("minZoom")]
    public double MinZoom { get; set; } = 0.5;

    [JsonRequired] [JsonPropertyName("maxZoom")]
    public double MaxZoom { get; set; } = 7.5;

    [JsonRequired] [JsonPropertyName("zoomIncrement")]
    public double ZoomIncrement { get; set; } = 0.25;

    [JsonRequired][JsonPropertyName("laneOrder")]
    public List<string> LaneOrder { get; set; } = ["top", "center", "bottom", "camera"];

    [JsonRequired][JsonPropertyName("jumpTargets")]
    public List<string> JumpTargets { get; set; } =
    [
        "labels",
        "bpmChanges",
        "firstNote",
        "lastNote",
        "chartStart",
        "chartEnd"
    ];

    [JsonRequired][JsonPropertyName("doublePreviewAlpha")]
    public double DoublePreviewAlpha { get; set; } = 0.5;

    [JsonRequired][JsonPropertyName("currentTimePosition")]
    public int CurrentTimePosition { get; set; } = 175;

    [JsonRequired][JsonPropertyName("hitSoundOffset")]
    public int HitSoundOffset { get; set; } = -30;

    [JsonRequired][JsonPropertyName("hardChartOffset")]
    public int HardChartOffset { get; set; } = -60;

    [JsonRequired][JsonPropertyName("hitSoundTickRate")]
    public int HitSoundTickRate { get; set; } = 180;

    [JsonRequired][JsonPropertyName("maxConcurrentHitSounds")]
    public int MaxConcurrentSfx { get; set; } = 16;

    [JsonRequired][JsonPropertyName("autosaveInterval")]
    public int AutosaveInterval { get; set; } = 600;

    [JsonRequired][JsonPropertyName("hitSounds")]
    public HitSounds HitSounds { get; set; } = new();

    [JsonRequired][JsonPropertyName("debug")]
    public Debug DebugToggles { get; set; } = new();
    
    [JsonRequired][JsonPropertyName("enableLivePlacement")]
    public bool EnableLivePlacement { get; set; } = false;
    
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
                         auto select: {AutoSelectBehavior}
                         allow top lane cop mashes: {AllowTopLaneCopMashes}
                         show sub freestyles while placing: {ShowSubFreestylesInNoteViewer}
                         negative mash conversion: {NegativeMashConversion}
                         live placement: {EnableLivePlacement}
                         double preview alpha: {DoublePreviewAlpha}
                         autosave interval: {AutosaveInterval} seconds
                         beat snaps: [{string.Join(", ", BeatSnaps)}]
                         min zoom: {MinZoom}
                         max zoom: {MaxZoom}
                         zoom increment: {ZoomIncrement}
                         lane order: [{string.Join(", ", LaneOrder)}]
                         jump targets: [{string.Join(", ", JumpTargets)}]
                         hit sound offset: {HitSoundOffset}
                         hard chart offset: {HardChartOffset}
                         hit sound tick rate: {HitSoundTickRate}
                         max concurrent hit sounds: {MaxConcurrentSfx}
                         hit sounds: {HitSounds.GetFormattedString()}
                         quick scroll beats: {QuickScrollBeats}
                         slider increment: {SliderIncrement}
                         debug mode: {DebugToggles.Enabled}
                         {DebugToggles.GetFormattedString()}
                         """);
    }
}