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

public partial class UiSlideEventDialog : BaseDialog<bool>
{
    public long NewEndTime { get; private set; }
    
    private long _startTime;
    
    public UiSlideEventDialog(long startTime, long endTime, List<string> slideInElements)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        
        OverlayToggle.IsChecked = slideInElements.Contains("AllOverlayUI");
        GradientToggle.IsChecked = slideInElements.Contains("BackgroundGradient");
        BgBarsToggle.IsChecked = slideInElements.Contains("BlackBGBars");
        FourByThreeBarsToggle.IsChecked = slideInElements.Contains("FourByThreeBars");
        LeftJudgementToggle.IsChecked = slideInElements.Contains("LeftJudgementLine");
        RightJudgementToggle.IsChecked = slideInElements.Contains("RightJudgementLine");
        HealthToggle.IsChecked = slideInElements.Contains("HealthBar");
        SpeedLinesToggle.IsChecked = slideInElements.Contains("SpeedLines");
    }
    
    public async Task<Optional<bool>> ShowAsync()
    {
        App.DialogIsOpen = true;
        var result = await base.ShowAsync();
        App.DialogIsOpen = false;
        return result;
    }

    public List<string> GetSlideInElements()
    {
        List<string> slideInElements = [];
        if (OverlayToggle.IsChecked ?? false) slideInElements.Add("AllOverlayUI");
        if (GradientToggle.IsChecked ?? false) slideInElements.Add("BackgroundGradient");
        if (BgBarsToggle.IsChecked ?? false) slideInElements.Add("BlackBGBars");
        if (FourByThreeBarsToggle.IsChecked ?? false) slideInElements.Add("FourByThreeBars");
        if (LeftJudgementToggle.IsChecked ?? false) slideInElements.Add("LeftJudgementLine");
        if (RightJudgementToggle.IsChecked ?? false) slideInElements.Add("RightJudgementLine");
        if (HealthToggle.IsChecked ?? false) slideInElements.Add("HealthBar");
        if (SpeedLinesToggle.IsChecked ?? false) slideInElements.Add("SpeedLines");
        return slideInElements;
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
    
    public List<string> SlideInElements { get; set; } = [];
    public List<string> SlideOutElements => UiElementNames.Where(e => !SlideInElements.Contains(e))
                                                          .ToList();
    
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new UiSlideEventDialog(Time, EndTime, SlideInElements);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true })
        {
            var newSlideInElements = editDialog.GetSlideInElements();
            if (SlideInElements.SequenceEqual(newSlideInElements))
            {
                return new UiSlideEvent
                {
                    Time = Time,
                    EndTime = editDialog.NewEndTime,
                    SlideInElements = newSlideInElements
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
        if (SlideInElements.Count > 0)
        {
            lines.Add(lineStart + "SlideIn:" + string.Join('|', SlideInElements));
        }
        if (SlideOutElements.Count > 0)
        {
            lines.Add(lineStart + "SlideOut:" + string.Join('|', SlideOutElements));
        }

        return string.Join('\n', lines);
    }
}