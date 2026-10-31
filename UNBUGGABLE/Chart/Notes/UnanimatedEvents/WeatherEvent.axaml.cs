using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

internal class WeatherAttribute : Attribute
{
    public string Name { get; }
    public string InternalName { get; }
    
    internal WeatherAttribute(string name, string internalName)
    {
        InternalName = internalName;
        Name = name;
    }
}

public static class Weathers
{
    public static string Name(this Weather p)
    {
        return GetAttr(p)?.Name ?? Enum.GetName(p) ?? "";
    }
    
    public static string InternalName(this Weather p)
    {
        return GetAttr(p)?.InternalName ?? Enum.GetName(p) ?? "";
    }   
    
    private static WeatherAttribute? GetAttr(Weather p)
    {
        var a = Attribute.GetCustomAttribute(ForValue(p),
                                             typeof(WeatherAttribute));
        return (WeatherAttribute?)a;
    }

    private static FieldInfo? ForValue(Weather p)
    {
        return typeof(Weather).GetField(Enum.GetName(typeof(Weather), p));
    }
}

public enum Weather
{
    [Weather("Morning Clear", "Morning_Clear")] MORNING_CLEAR,
    [Weather("Morning Foggy", "Morning_Foggy")] MORNING_FOGGY,
    [Weather("Midday Clear", "Midday_Clear")] MIDDAY_CLEAR,
    [Weather("Afternoon Clear", "Afternoon_Clear")] AFTERNOON_CLEAR,
    [Weather("Sunset Clear", "Sunset_Clear")] SUNSET_CLEAR,
    [Weather("Evening Clear", "Evening_Clear")] EVENING_CLEAR,
    [Weather("Evening Foggy", "Evening_Foggy")] EVENING_FOGGY,
    [Weather("Reset", "Reset")] RESET
}

public class WeatherSelectorBoxItem(Weather position)
{
    public readonly Weather Position = position;
    public string Name => Position.Name();
}

public partial class WeatherEventDialog : UnanimatedEventBaseDialog
{
    public Weather NewWeather { get; private set; }
    private readonly ObservableCollection<WeatherSelectorBoxItem> _WeatherSelectors = [];
    
    public WeatherEventDialog(Weather weather)
    {
        InitializeComponent();
        NewWeather = weather;

        foreach (var p in Enum.GetValues<Weather>())
        {
            _WeatherSelectors.Add(new WeatherSelectorBoxItem(p));
        }

        WeatherSelectorBox.ItemsSource = _WeatherSelectors;
        WeatherSelectorBox.SelectedIndex = (int)weather;
    }
    
    private void WeatherSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e == null || e.AddedItems.Count == 0)
        {
            return;
        }

        NewWeather = ((WeatherSelectorBoxItem)WeatherSelectorBox.SelectionBoxItem).Position;
    }
}

public class WeatherEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.WEATHER;
    public Weather Weather { get; set; } = Weather.RESET;
    
    // camera resets have nothing to edit, so this always returns null after initialization to
    // prevent empty commands from being added to the undo stack
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new WeatherEventDialog(Weather);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } && editDialog.NewWeather != Weather)
        {
            return new WeatherEvent
            {
                Time = Time,
                Weather = editDialog.NewWeather
            };
        }

        return null;
    }
    
    public override string ToEventString()
    {
        return $"Camera,{Time},Weather:{Weather.InternalName()}";
    }
}