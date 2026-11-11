using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class CameraResetAllEventDialog : UnanimatedEventBaseDialog
{
    public long NewEndTime { get; private set; }
    
    private readonly long _startTime;
    
    public CameraResetAllEventDialog(long startTime, long endTime)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        EndTimeBox.Text = endTime.ToString();
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

public class CameraResetAllEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_RESET_ALL;
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraResetAllEventDialog(Time, EndTime);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } && editDialog.NewEndTime != EndTime || isNew)
        {
            return new CameraResetAllEvent
            {
                Time = Time,
                EndTime = editDialog.NewEndTime
            };
        }

        return null;
    }
    
    public override string ToEventString()
    {
        return EndTime > Time ?
            $"Camera,{Time + Chart.Metadata.ChartOffset},{EndTime + Chart.Metadata.ChartOffset}," +
                $"Reset" :
            $"Camera,{Time + Chart.Metadata.ChartOffset},Reset";
    }
}