using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class CameraEaseDurationEventDialog : UnanimatedEventBaseDialog
{
    public long NewDuration { get; private set; }
    
    public CameraEaseDurationEventDialog(long duration)
    {
        InitializeComponent();
        NewDuration = duration;
        DurationBox.Text = duration.ToString();
    }
    
    private void DurationBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(DurationBox.Text) &&
            long.TryParse(DurationBox.Text, out var duration))
        {
            if (duration > 0)
            {
                NewDuration = duration;
                DurationBox.Text = duration.ToString();
            }
        }
        else
        {
            NewDuration = 0;
            DurationBox.Text = "0";
        }
    }
}

public class CameraEaseDurationEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_EASE_TIME;
    
    public long Duration { get; set; } = 0;
    
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraEaseDurationEventDialog(EndTime);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewDuration != Duration || isNew))
        {
            return new CameraEaseDurationEvent
            {
                Time = Time,
                Duration = editDialog.NewDuration
            };
        }

        return null;
    }
    
    public override string ToEventString()
    {
        return $"Camera,{Time + Chart.Metadata.ChartOffset},EaseTime:{Duration}";
    }
}