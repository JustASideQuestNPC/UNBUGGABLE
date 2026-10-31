using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaDialogs.Views;
using UNBUGGABLE.ConfigParser;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class UiShowHideEventDialog : BaseDialog<bool>
{
    public long NewEndTime { get; private set; }
    
    private long _startTime;
    
    public UiShowHideEventDialog(long startTime, long endTime, List<string> enabledElements)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        
        AccuracyToggle.IsChecked = enabledElements.Contains("Accuracy");
        BgBarsToggle.IsChecked = enabledElements.Contains("BlackBGBars");
        FourByThreeBarsToggle.IsChecked = enabledElements.Contains("FourByThreeBars");
        VignetteToggle.IsChecked = enabledElements.Contains("VignetteBars");
        LeftJudgementToggle.IsChecked = enabledElements.Contains("LeftJudgementLine");
        RightJudgementToggle.IsChecked = enabledElements.Contains("RightJudgementLine");
        HealthToggle.IsChecked = enabledElements.Contains("Health");
        MaxComboToggle.IsChecked = enabledElements.Contains("MaxCombo");
        NotesToggle.IsChecked = enabledElements.Contains("Notes");
        ReticleToggle.IsChecked = enabledElements.Contains("Reticle");
        ScoreToggle.IsChecked = enabledElements.Contains("Score");
        SpeedLinesToggle.IsChecked = enabledElements.Contains("SpeedLines");
        UpNextToggle.IsChecked = enabledElements.Contains("UpNextIndicators");
    }
    
    public async Task<Optional<bool>> ShowAsync()
    {
        App.DialogIsOpen = true;
        var result = await base.ShowAsync();
        App.DialogIsOpen = false;
        return result;
    }

    public List<string> GetEnabledElements()
    {
        List<string> enabledElements = [];
        if (AccuracyToggle.IsChecked ?? false) enabledElements.Add("Accuracy");
        if (BgBarsToggle.IsChecked ?? false) enabledElements.Add("BlackBGBars");
        if (FourByThreeBarsToggle.IsChecked ?? false) enabledElements.Add("FourByThreeBars");
        if (VignetteToggle.IsChecked ?? false) enabledElements.Add("VignetteBars");
        if (LeftJudgementToggle.IsChecked ?? false) enabledElements.Add("LeftJudgementLine");
        if (RightJudgementToggle.IsChecked ?? false) enabledElements.Add("RightJudgementLine");
        if (HealthToggle.IsChecked ?? false) enabledElements.Add("Health");
        if (MaxComboToggle.IsChecked ?? false) enabledElements.Add("MaxCombo");
        if (NotesToggle.IsChecked ?? false) enabledElements.Add("Notes");
        if (ReticleToggle.IsChecked ?? false) enabledElements.Add("Reticle");
        if (ScoreToggle.IsChecked ?? false) enabledElements.Add("Score");
        if (SpeedLinesToggle.IsChecked ?? false) enabledElements.Add("SpeedLines");
        if (UpNextToggle.IsChecked ?? false) enabledElements.Add("UpNextIndicators");
        return enabledElements;
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
    
    public override EventType EventType => EventType.UI_TOGGLE;
    
    public List<string> EnabledElements { get; set; } = [];
    public List<string> DisabledElements => UiElementNames.Where(e => !EnabledElements.Contains(e))
                                                          .ToList();
    
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new UiShowHideEventDialog(Time, EndTime, EnabledElements);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true })
        {
            var newEnabledElements = editDialog.GetEnabledElements();
            if (EnabledElements.SequenceEqual(newEnabledElements))
            {
                return new UiShowHideEvent
                {
                    Time = Time,
                    EndTime = editDialog.NewEndTime,
                    EnabledElements = newEnabledElements
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
        
        List<string> lines = [];
        if (EnabledElements.Count > 0)
        {
            lines.Add(lineStart + "Show:" + string.Join('|', EnabledElements));
        }
        if (DisabledElements.Count > 0)
        {
            lines.Add(lineStart + "Hide:" + string.Join('|', DisabledElements));
        }

        return string.Join('\n', lines);
    }
}