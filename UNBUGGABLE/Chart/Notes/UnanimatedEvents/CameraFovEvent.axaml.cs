using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class CameraFovEventDialog : UnanimatedEventBaseDialog
{
    public long NewEndTime { get; private set; }
    public double NewFov => (double)(FovInput.Value ?? 0);
    public bool NewOffset => OffsetToggle.IsChecked ?? false;
    
    private readonly long _startTime;
    
    public CameraFovEventDialog(long startTime, long endTime, double fov, bool offset)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        OffsetToggle.IsChecked = offset;
        EndTimeBox.Text = endTime.ToString();
        FovInput.Value = (decimal)fov;
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

public class CameraFovEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_FOV;
    
    public double Fov { get; set; } = 90;
    public bool Offset { get; set; } = false;
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraFovEventDialog(Time, EndTime, Fov, Offset);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewEndTime != EndTime || editDialog.NewFov != Fov ||
             editDialog.NewOffset != Offset))
        {
            return new CameraFovEvent
            {
                Time = Time,
                EndTime = editDialog.NewEndTime,
                Fov = editDialog.NewFov,
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
        eventString += Offset ? "FOVOffset:" : "FOVTarget:";
        eventString += $"{Fov}";
        return eventString;
    }
}