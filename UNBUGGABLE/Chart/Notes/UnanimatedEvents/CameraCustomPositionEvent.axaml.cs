using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class CameraCustomPositionEventDialog : UnanimatedEventBaseDialog
{
    public bool NewOffset => OffsetToggle.IsChecked ?? false;
    public bool NewUseStart => UseStartToggleButton.IsChecked ?? false;
    public bool NewInstant => InstantToggle.IsChecked ?? false;

    public Vector3D NewPosition => new((double)(XInput.Value ?? 0), (double)(YInput.Value ?? 0),
                                       (double)(ZInput.Value ?? 0));
    
    public Vector3D NewStartPosition => new((double)(StartXInput.Value ?? 0),
                                            (double)(StartYInput.Value ?? 0),
                                            (double)(StartZInput.Value ?? 0));
    
    public CameraCustomPositionEventDialog(bool offset, bool useStart, bool instant,
        Vector3D position, Vector3D startPosition)
    {
        InitializeComponent();
        OffsetToggle.IsChecked = offset;
        UseStartToggleButton.IsChecked = useStart;
        StartPointContainer.IsVisible = useStart;
        StartPointContainer.IsEnabled = useStart;
        InstantToggle.IsChecked = instant;
        
        XInput.Value = (decimal)position.X;
        YInput.Value = (decimal)position.Y;
        ZInput.Value = (decimal)position.Z;
        StartXInput.Value = (decimal)startPosition.X;
        StartYInput.Value = (decimal)startPosition.Y;
        StartZInput.Value = (decimal)startPosition.Z;
    }

    private void UseStartToggleClicked(object? sender, RoutedEventArgs e)
    {
        StartPointContainer.IsVisible = UseStartToggleButton.IsChecked ?? false;
        StartPointContainer.IsEnabled = UseStartToggleButton.IsChecked ?? false;
    }
}

public class CameraCustomPositionEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_CUSTOM_POSITION;
    
    public bool Offset { get; set; }
    public Vector3D Position { get; set; }
    public Vector3D StartPosition { get; set; }
    public bool UseStart { get; set; }
    public new bool Instant { get; set; }
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraCustomPositionEventDialog(Offset, UseStart, Instant, Position,
                                                             StartPosition);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewOffset != Offset || editDialog.NewUseStart != UseStart ||
             editDialog.NewInstant != Instant || !editDialog.NewPosition.Equals(Position) ||
             !editDialog.NewStartPosition.Equals(StartPosition)))
        {
            return new CameraCustomPositionEvent
            {
                Time = Time,
                Offset = editDialog.NewOffset,
                Position = editDialog.NewPosition,
                UseStart = editDialog.NewUseStart,
                Instant = editDialog.NewInstant,
                StartPosition = editDialog.NewStartPosition
            };
        }

        return null;
    }
    
    public override string ToEventString()
    {
        var eventString = $"Camera,{Time + Chart.Metadata.ChartOffset},";
        eventString += Offset ? "CustomCameraOffset," : "CustomCameraTarget,";
        eventString += $"{Position.X}:{Position.Y}:{Position.Z}";
        if (Instant)
        {
            eventString += ":Instant";
        }

        if (UseStart)
        {
            eventString += $":StartPoint:{StartPosition.X}:{StartPosition.Y}:{StartPosition.Z}";
        }
        return eventString;
    }
}