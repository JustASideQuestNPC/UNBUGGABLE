using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

internal class StageAttribute : Attribute
{
    public string Name { get; }
    public string InternalName { get; }
    public bool BaseGameAccessible { get; }
    public bool RequiresDlc { get; }
    
    internal StageAttribute(string name, string internalName, bool baseGameAccessible = true,
        bool requiresDlc = false)
    {
        InternalName = internalName;
        Name = name != "" ? name : internalName;
        BaseGameAccessible = baseGameAccessible;
        RequiresDlc = requiresDlc;
    }
}

public static class Stages
{
    public static string Name(this Stage s)
    {
        return GetAttr(s)?.Name ?? Enum.GetName(s) ?? "";
    }
    
    public static string InternalName(this Stage s)
    {
        return GetAttr(s)?.InternalName ?? Enum.GetName(s) ?? "";
    }
    
    public static bool BaseGameAccessible(this Stage s)
    {
        return GetAttr(s)?.BaseGameAccessible ?? false;
    }
    
    public static bool RequiresDlc(this Stage s)
    {
        return GetAttr(s)?.RequiresDlc ?? false;
    }
    
    private static StageAttribute? GetAttr(Stage s)
    {
        var a = Attribute.GetCustomAttribute(ForValue(s),
                                             typeof(StageAttribute));
        return (StageAttribute?)a;
    }

    private static FieldInfo? ForValue(Stage s)
    {
        return typeof(Stage).GetField(Enum.GetName(typeof(Stage), s));
    }
}

public enum Stage
{
    [Stage("None", "")] NONE,
    [Stage("Train Station", "TrainStationRhythm")] TRAIN_STATION,
    [Stage("Stadium (Past)", "StadiumPast")] STADIUM_PAST,
    [Stage("Stadium (Present)", "StadiumPresent")] STADIUM_PRESENT,
    [Stage("Dreamscape", "DreamReflection")] DREAM,
    [Stage("Prison Yard", "PrisonYard")] PRISON_YARD,
    [Stage("Train", "MovingTrain")] TRAIN,
    [Stage("Lighthouse Show", "Lighthouse")] LIGHTHOUSE,
    [Stage("City Hideout", "LoadingDockInterior")] LOADING_DOCK,
    [Stage("Recording Studio", "RecordingStudio")] RECORDING_STUDIO,
    [Stage("Overpass", "CityStreet")] CITY_STREET,
    [Stage("Underground Show", "RichPeopleConcert")] RICH_PEOPLE_CONCERT,
    [Stage("Alleyway Show", "AlleywayStage")] ALLEYWAY,
    [Stage("Warehouse Show", "WarehouseStage")] WAREHOUSE,
    [Stage("HARM HQ Lobby", "HARMLobby")] HARM_LOBBY,
    [Stage("Zero Moment Array", "HARMZeroMomentArray")] ZM_ARRAY,
    [Stage("Zero Moment Test Chamber", "HARMSforzandoArena")] ZM_TEST_CHAMBER,
    [Stage("Graveyard", "HARMGraveyard")] GRAVEYARD,
    // no straight roads collab
    [Stage("NSR", "NSR_Stage")] NSR,
    [Stage("Greenscreen", "GreenscreenRhythm")] GREENSCREEN,
    [Stage("Playback", "PlaybackStage")] PLAYBACK,
    // requires jamie paige dlc
    [Stage("Birdbrain", "BirdBrainRhythm", true, true)] BIRDBRAIN,
    // also requires jamie page
    [Stage("Women Wrestling", "WomenWrestlingRhythm", true, true)] WOMEN_WRESTLING,
    // used by both noisz stages, enables fade-to-black and the alternate spike sprite
    [Stage("Noisz", "NOISZRhythm", false)] NOISZ,
    // used by proper rhythm (tutorial), black background and nothing else
    [Stage("Tutorial", "TutorialRhythm", false)] TUTORIAL,
    // used by waiting (lumena-tan remix), uses the train station stage and applies a filter
    [Stage("Train Station (Pixelated)", "TrainStationRhythmPixel", false)] TRAIN_STATION_PIXEL,
}

public class StageSelectorBoxItem(Stage stage)
{
    public readonly Stage Stage = stage;
    public string Name => stage.Name();
}

public partial class ChangeStageEventDialog : UnanimatedEventBaseDialog
{
    public Stage NewStage { get; private set; }
    
    private ObservableCollection<StageSelectorBoxItem> _stageSelectors = [];
    
    public ChangeStageEventDialog(Stage currentStage)
    {
        InitializeComponent();

        foreach (var stage in Enum.GetValues<Stage>())
        {
            if (stage == Stage.NONE)
            {
                continue;
            }
            
            _stageSelectors.Add(new StageSelectorBoxItem(stage));
        }
        StageSelectorBox.ItemsSource = _stageSelectors;
        StageSelectorBox.SelectedIndex = (int)currentStage;
        RequiresDlcAlertIcon.IsVisible = currentStage.RequiresDlc();
    }

    private void StageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e == null || e.AddedItems.Count == 0)
        {
            return;
        }

        NewStage = ((StageSelectorBoxItem)StageSelectorBox.SelectionBoxItem).Stage;
        RequiresDlcAlertIcon.IsVisible = NewStage.RequiresDlc();
    }
}

public class ChangeStageEvent : UnanimatedEventBase
{
    public Stage Stage { get; set; } = Stage.NONE;

    public override EventType EventType => EventType.STAGE_CHANGE;

    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool _ = false)
    {
        var editDialog = new ChangeStageEventDialog(Stage);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } && editDialog.NewStage != Stage)
        {
            return new ChangeStageEvent
            {
                Time = Time,
                Stage = editDialog.NewStage
            };
        }
        return null;
    }

    public override string ToEventString()
    {
        return $"StageScene,{Time + Chart.Metadata.ChartOffset},{Stage.InternalName()}";
    }
}