using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

internal class PresetPositionAttribute : Attribute
{
    public string Name { get; }
    public string InternalName { get; }
    
    internal PresetPositionAttribute(string name, string internalName)
    {
        InternalName = internalName;
        Name = name;
    }
}

public static class PresetPositions
{
    public static string Name(this PresetPosition p)
    {
        return GetAttr(p)?.Name ?? Enum.GetName(p) ?? "";
    }
    
    public static string InternalName(this PresetPosition p)
    {
        return GetAttr(p)?.InternalName ?? Enum.GetName(p) ?? "";
    }   
    
    private static PresetPositionAttribute? GetAttr(PresetPosition p)
    {
        var a = Attribute.GetCustomAttribute(ForValue(p),
                                             typeof(PresetPositionAttribute));
        return (PresetPositionAttribute?)a;
    }

    private static FieldInfo? ForValue(PresetPosition p)
    {
        return typeof(PresetPosition).GetField(Enum.GetName(typeof(PresetPosition), p));
    }
}

public enum PresetPosition
{
    [PresetPosition("Left", "Left")] LEFT,
    [PresetPosition("Right", "Right")] RIGHT,
    [PresetPosition("Left Wide", "LeftWide")] LEFT_WIDE,
    [PresetPosition("Right Wide", "RightWide")] RIGHT_WIDE,
    [PresetPosition("Center Wide", "Wide")] CENTER_WIDE,
    [PresetPosition("Reset to Default", "Reset")] RESET
}

public class PositionSelectorBoxItem(PresetPosition position)
{
    public readonly PresetPosition Position = position;
    public string Name => Position.Name();
}

public partial class CameraPresetPositionEventDialog : UnanimatedEventBaseDialog
{
    public long NewEndTime { get; private set; }
    public PresetPosition NewPosition { get; private set; }
    
    private readonly long _startTime;
    private readonly ObservableCollection<PositionSelectorBoxItem> _positionSelectors = [];
    
    public CameraPresetPositionEventDialog(long startTime, long endTime, PresetPosition position)
    {
        InitializeComponent();
        _startTime = startTime;
        NewEndTime = endTime;
        NewPosition = position;

        foreach (var p in Enum.GetValues<PresetPosition>())
        {
            _positionSelectors.Add(new PositionSelectorBoxItem(p));
        }

        PositionSelectorBox.ItemsSource = _positionSelectors;
        PositionSelectorBox.SelectedIndex = (int)position;
        
        EndTimeBox.Text = endTime.ToString();
    }
    
    private void PositionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e == null || e.AddedItems.Count == 0)
        {
            return;
        }

        NewPosition = ((PositionSelectorBoxItem)PositionSelectorBox.SelectionBoxItem).Position;
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

public class CameraPresetPositionEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_PRESET_POSITION;
    public PresetPosition Position { get; set; } = PresetPosition.LEFT;
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraPresetPositionEventDialog(Time, EndTime, Position);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewEndTime != EndTime || editDialog.NewPosition != Position))
        {
            return new CameraPresetPositionEvent
            {
                Time = Time,
                EndTime = editDialog.NewEndTime,
                Position = editDialog.NewPosition
            };
        }

        return null;
    }
    
    public override string ToEventString()
    {
        return EndTime > Time ?
            $"Camera,{Time + Chart.Metadata.ChartOffset},{EndTime + Chart.Metadata.ChartOffset}," +
                $"CameraTarget:{Position.InternalName()}" :
            $"Camera,{Time + Chart.Metadata.ChartOffset},CameraTarget:{Position.InternalName()}";
    }
}