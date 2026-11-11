using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaDialogs.Views;
using UNBUGGABLE.ConfigParser;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

internal class ShowHideStateAttribute : Attribute
{
    public string Name { get; }
    
    internal ShowHideStateAttribute(string name)
    {
        Name = name;
    }
}

public static class ShowHideStates
{
    public static string Name(this ShowHideState s)
    {
        return GetAttr(s)?.Name ?? Enum.GetName(s) ?? "";
    }
    
    private static ShowHideStateAttribute? GetAttr(ShowHideState s)
    {
        var a = Attribute.GetCustomAttribute(ForValue(s),
                                             typeof(ShowHideStateAttribute));
        return (ShowHideStateAttribute?)a;
    }

    private static FieldInfo? ForValue(ShowHideState s)
    {
        return typeof(ShowHideState).GetField(Enum.GetName(typeof(ShowHideState), s));
    }
}

public enum ShowHideState
{
    [ShowHideState("none")] NONE,
    [ShowHideState("show")] SHOW,
    [ShowHideState("hide")] HIDE,
}

public partial class UiShowHideEventDialog : BaseDialog<bool>
{
    public long NewEndTime { get; private set; }
    public Dictionary<string, ShowHideState> NewShowHideStates { get; set; } = new();
    
    private long _startTime;
    
    public UiShowHideEventDialog(long startTime, long endTime,
        Dictionary<string, ShowHideState> showHideStates)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        NewShowHideStates = showHideStates;
        
        AccuracyToggleText.Text = showHideStates.TryGetValue("Accuracy", out var value) ?
            value.Name() : "none";
        BgBarsToggleText.Text = showHideStates.TryGetValue("BlackBGBars", out value) ?
            value.Name() : "none";
        FourByThreeBarsToggleText.Text = showHideStates.TryGetValue("FourByThreeBars", out value) ?
            value.Name() : "none";
        VignetteToggleText.Text = showHideStates.TryGetValue("VignetteBars", out value) ?
            value.Name() : "none";
        LeftJudgementToggleText.Text = showHideStates.TryGetValue("LeftJudgementLine", out value) ?
            value.Name() : "none";
        RightJudgementToggleText.Text =
            showHideStates.TryGetValue("RightJudgementLine", out value) ? value.Name() : "none";
        HealthToggleText.Text = showHideStates.TryGetValue("Health", out value) ?
            value.Name() : "none";
        MaxComboToggleText.Text = showHideStates.TryGetValue("MaxCombo", out value) ?
            value.Name() : "none";
        ReticleToggleText.Text = showHideStates.TryGetValue("Reticle", out value) ?
            value.Name() : "none";
        ScoreToggleText.Text = showHideStates.TryGetValue("Score", out value) ?
            value.Name() : "none";
        SpeedLinesToggleText.Text = showHideStates.TryGetValue("SpeedLines", out value) ?
            value.Name() : "none";
        UpNextToggleText.Text = showHideStates.TryGetValue("UpNextIndicators", out value) ?
            value.Name() : "none";
        MeasureBarsToggleText.Text = showHideStates.TryGetValue("MeasureBars", out value) ?
            value.Name() : "none";
    }
    
    public async Task<Optional<bool>> ShowAsync()
    {
        App.DialogIsOpen = true;
        var result = await base.ShowAsync();
        App.DialogIsOpen = false;
        return result;
    }
    
    protected void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
    
    protected void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
    
    private void EndTimeBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(EndTimeBox.Text) &&
            long.TryParse(EndTimeBox.Text, out var endTime))
        {
            if (endTime > _startTime)
            {
                NewEndTime = endTime;
                EndTimeBox.Text = endTime.ToString();
            }
            else if (endTime == 0)
            {
                NewEndTime = _startTime;
                EndTimeBox.Text = _startTime.ToString();
            }
        }
        else
        {
            NewEndTime = _startTime;
            EndTimeBox.Text = _startTime.ToString();
        }
    }
    
    private void ToggleButtonClick(object? sender, RoutedEventArgs e)
    {
        var button = (Button)e.Source;
        var dictKey = button.Name switch
        {
            "AccuracyToggle" => "Accuracy",
            "BgBarsToggle" => "BlackBGBars",
            "FourByThreeBarsToggle" => "FourByThreeBars",
            "VignetteToggle" => "VignetteBars",
            "MeasureBarsToggle" => "MeasureBars",
            "LeftJudgementToggle" => "LeftJudgementLine",
            "RightJudgementToggle" => "RightJudgementLine",
            "HealthToggle" => "Health",
            "MaxComboToggle" => "MaxCombo",
            "ReticleToggle" => "Reticle",
            "ScoreToggle" => "Score",
            "SpeedLinesToggle" => "SpeedLines",
            "UpNextToggle" => "UpNextIndicators",
            "NotesToggle" => "Notes",
            _ => throw new Exception("Invalid toggle button")
        };

        if (NewShowHideStates.TryGetValue(dictKey, out var value))
        {
            NewShowHideStates[dictKey] = (ShowHideState)(((int)value + 1) % 3);
            ((TextBlock)button.Content).Text = NewShowHideStates[dictKey].Name();
        }
    }
}

