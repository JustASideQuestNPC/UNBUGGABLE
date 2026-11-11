using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class CameraRotateEventDialog : UnanimatedEventBaseDialog
{
    public long NewEndTime { get; private set; }
    public double NewAngle => (double)(AngleInput.Value ?? 0);
    public bool NewOffset => OffsetToggle.IsChecked ?? false;
    
    private readonly long _startTime;
    
    public CameraRotateEventDialog(long startTime, long endTime, double angle, bool offset)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        OffsetToggle.IsChecked = offset;
        EndTimeBox.Text = endTime.ToString();
        AngleInput.Value = (decimal)angle;
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

public class CameraRotateEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_ROTATE;
    
    public double Angle { get; set; } = 0;
    public bool Offset { get; set; } = false;
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraRotateEventDialog(Time, EndTime, Angle, Offset);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewEndTime != EndTime || editDialog.NewAngle != Angle ||
             editDialog.NewOffset != Offset || isNew))
        {
            return new CameraRotateEvent
            {
                Time = Time,
                EndTime = editDialog.NewEndTime,
                Angle = editDialog.NewAngle,
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
        eventString += Offset ? "RotOffset:" : "RotTarget:";
        eventString += $"{Angle}";
        return eventString;
    }
}