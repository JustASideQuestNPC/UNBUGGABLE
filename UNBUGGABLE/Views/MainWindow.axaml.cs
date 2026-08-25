using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using DialogHostAvalonia;
using UNBEATABLEChartEditor.Dialogs;
using UNBEATABLEChartEditor.Input;
using UNBUGGABLE.Resources;

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
        await InputManager.OnKeyDown(e);
    }
    
    private async void OnKeyUp(TopLevel sender, KeyEventArgs e)
    {
        await InputManager.OnKeyUp(e);
    }
    
    private async void OnPointerWheelChanged(TopLevel sender, PointerWheelEventArgs e)
    {
        if (e.Delta.X == 0)
        {
            await InputManager.OnScroll(e);
        }
    }

    private void OnNoteViewerPointerMove(object? sender, PointerEventArgs e)
    {
        ChartBuilder.MousePosition = e.GetPosition((Border)sender);
    }

    private async void OnNoteViewerPointerPress(object? sender, PointerPressedEventArgs e)
    {
        await InputManager.OnMousePress(e);
    }

    private async void OnNoteViewerPointerRelease(object? sender, PointerReleasedEventArgs e)
    {
        await InputManager.OnMouseRelease(e);
    }

    private void OnWindowLoseFocus(object? sender, RoutedEventArgs e)
    {
        InputManager.ResetInputStates();
    }
}