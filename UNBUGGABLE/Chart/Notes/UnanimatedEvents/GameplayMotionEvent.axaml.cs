using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaDialogs.Views;
using UNBUGGABLE.ConfigParser;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class GameplayMotionEventDialog : BaseDialog<bool>
{
    public bool NewScreenShake => ScreenShakeToggle.IsChecked ?? false;
    public bool NewRotation => RotationToggle.IsChecked ?? false;
    public bool NewZoom => ZoomToggle.IsChecked ?? false;
    
    public GameplayMotionEventDialog(bool screenShake, bool rotation, bool zoom)
    {
        InitializeComponent();
        ScreenShakeToggle.IsChecked = screenShake;
        RotationToggle.IsChecked = rotation;
        ZoomToggle.IsChecked = zoom;
    }
    
    public async Task<Optional<bool>> ShowAsync()
    {
        App.DialogIsOpen = true;
        var result = await base.ShowAsync();
        App.DialogIsOpen = false;
        return result;
    }
    
    protected void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
    
    protected void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}

public class GameplayMotionEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.GAMEPLAY_MOTION;
    
    public bool ScreenShake { get; set; }
    public bool Rotation { get; set; }
    public bool Zoom { get; set; }
    
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new GameplayMotionEventDialog(ScreenShake, Rotation, Zoom);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } &&
            (editDialog.NewScreenShake != ScreenShake || editDialog.NewRotation != Rotation ||
             editDialog.NewZoom != Zoom))
        {
            return new GameplayMotionEvent
            {
                Time = Time,
                ScreenShake = editDialog.NewScreenShake,
                Rotation = editDialog.NewRotation,
                Zoom = editDialog.NewZoom
            };
        }

        return null;
    }

    public override string ToEventString()
    {
        return $"Gameplay,{Time + Chart.Metadata.ChartOffset},ScreenShake:{ScreenShake}\n" +
               $"Gameplay,{Time + Chart.Metadata.ChartOffset},ScreenRot:{Rotation}\n" +
               $"Gameplay,{Time + Chart.Metadata.ChartOffset},ScreenZoom:{Zoom}";
    }
}