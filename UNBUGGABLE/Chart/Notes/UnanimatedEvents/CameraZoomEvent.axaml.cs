using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class CameraZoomEventDialog : UnanimatedEventBaseDialog
{
    public long NewEndTime { get; private set; }
    public double NewZoom => (double)(ZoomInput.Value ?? 0);
    public bool NewOffset => OffsetToggle.IsChecked ?? false;
    
    private readonly long _startTime;
    
    public CameraZoomEventDialog(long startTime, long endTime, double zoom, bool offset)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        OffsetToggle.IsChecked = offset;
        EndTimeBox.Text = endTime.ToString();
        ZoomInput.Value = (decimal)zoom;
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

public class CameraZoomEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_ZOOM;
    
    public double Zoom { get; set; } = 1;
    public bool Offset { get; set; } = false;
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraZoomEventDialog(Time, EndTime, Zoom, Offset);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewEndTime != EndTime || editDialog.NewZoom != Zoom ||
             editDialog.NewOffset != Offset))
        {
            return new CameraZoomEvent
            {
                Time = Time,
                EndTime = editDialog.NewEndTime,
                Zoom = editDialog.NewZoom,
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
        eventString += Offset ? "ZoomOffset:" : "ZoomTarget:";
        eventString += $"{Math.Abs(Zoom)}";
        return eventString;
    }
}