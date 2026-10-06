using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaDialogs.Views;
using UNBUGGABLE.UnanimatedTriggers;

namespace UNBEATABLEChartEditor.Chart.Notes.UnanimatedTriggers;

public class CharacterSelectorBoxItem(Character character) : ComboBoxItem
{
    public readonly Character Character = character;
    public new object Content => Character.Name();
}

public partial class ChangeCharacterTriggerDialog : BaseDialog
{
    private ChangeCharacterTrigger _trigger;
    private Character _currentPrimaryCharacter;
    private Character _currentAssistCharacter;
    private Character _newPrimaryCharacter;
    private Character _newAssistCharacter;
    
    public ChangeCharacterTriggerDialog()
    {
        InitializeComponent();

        foreach (var c in Enum.GetValues(typeof(Character)))
        {
            PrimaryCharacterBox.Items.Add(new CharacterSelectorBoxItem((Character) c));
            AssistCharacterBox.Items.Add(new CharacterSelectorBoxItem((Character) c));
        }
    }
    
    private void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

class CharacterAttribute : Attribute
{
    public string Name { get; private set; }
    public bool RequiresDlc { get; private set; }
    
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

    private static FieldInfo? ForValue(Character p)
    {
        return typeof(Character).GetField(Enum.GetName(typeof(Character), p));
    }
}

public enum Character
{
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

public class ChangeCharacterTrigger : UnanimatedTriggerBase
{
    public Character PrimaryCharacter { get; set; } = Character.BEAT;
    public Character AssistCharacter { get; set; } = Character.BEAT;
    
    public override string ToEventString()
    {
        return $"Character,{Time},SetCharacter:{PrimaryCharacter.Name()},{AssistCharacter.Name()}";
    }
}