using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Interactivity;
using AvaloniaDialogs.Views;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

internal class EventTypeAttribute : Attribute
{
    public string Name { get; }
    public Type? NoteClass { get; }
    
    internal EventTypeAttribute(string name, Type? noteClass)
    {
        Name = name;
        NoteClass = noteClass;
    }
}

public static class EventTypes
{
    public static string Name(this EventType t)
    {
        return GetAttr(t)?.Name ?? Enum.GetName(t) ?? "";
    }

    public static Type? NoteClass(this EventType t)
    {
        return GetAttr(t)?.NoteClass ?? null;
    }
    
    private static EventTypeAttribute? GetAttr(EventType t)
    {
        var a = Attribute.GetCustomAttribute(ForValue(t),
                                             typeof(EventTypeAttribute));
        return (EventTypeAttribute?)a;
    }

    private static FieldInfo? ForValue(EventType t)
    {
        return typeof(EventType).GetField(Enum.GetName(typeof(EventType), t));
    }
}

/// <summary>
/// All available trigger types. Camera triggers with both offset and target modes are merged into
/// a single enum value.
/// </summary>
public enum EventType
{
    [EventType("Reset Camera", typeof(CameraResetAllEvent))] CAMERA_RESET_ALL,
    [EventType("Move Camera to Preset Position", typeof(CameraPresetPositionEvent))] CAMERA_PRESET_POSITION,
    [EventType("Move Camera Horizontally", typeof(CameraHorizontalEvent))] CAMERA_HORIZONTAL,
    [EventType("3D Camera Movement", typeof(CameraCustomPositionEvent))] CAMERA_CUSTOM_POSITION,
    [EventType("Zoom Camera In/Out", typeof(CameraZoomEvent))] CAMERA_ZOOM,
    [EventType("Basic Camera Rotation", typeof(CameraRotateEvent))] CAMERA_ROTATE,
    [EventType("3D Camera Rotation", typeof(CameraCustomRotateEvent))] CAMERA_CUSTOM_ROTATE,
    [EventType("Change Camera FOV", typeof(CameraFovEvent))] CAMERA_FOV,
    [EventType("Change Camera Easing Mode", typeof(CameraEaseModeEvent))] CAMERA_EASE_MODE,
    [EventType("Change Camera Easing Duration", typeof(CameraEaseDurationEvent))] CAMERA_EASE_TIME,
    [EventType("Toggle Reduced Motion Settings", typeof(GameplayMotionEvent))] GAMEPLAY_MOTION,
    [EventType("Change Primary/Assist Character", typeof(ChangeCharacterEvent))] CHARACTER_CHANGE,
    [EventType("Change Stage", typeof(ChangeStageEvent))] STAGE_CHANGE,
    [EventType("Lock UI Position", typeof(UiLockEvent))] UI_LOCK,
    [EventType("Show/Hide UI Elements", typeof(UiShowHideEvent))] UI_TOGGLE,
    [EventType("Slide UI Elements In/Out", typeof(UiSlideEvent))] UI_SLIDE,
    [EventType("Time and Weather", typeof(WeatherEvent))] WEATHER
}

public class EventTypeSelectorBoxItem(EventType eventType)
{
    public readonly EventType EventType = eventType;
    public string Name => EventType.Name();
}

public partial class ChooseEventTypeDialog : BaseDialog<EventType?>
{
    private ObservableCollection<EventTypeSelectorBoxItem> _triggerTypeSelectors = [];
    
    public ChooseEventTypeDialog()
    {
        InitializeComponent();

        foreach (var triggerType in Enum.GetValues<EventType>())
        {
            _triggerTypeSelectors.Add(new EventTypeSelectorBoxItem(triggerType));
        }
        
        EventTypeSelectorBox.ItemsSource = _triggerTypeSelectors;
    }
    
    public async Task<Optional<EventType?>> ShowAsync()
    {
        App.DialogIsOpen = true;
        var result = await base.ShowAsync();
        App.DialogIsOpen = false;
        return result;
    }
    
    private void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(((EventTypeSelectorBoxItem?)
                  EventTypeSelectorBox.SelectionBoxItem)?.EventType);
    }
    
    private void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}