public class UiShowHideEvent : UnanimatedEventBase
{
    public static readonly List<string> UiElementNames = [
        "Accuracy",
        "BlackBGBars",
        "FourByThreeBars",
        "VignetteBars",
        "MeasureBars",
        "LeftJudgementLine",
        "RightJudgementLine",
        "Health",
        "MaxCombo",
        "Notes",
        "Reticle",
        "Score",
        "SpeedLines",
        "UpNextIndicators"
    ];
    
    public override EventType EventType => EventType.UI_SHOW_HIDE;
    
    public Dictionary<string, ShowHideState> ShowHideStates { get; set; } = new()
    {
        ["Accuracy"] = ShowHideState.NONE,
        ["BlackBGBars"] = ShowHideState.NONE,
        ["FourByThreeBars"] = ShowHideState.NONE,
        ["VignetteBars"] = ShowHideState.NONE,
        ["MeasureBars"] = ShowHideState.NONE,
        ["LeftJudgementLine"] = ShowHideState.NONE,
        ["RightJudgementLine"] = ShowHideState.NONE,
        ["Health"] = ShowHideState.NONE,
        ["MaxCombo"] = ShowHideState.NONE,
        ["Notes"] = ShowHideState.NONE,
        ["Reticle"] = ShowHideState.NONE,
        ["Score"] = ShowHideState.NONE,
        ["SpeedLines"] = ShowHideState.NONE,
        ["UpNextIndicators"] = ShowHideState.NONE,
    };
    
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new UiShowHideEventDialog(Time, EndTime, ShowHideStates);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true })
        {
            var edited = editDialog.NewEndTime != EndTime || isNew;
            if (!edited)
            {
                foreach (var (key, value) in editDialog.NewShowHideStates)
                {
                    if (ShowHideStates.TryGetValue(key, out var oldValue))
                    {
                        if (value != oldValue)
                        {
                            edited = true;
                            break;
                        }
                    }
                }
            }

            if (edited)
            {
                return new UiShowHideEvent
                {
                    Time = Time,
                    EndTime = editDialog.NewEndTime,
                    ShowHideStates = editDialog.NewShowHideStates
                };
            }
        }

        return null;
    }

    public override string ToEventString()
    {
        var lineStart = EndTime > Time ?
            $"UI,{Time + Chart.Metadata.ChartOffset},{EndTime + Chart.Metadata.ChartOffset}," :
            $"UI,{Time + Chart.Metadata.ChartOffset},";
        
        var showElements = UiElementNames.FindAll(e => ShowHideStates[e] == ShowHideState.SHOW);
        var hideElements = UiElementNames.FindAll(e => ShowHideStates[e] == ShowHideState.HIDE);

        List<string> lines = [];
        if (showElements.Count > 0)
        {
            lines.Add(lineStart + "Show:" + string.Join('|', showElements));
        }
        if (hideElements.Count > 0)
        {
            lines.Add(lineStart + "Hide:" + string.Join('|', hideElements));
        }

        return string.Join('\n', lines);
    }
}