using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;

namespace UNBEATABLEChartEditor.Input;

// i know avalonia has a MouseButton enum, but i also need scroll wheel inputs to be part of it
public enum MouseButton
{
    LEFT,
    RIGHT,
    MIDDLE,
    WHEEL_UP,
    WHEEL_DOWN,
}

public class Keybind
{
    public bool Ctrl;
    public bool Shift;
    public bool Alt;
    public Key? Key;
    public MouseButton? MouseButton;
}

public abstract class InputActionBase
{
    public readonly List<Keybind> Keybinds = [];
    
    /// <summary>
    /// If true, the action can activate regardless of what modifier keys are pressed, unless an
    /// action with those specific modifiers exists. For example, the default keybind for moving
    /// back in the song is [up] and ignores modifiers. The default keybind for moving a
    /// selection back in the song is [shift+up]. Moving back in the song will activate if any
    /// combination of modifier keys <i>except</i> [shift], because the keybind for moving a
    /// selection overrides it.
    /// </summary>
    public virtual bool IgnoreModifiers => false;
    
    /// <summary>
    /// If true, the action can activate whether the song is playing.
    /// </summary>
    public virtual bool CanUseWhilePlaying => false;

    public virtual bool CanUseWhilePlacingNotes => true;
    
    public InputActionBase(List<string> keybindStrings)
    {
        foreach (var keybindString in keybindStrings)
        {
            Keybinds.Add(GetKeybind(keybindString));
        }
    }

    public virtual async Task OnPress() {}
    public virtual async Task OnRelease() {}

    protected static Keybind GetKeybind(string keybindString)
    {
        var split = keybindString.Split('+').ToList();
        var keybind = new Keybind();
        
        // parse modifiers
        if (split.Count > 1)
        {
            for (var i = 0; i < split.Count - 1; ++i)
            {
                switch (split[i].ToLower())
                {
                    case "ctrl":
                        keybind.Ctrl = true;
                        break;
                    case "shift":
                        keybind.Shift = true;
                        break;
                    case "alt":
                        keybind.Alt = true;
                        break;
                }
            }
        }

        var foundPrimaryKey = false;
        foreach (var enumValue in Enum.GetValues(typeof(Key)))
        {
            // convert the first character to uppercase to match the avalonia enum
            if (enumValue.ToString() == char.ToUpper(split[^1][0]) + split[^1][1..])
            {
                keybind.Key = (Key)enumValue;
                foundPrimaryKey = true;
                break;
            }
        }

        if (!foundPrimaryKey)
        {
            keybind.MouseButton = split[^1] switch
            {
                "leftMouse" => MouseButton.LEFT,
                "rightMouse" => MouseButton.RIGHT,
                "middleMouse" => MouseButton.MIDDLE,
                "scrollUp" => MouseButton.WHEEL_UP,
                "scrollDown" => MouseButton.WHEEL_DOWN,
                // this *should* be unreachable
                _ => throw new ArgumentOutOfRangeException(nameof(split), split, null)
            };
        }
        
        return keybind;
    }
}