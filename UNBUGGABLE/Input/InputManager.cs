using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using UNBUGGABLE;

namespace UNBEATABLEChartEditor.Input;

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
                _leftCtrlPressed = true;
                break;
            case Key.RightShift:
                _rightCtrlPressed = true;
                break;
            case Key.LeftAlt:
                _leftCtrlPressed = true;
                break;
            case Key.RightAlt:
                _rightCtrlPressed = true;
                break;
            default:
                await DoPressCallbacks(k);
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
                _leftCtrlPressed = false;
                break;
            case Key.RightShift:
                _rightCtrlPressed = false;
                break;
            case Key.LeftAlt:
                _leftCtrlPressed = false;
                break;
            case Key.RightAlt:
                _rightCtrlPressed = false;
                break;
            default:
                await DoReleaseCallbacks(k);
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
        await DoPressCallbacks(button);
    }

    public static async Task OnMousePress(bool isRightButton, bool isMiddleButton)
    {
        if (!Chart.SongLoaded || App.DialogIsOpen)
        {
            return;
        }
        
        var button = isRightButton ? MouseButton.RIGHT : isMiddleButton ? MouseButton.MIDDLE :
            MouseButton.LEFT;
        await DoPressCallbacks(button);
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
        await DoReleaseCallbacks(button);
        ChartBuilder.OnMouseRelease();
    }

    private static async Task DoPressCallbacks(Key k)
    {
        foreach (var action in Actions)
        {
            foreach (var keybind in action.Keybinds)
            {
                if (CtrlPressed == keybind.Ctrl && ShiftPressed == keybind.Shift &&
                    AltPressed == keybind.Alt && k == keybind.Key)
                {
                    await action.OnPress();
                    break;
                }
            }
        }
    }
    private static async Task DoPressCallbacks(MouseButton b)
    {
        foreach (var action in Actions)
        {
            foreach (var keybind in action.Keybinds)
            {
                if (CtrlPressed == keybind.Ctrl && ShiftPressed == keybind.Shift &&
                    AltPressed == keybind.Alt && b == keybind.MouseButton)
                {
                    await action.OnPress();
                    break;
                }
            }
        }
    }
    
    private static async Task DoReleaseCallbacks(Key k)
    {
        foreach (var action in Actions)
        {
            foreach (var keybind in action.Keybinds)
            {
                if (CtrlPressed == keybind.Ctrl && ShiftPressed == keybind.Shift &&
                    AltPressed == keybind.Alt && k == keybind.Key)
                {
                    await action.OnRelease();
                    break;
                }
            }
        }
    }
    private static async Task DoReleaseCallbacks(MouseButton b)
    {
        foreach (var action in Actions)
        {
            foreach (var keybind in action.Keybinds)
            {
                if (CtrlPressed == keybind.Ctrl && ShiftPressed == keybind.Shift &&
                    AltPressed == keybind.Alt && b == keybind.MouseButton)
                {
                    await action.OnRelease();
                    break;
                }
            }
        }
    }
}