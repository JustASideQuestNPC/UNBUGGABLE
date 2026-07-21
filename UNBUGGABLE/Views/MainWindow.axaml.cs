using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

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
        await ChartBuilder.OnKeyDown(e.Key);
    }
    
    private void OnKeyUp(TopLevel sender, KeyEventArgs e)
    {
        ChartBuilder.OnKeyUp(e.Key);
    }
    
    private void OnPointerWheelChanged(TopLevel sender, PointerWheelEventArgs e)
    {
        if (e.Delta.X == 0)
        {
            ChartBuilder.OnScroll(e.Delta.Y);
        }
    }

    private void OnNoteViewerPointerMove(object? sender, PointerEventArgs e)
    {
        ChartBuilder.MousePosition = e.GetPosition((Border)sender);
    }

    private void OnNoteViewerPointerPress(object? sender, PointerPressedEventArgs e)
    {
        ChartBuilder.OnMousePress(e.Properties.IsRightButtonPressed);
    }

    private void OnNoteViewerPointerRelease(object? sender, PointerReleasedEventArgs e)
    {
        ChartBuilder.OnMouseRelease();
    }
}