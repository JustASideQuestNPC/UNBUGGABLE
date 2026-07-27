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
    
    private static bool _leftCtrlPressed = false;
    private static bool _rightCtrlPressed = false;
    public static bool CtrlPressed => _leftCtrlPressed || _rightCtrlPressed;
    
    private static bool _leftShiftPressed = false;
    private static bool _rightShiftPressed = false;
    public static bool ShiftPressed => _leftShiftPressed || _rightShiftPressed;
    
    private static bool _leftAltPressed = false;
    private static bool _rightAltPressed = false;
    public static bool AltPressed => _leftAltPressed || _rightAltPressed;

    public static List<InputActionBase> Actions { get; set; } = [];
    
    public static void ResetInputStates()
    {
        _leftCtrlPressed = false;
        _rightCtrlPressed = false;
        _leftShiftPressed = false;
        _rightShiftPressed = false;
        ChartBuilder.ResetInputStates();
    }

    public static async Task OnKeyDown(Key k)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        LastPressedKey = k;
        switch (k)
        {
            case Key.LeftCtrl:
                _leftCtrlPressed = true;
                break;
            case Key.RightCtrl:
                _rightCtrlPressed = true;
                break;
            case Key.LeftShift:
                _leftShiftPressed = true;
                break;
            case Key.RightShift:
                _rightShiftPressed = true;
                break;
            case Key.LeftAlt:
                _leftAltPressed = true;
                break;
            case Key.RightAlt:
                _rightAltPressed = true;
                break;
            default:
                await RunCallbacks(CallbackType.KEY_PRESS, k);
                break;
        }
    }

    public static async Task OnKeyUp(Key k)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        switch (k)
        {
            case Key.LeftCtrl:
                _leftCtrlPressed = false;
                break;
            case Key.RightCtrl:
                _rightCtrlPressed = false;
                break;
            case Key.LeftShift:
                _leftShiftPressed = false;
                break;
            case Key.RightShift:
                _rightShiftPressed = false;
                break;
            case Key.LeftAlt:
                _leftAltPressed = false;
                break;
            case Key.RightAlt:
                _rightAltPressed = false;
                break;
            default:
                await RunCallbacks(CallbackType.KEY_RELEASE, k);
                break;
        }
    }

    public static async Task OnScroll(double scrollAmount)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        var button = scrollAmount < 0 ? MouseButton.WHEEL_UP : MouseButton.WHEEL_DOWN;
        await RunCallbacks(CallbackType.MOUSE_PRESS, button);
    }

    public static async Task OnMousePress(bool isRightButton, bool isMiddleButton)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        var button = isRightButton ? MouseButton.RIGHT : isMiddleButton ? MouseButton.MIDDLE :
            MouseButton.LEFT;
        await RunCallbacks(CallbackType.MOUSE_PRESS, button);
        await ChartBuilder.OnMousePress(isRightButton);
    }

    public static async Task OnMouseRelease(bool isRightButton, bool isMiddleButton)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        var button = isRightButton ? MouseButton.RIGHT : isMiddleButton ? MouseButton.MIDDLE :
            MouseButton.LEFT;
        await RunCallbacks(CallbackType.MOUSE_RELEASE, button);
        ChartBuilder.OnMouseRelease();
    }

    private static async Task RunCallbacks(CallbackType type, object arg)
    {
        // actions that ignore modifiers only activate if no other actions were
        InputActionBase? ignoreModifierFallback = null;
        foreach (var action in Actions)
        {
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