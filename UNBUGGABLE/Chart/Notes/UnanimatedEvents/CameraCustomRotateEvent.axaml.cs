using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class CameraCustomRotateEventDialog : UnanimatedEventBaseDialog
{
    public bool NewOffset => OffsetToggle.IsChecked ?? false;
    public bool NewUseStart => UseStartToggleButton.IsChecked ?? false;
    public bool NewInstant => InstantToggle.IsChecked ?? false;

    public Vector3D NewAngleVector => new((double)(XInput.Value ?? 0), (double)(YInput.Value ?? 0),
                                          (double)(ZInput.Value ?? 0));
    
    public Vector3D NewStartAngleVector => new((double)(StartXInput.Value ?? 0),
                                                (double)(StartYInput.Value ?? 0),
                                                (double)(StartZInput.Value ?? 0));

    public CameraCustomRotateEventDialog(bool offset, bool useStart, bool instant,
        Vector3D angleVector, Vector3D startAngleVector)
    {
        InitializeComponent();
        OffsetToggle.IsChecked = offset;
        UseStartToggleButton.IsChecked = useStart;
        StartPointContainer.IsVisible = useStart;
        StartPointContainer.IsEnabled = useStart;
        InstantToggle.IsChecked = instant;

        XInput.Value = (decimal)angleVector.X;
        YInput.Value = (decimal)angleVector.Y;
        ZInput.Value = (decimal)angleVector.Z;
        StartXInput.Value = (decimal)startAngleVector.X;
        StartYInput.Value = (decimal)startAngleVector.Y;
        StartZInput.Value = (decimal)startAngleVector.Z;
    }

    private void UseStartToggleClicked(object? sender, RoutedEventArgs e)
    {
        StartPointContainer.IsVisible = UseStartToggleButton.IsChecked ?? false;
        StartPointContainer.IsEnabled = UseStartToggleButton.IsChecked ?? false;
    }
}

public class CameraCustomRotateEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_CUSTOM_ROTATE;
    
    public bool Offset { get; set; }
    public Vector3D AngleVector { get; set; }
    public Vector3D StartAngleVector { get; set; }
    public bool UseStart { get; set; }
    public new bool Instant { get; set; }
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraCustomRotateEventDialog(Offset, UseStart, Instant, AngleVector,
                                                             StartAngleVector);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewOffset != Offset || editDialog.NewUseStart != UseStart ||
             editDialog.NewInstant != Instant || !editDialog.NewAngleVector.Equals(AngleVector) ||
             !editDialog.NewStartAngleVector.Equals(StartAngleVector)))
        {
            return new CameraCustomRotateEvent
            {
                Time = Time,
                Offset = editDialog.NewOffset,
                AngleVector = editDialog.NewAngleVector,
                UseStart = editDialog.NewUseStart,
                Instant = editDialog.NewInstant,
                StartAngleVector = editDialog.NewStartAngleVector
            };
        }

        return null;
    }
    
    public override string ToEventString()
    {
        var eventString = $"Camera,{Time + Chart.Metadata.ChartOffset},";
        eventString += Offset ? "CustomRotOffset," : "CustomRotTarget,";
        eventString += $"{AngleVector.X}:{AngleVector.Y}:{AngleVector.Z}";
        if (Instant)
        {
            eventString += ":Instant";
        }

        if (UseStart)
        {
            eventString +=
                $":StartPoint:{StartAngleVector.X}:{StartAngleVector.Y}:{StartAngleVector.Z}";
        }
        return eventString;
    }
}