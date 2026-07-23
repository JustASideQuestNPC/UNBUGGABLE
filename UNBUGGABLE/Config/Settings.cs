using System.Collections.Generic;
using System.Diagnostics;
using YamlDotNet.Serialization;

namespace UNBUGGABLE.Resources;

public class HitSounds {
    // all of these members have to be required so that i can make the yaml parser fail if any
    // settings are missing
    public required bool Single;
    public required bool Spike;
    public required bool Freestyle;
    public required bool HoldStart;
    public required bool HoldEnd;
    public required bool DoubleStart;
    public required bool DoubleEnd;
    public required bool MashStart;
    public required bool MashEnd;
    public required bool CopSingle;
    public required bool CopHoldStart;
    public required bool CopHoldEnd;
    public required bool CopMashStart;
    public required bool CopMashEnd;
    public required bool CameraChange;
    public required bool Marker1;
    public required bool Marker2;
    public required bool Marker3;

    public string GetSettingsString()
    {
        return $"""
                
                    single: {Single}
                    spike: {Spike}
                    freestyle: {Freestyle}
                    hold start: {HoldStart}
                    hold end: {HoldEnd}
                    double start: {DoubleStart}
                    double end: {DoubleEnd}
                    mash start: {MashStart}
                    mash end: {MashEnd}
                    cop single: {CopSingle}
                    cop hold start: {CopHoldStart}
                    cop hold end: {CopHoldEnd}
                    cop mash start: {CopMashStart}
                    cop mash end: {CopMashEnd}
                    camera change: {CameraChange}
                    marker 1: {Marker1}
                    marker 2: {Marker2}
                    marker 3: {Marker3}
                """;
    }
}

public class Settings
{
    public required string ColorTheme;
    
    [YamlMember(Alias = "useBeatFiles", ApplyNamingConventions = false)]
    public required bool DefaultSaveToBeatFiles;
    public required bool EnhancedPreview;
    
    [YamlMember(Alias = "alwaysShowAllNoteFlags", ApplyNamingConventions = false)]
    public required bool AlwaysShowAllFlags;
    public required bool EnableBreakpoints;
    
    [YamlMember(Alias = "useLane2AsMarkers", ApplyNamingConventions = false)]
    public required bool Lane2Markers;

    [YamlMember(Alias = "saveMarkersAsLane2Notes", ApplyNamingConventions = false)]
    public required bool SaveMarkersInLane2;

    public required bool AlwaysEnableCustomDifficultyName;
    public required bool AutoSelectPastedNotes;
    public required bool AllowTopLaneCopMashes;

    [YamlMember(Alias = "showFreestyleSubNotesWhilePlacing", ApplyNamingConventions = false)]
    public required bool ShowSubFreestylesInNoteViewer;

    public required List<int> BeatSnaps;
    public required double MinZoom;
    public required double MaxZoom;
    public required double ZoomIncrement;
    public required List<string> LaneOrder;
    public required double HitSoundOffset;
    public required double HardChartOffset;
    public required double HitSoundTickRate;
    public required double CurrentTimePosition;
    public required HitSounds HitSounds;

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
                         beat snaps: [{string.Join(", ", BeatSnaps)}]
                         min zoom: {MinZoom}
                         max zoom: {MaxZoom}
                         zoom increment: {ZoomIncrement}
                         lane order: [{string.Join(", ", LaneOrder)}]
                         hit sound offset: {HitSoundOffset}
                         hard chart offset: {HardChartOffset}
                         hit sound tick rate: {HitSoundTickRate}
                         hit sounds: {HitSounds.GetSettingsString()}
                         """);
    }
}