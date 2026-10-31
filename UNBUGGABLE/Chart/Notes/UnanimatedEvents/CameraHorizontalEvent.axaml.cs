using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class CameraHorizontalEventDialog : UnanimatedEventBaseDialog
{
    public long NewEndTime { get; private set; }
    public double NewDistance => (double)(DistanceInput.Value ?? 0);
    public bool NewOffset => OffsetToggle.IsChecked ?? false;
    
    private readonly long _startTime;
    
    public CameraHorizontalEventDialog(long startTime, long endTime, double distance, bool offset)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        OffsetToggle.IsChecked = offset;
        EndTimeBox.Text = endTime.ToString();
        DistanceInput.Value = (decimal)distance;
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

public class CameraHorizontalEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_HORIZONTAL;
    
    public double Distance { get; set; } = 0;
    public bool Offset { get; set; } = false;
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraHorizontalEventDialog(Time, EndTime, Distance, Offset);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewEndTime != EndTime || editDialog.NewDistance != Distance ||
             editDialog.NewOffset != Offset))
        {
            return new CameraHorizontalEvent
            {
                Time = Time,
                EndTime = editDialog.NewEndTime,
                Distance = editDialog.NewDistance,
                Offset = editDialog.NewOffset
            };
        }

        return null;
    }
    
    public override string ToEventString()
    {
        var eventString = EndTime > Time ?
            $"Camera,{Time + Chart.Metadata.ChartOffset},{EndTime + Chart.Metadata.ChartOffset}," :
            $"Camera,{Time + Chart.Metadata.ChartOffset},";
        eventString += Offset ? "HorizontalOffset:" : "HorizontalTarget:";
        eventString += $"{Distance}";
        return eventString;
    }
}