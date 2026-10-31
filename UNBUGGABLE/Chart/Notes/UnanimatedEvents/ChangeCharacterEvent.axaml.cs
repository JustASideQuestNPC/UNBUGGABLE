using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

internal class CharacterAttribute : Attribute
{
    public string Name { get; }
    public bool RequiresDlc { get; }
    
    internal CharacterAttribute(string name, bool requiresDlc = false)
    {
        Name = name;
        RequiresDlc = requiresDlc;
    }
}

public static class Characters
{
    public static string Name(this Character c)
    {
        return GetAttr(c)?.Name ?? Enum.GetName(c) ?? "";
    }
    
    public static bool RequiresDlc(this Character c)
    {
        return GetAttr(c)?.RequiresDlc ?? false;
    }
    
    private static CharacterAttribute? GetAttr(Character c)
    {
        var a = Attribute.GetCustomAttribute(ForValue(c),
                                             typeof(CharacterAttribute));
        return (CharacterAttribute?)a;
    }

    private static FieldInfo? ForValue(Character c)
    {
        return typeof(Character).GetField(Enum.GetName(typeof(Character), c));
    }
}

public enum Character
{
    [Character("")] NONE,
    [Character("Beat")] BEAT,
    [Character("Beat (Hoodie)")] BEAT_HOODIE,
    [Character("Beat (Guitar)")] BEAT_GUITAR,
    [Character("Beat (Up)")] BEAT_UP,
    [Character("Beat (Nothing)")] BEAT_NOTHING,
    [Character("Clef")] CLEF,
    [Character("Quaver")] QUAVER,
    [Character("Quaver (Acoustic)")] QUAVER_ACOUSTIC,
    [Character("Quaver (CQC)")] QUAVER_CQC,
    [Character("Treble")] TREBLE,
    [Character("Rest")] REST,
    [Character("Rest (OMF)")] REST_OMF,
    [Character("Eve")] EVE,
    [Character("Grace")] GRACE,
    [Character("Crest", true)] CREST,
    [Character("Crest (Maid)", true)] CREST_MAID,
    [Character("DC", true)] DC,
    [Character("Poco", true)] POCO,
    [Character("Apoco", true)] APOCO,
    [Character("Penny", true)] PENNY,
    [Character("Sforzando", true)] SFORZANDO,
    [Character("JamieP", true)] JAMIE_P,
    [Character("Quaver (Shrimp)", true)] QUAVER_SHRIMP
}

public class CharacterSelectorBoxItem(Character character)
{
    public readonly Character Character = character;
    public string Name => Character.Name();
}

public partial class ChangeCharacterEventDialog : UnanimatedEventBaseDialog
{
    public Character NewPrimary { get; private set; }
    public Character NewAssist { get; private set; }
    
    private ObservableCollection<CharacterSelectorBoxItem> _primarySelectors = [];
    private ObservableCollection<CharacterSelectorBoxItem> _assistSelectors = [];
    
    public ChangeCharacterEventDialog(Character primary, Character assist)
    {
        InitializeComponent();

        foreach (var c in Enum.GetValues<Character>())
        {
            // none is a dummy character used when the event is created
            if (c == Character.NONE)
            {
                continue;
            }
            
            _primarySelectors.Add(new CharacterSelectorBoxItem(c));
            _assistSelectors.Add(new CharacterSelectorBoxItem(c));
        }
        
        PrimarySelectorBox.ItemsSource = _primarySelectors;
        PrimarySelectorBox.SelectedIndex = Math.Min((int)primary - 1, 0);
        PrimaryRequiresDlcAlertIcon.IsVisible = primary.RequiresDlc();
        
        AssistSelectorBox.ItemsSource = _assistSelectors;
        AssistSelectorBox.SelectedIndex = Math.Min((int)assist - 1, 0);
        AssistRequiresDlcAlertIcon.IsVisible = assist.RequiresDlc();
    }

    private void PrimarySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e == null || e.AddedItems.Count == 0 || PrimarySelectorBox.SelectionBoxItem == null)
        {
            return;
        }

        NewPrimary = ((CharacterSelectorBoxItem)PrimarySelectorBox.SelectionBoxItem).Character;
        PrimaryRequiresDlcAlertIcon.IsVisible = NewPrimary.RequiresDlc();
    }
    
    private void AssistSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e == null || e.AddedItems.Count == 0 || AssistSelectorBox.SelectionBoxItem == null)
        {
            return;
        }

        NewAssist = ((CharacterSelectorBoxItem)AssistSelectorBox.SelectionBoxItem).Character;
        AssistRequiresDlcAlertIcon.IsVisible = NewAssist.RequiresDlc();
    }
}

public class ChangeCharacterEvent : UnanimatedEventBase
{
    public Character PrimaryCharacter { get; set; } = Character.NONE;
    public Character AssistCharacter { get; set; } = Character.NONE;

    public override EventType EventType => EventType.CHARACTER_CHANGE;

    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool _ = false)
    {
        var editDialog = new ChangeCharacterEventDialog(PrimaryCharacter, AssistCharacter);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewPrimary != PrimaryCharacter ||
             editDialog.NewAssist != AssistCharacter))
        {
            return new ChangeCharacterEvent
            {
                Time = Time,
                PrimaryCharacter = editDialog.NewPrimary,
                AssistCharacter = editDialog.NewAssist
            };
        }

        return null;
    }
    
    public override string ToEventString()
    {
        return $"Character,{Time + Chart.Metadata.ChartOffset}," +
               $"SetCharacter:{PrimaryCharacter.Name()}:{AssistCharacter.Name()}";
    }
}