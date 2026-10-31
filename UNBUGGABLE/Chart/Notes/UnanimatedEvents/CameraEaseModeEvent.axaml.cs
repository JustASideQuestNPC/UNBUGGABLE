using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

internal class EaseModeAttribute : Attribute
{
    public string Name { get; }
    public string InternalName { get; }
    
    internal EaseModeAttribute(string name, string internalName)
    {
        InternalName = internalName;
        Name = name;
    }
}

public static class EaseModes
{
    public static string Name(this EaseMode p)
    {
        return GetAttr(p)?.Name ?? Enum.GetName(p) ?? "";
    }
    
    public static string InternalName(this EaseMode p)
    {
        return GetAttr(p)?.InternalName ?? Enum.GetName(p) ?? "";
    }   
    
    private static EaseModeAttribute? GetAttr(EaseMode p)
    {
        var a = Attribute.GetCustomAttribute(ForValue(p),
                                             typeof(EaseModeAttribute));
        return (EaseModeAttribute?)a;
    }

    private static FieldInfo? ForValue(EaseMode p)
    {
        return typeof(EaseMode).GetField(Enum.GetName(typeof(EaseMode), p));
    }
}

public enum EaseMode
{
    [EaseMode("Linear", "Linear")] LINEAR,
    [EaseMode("Sine (In)", "InSine")] SINE_IN,
    [EaseMode("Sine (Out)", "OutSine")] SINE_OUT,
    [EaseMode("Sine (In/Out)", "InOutSine")] SINE_IN_OUT,
    [EaseMode("Quadratic (In)", "InQuad")] QUAD_IN,
    [EaseMode("Quadratic (Out)", "OutQuad")] QUAD_OUT,
    [EaseMode("Quadratic (In/Out)", "InOutQuad")] QUAD_IN_OUT,
    [EaseMode("Cubic (In)", "InCubic")] CUBIC_IN,
    [EaseMode("Cubic (Out)", "OutCubic")] CUBIC_OUT,
    [EaseMode("Cubic (In/Out)", "InOutCubic")] CUBIC_IN_OUT,
    [EaseMode("Quartic (In)", "InQuad")] QUART_IN,
    [EaseMode("Quartic (Out)", "OutQuad")] QUART_OUT,
    [EaseMode("Quartic (In/Out)", "InOutQuad")] QUART_IN_OUT,
    [EaseMode("Quintic (In)", "InQuint")] QUINT_IN,
    [EaseMode("Quintic (Out)", "OutQuint")] QUINT_OUT,
    [EaseMode("Quintic (In/Out)", "InOutQuint")] QUINT_IN_OUT,
    [EaseMode("Exponential (In)", "InExpo")] EXPO_IN,
    [EaseMode("Exponential (Out)", "OutExpo")] EXPO_OUT,
    [EaseMode("Exponential (In/Out)", "InOutExpo")] EXPO_IN_OUT,
    [EaseMode("Circular (In)", "InCirc")] CIRC_IN,
    [EaseMode("Circular (Out)", "OutCirc")] CIRC_OUT,
    [EaseMode("Circular (In/Out)", "InOutCirc")] CIRC_IN_OUT,
    [EaseMode("Elastic (In)", "InElastic")] ELASTIC_IN,
    [EaseMode("Elastic (Out)", "OutElastic")] ELASTIC_OUT,
    [EaseMode("Elastic (In/Out)", "InOutElastic")] ELASTIC_IN_OUT,
    [EaseMode("Back (In)", "InBack")] BACK_IN,
    [EaseMode("Back (Out)", "OutBack")] BACK_OUT,
    [EaseMode("Back (In/Out)", "InOutBack")] BACK_IN_OUT,
    [EaseMode("Bounce (In)", "InBounce")] BOUNCE_IN,
    [EaseMode("Bounce (Out)", "OutBounce")] BOUNCE_OUT,
    [EaseMode("Bounce (In/Out)", "InOutBounce")] BOUNCE_IN_OUT,
    [EaseMode("Flash", "Flash")] FLASH_NONE,
    [EaseMode("Flash (In)", "InFlash")] FLASH_IN,
    [EaseMode("Flash (Out)", "OutFlash")] FLASH_OUT,
    [EaseMode("Flash (In/Out)", "InOutFlash")] FLASH_IN_OUT,
}

public class EaseModeSelectorBoxItem(EaseMode position)
{
    public readonly EaseMode Position = position;
    public string Name => Position.Name();
}

public partial class CameraEaseModeEventDialog : UnanimatedEventBaseDialog
{
    public EaseMode NewEaseMode { get; private set; }
    private readonly ObservableCollection<EaseModeSelectorBoxItem> _easeModeSelectors = [];
    
    public CameraEaseModeEventDialog(EaseMode easeMode)
    {
        InitializeComponent();
        NewEaseMode = easeMode;

        foreach (var p in Enum.GetValues<EaseMode>())
        {
            _easeModeSelectors.Add(new EaseModeSelectorBoxItem(p));
        }

        EaseModeSelectorBox.ItemsSource = _easeModeSelectors;
        EaseModeSelectorBox.SelectedIndex = (int)easeMode;
    }
    
    private void EaseModeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e == null || e.AddedItems.Count == 0)
        {
            return;
        }

        NewEaseMode = ((EaseModeSelectorBoxItem)EaseModeSelectorBox.SelectionBoxItem).Position;
    }
}

public class CameraEaseModeEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.CAMERA_EASE_MODE;
    public EaseMode EaseMode { get; set; } = EaseMode.LINEAR;
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new CameraEaseModeEventDialog(EaseMode);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } && editDialog.NewEaseMode != EaseMode)
        {
            return new CameraEaseModeEvent
            {
                Time = Time,
                EaseMode = editDialog.NewEaseMode
            };
        }

        return null;
    }
    
    public override string ToEventString()
    {
        return $"Camera,{Time},EaseMode:{EaseMode.InternalName()}";
    }
}