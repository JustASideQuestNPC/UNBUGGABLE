using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using UNBEATABLEChartEditor.Input;

namespace UNBUGGABLE.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown);
        KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp);
        PointerWheelChangedEvent.AddClassHandler<TopLevel>(OnPointerWheelChanged);
    }

    private async void OnKeyDown(TopLevel sender, KeyEventArgs e)
    {
        await InputManager.OnKeyDown(e.Key);
    }
    
    private async void OnKeyUp(TopLevel sender, KeyEventArgs e)
    {
        await InputManager.OnKeyUp(e.Key);
    }
    
    private async void OnPointerWheelChanged(TopLevel sender, PointerWheelEventArgs e)
    {
        if (e.Delta.X == 0)
        {
            await InputManager.OnScroll(e.Delta.Y);
        }
    }

    private void OnNoteViewerPointerMove(object? sender, PointerEventArgs e)
    {
        ChartBuilder.MousePosition = e.GetPosition((Border)sender);
    }

    private async void OnNoteViewerPointerPress(object? sender, PointerPressedEventArgs e)
    {
        await InputManager.OnMousePress(e.Properties.IsRightButtonPressed,
                                        e.Properties.IsMiddleButtonPressed);
    }

    private async void OnNoteViewerPointerRelease(object? sender, PointerReleasedEventArgs e)
    {
        await InputManager.OnMouseRelease(e.Properties.IsRightButtonPressed,
                                          e.Properties.IsMiddleButtonPressed);
    }
}