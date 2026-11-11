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

internal class SlideStateAttribute : Attribute
{
    public string Name { get; }
    
    internal SlideStateAttribute(string name)
    {
        Name = name;
    }
}

public static class SlideStates
{
    public static string Name(this SlideState s)
    {
        return GetAttr(s)?.Name ?? Enum.GetName(s) ?? "";
    }
    
    private static SlideStateAttribute? GetAttr(SlideState s)
    {
        var a = Attribute.GetCustomAttribute(ForValue(s),
                                             typeof(SlideStateAttribute));
        return (SlideStateAttribute?)a;
    }

    private static FieldInfo? ForValue(SlideState s)
    {
        return typeof(SlideState).GetField(Enum.GetName(typeof(SlideState), s));
    }
}

public enum SlideState
{
    [SlideState("none")] NONE,
    [SlideState("in")] IN,
    [SlideState("out")] OUT,
}

public partial class UiSlideEventDialog : BaseDialog<bool>
{
    public long NewEndTime { get; private set; }
    public Dictionary<string, SlideState> NewSlideStates { get; set; } = new();
    
    private long _startTime;
    
    public UiSlideEventDialog(long startTime, long endTime,
        Dictionary<string, SlideState> slideStates)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        NewSlideStates = slideStates;

        OverlayToggleText.Text = slideStates.TryGetValue("AllOverlayUI", out var value) ?
            value.Name() : "none";
        GradientToggleText.Text = slideStates.TryGetValue("BackgroundGradient", out value) ?
            value.Name() : "none";
        BgBarsToggleText.Text = slideStates.TryGetValue("BlackBGBars", out value) ?
            value.Name() : "none";
        FourByThreeBarsToggleText.Text = slideStates.TryGetValue("FourByThreeBars", out value) ?
            value.Name() : "none";
        LeftJudgementToggleText.Text = slideStates.TryGetValue("LeftJudgementLine", out value) ?
            value.Name() : "none";
        RightJudgementToggleText.Text = slideStates.TryGetValue("RightJudgementLine", out value) ?
            value.Name() : "none";
        HealthBarToggleText.Text = slideStates.TryGetValue("HealthBar", out value) ?
            value.Name() : "none";
        SpeedLinesToggleText.Text = slideStates.TryGetValue("SpeedLines", out value) ?
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
            "OverlayToggle" => "AllOverlayUI",
            "GradientToggle" => "BackgroundGradient",
            "BgBarsToggle" => "BlackBGBars",
            "FourByThreeBarsToggle" => "FourByThreeBars",
            "LeftJudgementToggle" => "LeftJudgementLine",
            "RightJudgementToggle" => "RightJudgementLine",
            "HealthBarToggle" => "HealthBar",
            "SpeedLinesToggle" => "SpeedLines",
            _ => throw new Exception("Invalid toggle button")
        };

        if (NewSlideStates.TryGetValue(dictKey, out var value))
        {
            NewSlideStates[dictKey] = (SlideState)(((int)value + 1) % 3);
            ((TextBlock)button.Content).Text = NewSlideStates[dictKey].Name();
        }
    }
}

public class UiSlideEvent : UnanimatedEventBase
{
    public static readonly List<string> UiElementNames = [
        "AllOverlayUI",
        "BackgroundGradient",
        "BlackBGBars",
        "FourByThreeBars",
        "LeftJudgementLine",
        "RightJudgementLine",
        "HealthBar",
        "SpeedLines"
    ];
    
    public override EventType EventType => EventType.UI_SLIDE;

    public Dictionary<string, SlideState> SlideStates { get; set; } = new()
    {
        ["AllOverlayUI"] = SlideState.NONE,
        ["BackgroundGradient"] = SlideState.NONE,
        ["BlackBGBars"] = SlideState.NONE,
        ["FourByThreeBars"] = SlideState.NONE,
        ["LeftJudgementLine"] = SlideState.NONE,
        ["RightJudgementLine"] = SlideState.NONE,
        ["HealthBar"] = SlideState.NONE,
        ["SpeedLines"] = SlideState.NONE,
    };
    
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new UiSlideEventDialog(Time, EndTime, SlideStates);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true })
        {
            var edited = editDialog.NewEndTime != EndTime || isNew;
            if (!edited)
            {
                foreach (var (key, value) in editDialog.NewSlideStates)
                {
                    if (SlideStates.TryGetValue(key, out var oldValue))
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
                return new UiSlideEvent
                {
                    Time = Time,
                    EndTime = editDialog.NewEndTime,
                    SlideStates = editDialog.NewSlideStates
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

        var slideInElements = UiElementNames.FindAll(e => SlideStates[e] == SlideState.IN);
        var slideOutElements = UiElementNames.FindAll(e => SlideStates[e] == SlideState.OUT);

        List<string> lines = [];
        if (slideInElements.Count > 0)
        {
            lines.Add(lineStart + "SlideIn:" + string.Join('|', slideInElements));
        }
        if (slideOutElements.Count > 0)
        {
            lines.Add(lineStart + "SlideOut:" + string.Join('|', slideOutElements));
        }
        
        return string.Join('\n', lines);
    }
}