using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using UNBUGGABLE;

namespace UNBEATABLEChartEditor.Input;
internal enum CallbackType
{
    KEY_PRESS,
    KEY_RELEASE,
    MOUSE_PRESS,
    MOUSE_RELEASE,
    SCROLL
}

public static class InputManager
{
    public static Key LastPressedKey { get; private set; }
    public static bool CtrlPressed => _keyStates.GetValueOrDefault(Key.LeftCtrl) ||
                                      _keyStates.GetValueOrDefault(Key.RightCtrl);
    public static bool ShiftPressed => _keyStates.GetValueOrDefault(Key.LeftShift) ||
                                       _keyStates.GetValueOrDefault(Key.RightShift);
    public static bool AltPressed => _keyStates.GetValueOrDefault(Key.LeftAlt) ||
                                     _keyStates.GetValueOrDefault(Key.RightAlt);

    public static List<InputActionBase> Actions { get; set; } = [];
    
    // the state of every keyboard key and mouse button; used to prevent OnPress from being called
    // every frame when the key is held
    private static readonly Dictionary<Key, bool> _keyStates = new();
    private static readonly Dictionary<MouseButton, bool> _mouseButtonStates = new();
    
    public static void ResetInputStates()
    {
        _keyStates.Clear();
        _mouseButtonStates.Clear();
        ChartBuilder.ResetInputStates();
    }

    public static async Task OnKeyDown(KeyEventArgs e)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }

        e.Handled = true;
        var k = e.Key;
        
        LastPressedKey = k;
        // only call once until the key is released
        if (!_keyStates.GetValueOrDefault(k))
        {
            await RunCallbacks(CallbackType.KEY_PRESS, k);
        }
        _keyStates[k] = true;
    }

    public static async Task OnKeyUp(KeyEventArgs e)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        e.Handled = true;
        var k = e.Key;
        
        // only call once until the key is pressed (this check is probably unnecessary but i don't
        // want to have to go fix this bug for the fourth time)
        if (_keyStates.GetValueOrDefault(k))
        {
            await RunCallbacks(CallbackType.KEY_RELEASE, k);
        }
        _keyStates[k] = false;
    }

    public static async Task OnScroll(PointerWheelEventArgs e)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        e.Handled = true;
        var scrollAmount = e.Delta.Y;
        
        // the scroll wheel doesn't go in the state dictionary because it can never be "released"
        var button = scrollAmount < 0 ? MouseButton.WHEEL_UP : MouseButton.WHEEL_DOWN;
        await RunCallbacks(CallbackType.MOUSE_PRESS, button);
    }

    public static async Task OnMousePress(PointerPressedEventArgs e)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        e.Handled = true;
        var button = e.Properties.IsRightButtonPressed ? MouseButton.RIGHT :
            e.Properties.IsMiddleButtonPressed ? MouseButton.MIDDLE : MouseButton.LEFT;
        
        if (!_mouseButtonStates.GetValueOrDefault(button))
        {
            await RunCallbacks(CallbackType.MOUSE_PRESS, button);
        }
        _mouseButtonStates[button] = true;
        
        // the chart builder doesn't care if this gets called repeatedly
        await ChartBuilder.OnMousePress(e.Properties.IsRightButtonPressed);
    }

    public static async Task OnMouseRelease(PointerReleasedEventArgs e)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        e.Handled = true;
        var button = e.Properties.IsRightButtonPressed ? MouseButton.RIGHT :
            e.Properties.IsMiddleButtonPressed ? MouseButton.MIDDLE : MouseButton.LEFT;
        
        if (_mouseButtonStates.GetValueOrDefault(button))
        {
            await RunCallbacks(CallbackType.MOUSE_RELEASE, button);
        }
        _mouseButtonStates[button] = false;
        
        ChartBuilder.OnMouseRelease();
    }

    private static async Task RunCallbacks(CallbackType type, object arg)
    {
        // actions that ignore modifiers only activate if no other actions were
        InputActionBase? ignoreModifierFallback = null;
        foreach (var action in Actions)
        {
            if (Chart.Playing && !action.CanUseWhilePlaying)
            {
                continue;
            }

            if (ChartBuilder.PlacingNote && !action.CanUseWhilePlacingNotes)
            {
                continue;
            }
            
            foreach (var keybind in action.Keybinds)
            {
                if ((type is CallbackType.KEY_PRESS or CallbackType.KEY_RELEASE &&
                     (Key)arg == keybind.Key) || (
                        type is CallbackType.MOUSE_PRESS or CallbackType.MOUSE_RELEASE or
                            CallbackType.SCROLL && // scroll wheel is considered a mouse button here
                        (MouseButton)arg == keybind.MouseButton))
                {
                    if (CtrlPressed == keybind.Ctrl && ShiftPressed == keybind.Shift &&
                        AltPressed == keybind.Alt)
                    {
                        if (type is CallbackType.KEY_PRESS or CallbackType.MOUSE_PRESS or
                            CallbackType.SCROLL)
                        {
                            await action.OnPress();
                        }
                        else
                        {
                            await action.OnRelease();
                        }

                        return;
                    }
                    
                    if (action.IgnoreModifiers && ignoreModifierFallback == null)
                    {
                        ignoreModifierFallback = action;
                        break;
                    }
                }
            }
        }
        
        if (ignoreModifierFallback != null)
        {
            if (type is CallbackType.KEY_PRESS or CallbackType.MOUSE_PRESS or
                CallbackType.SCROLL)
            {
                await ignoreModifierFallback.OnPress();
            }
            else
            {
                await ignoreModifierFallback.OnRelease();
            }
        }
    }
}