using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaDialogs.Views;
using UNBUGGABLE.ConfigParser;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class UiLockEventDialog : BaseDialog<bool>
{
    public bool NewLockUi => LockUiToggle.IsChecked ?? false;
    
    public UiLockEventDialog(bool lockUi)
    {
        InitializeComponent();
        LockUiToggle.IsChecked = lockUi;
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

public class UiLockEvent : UnanimatedEventBase
{
    public override EventType EventType => EventType.UI_LOCK;
    
    public bool LockUi { get; set; }
        
    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new UiLockEventDialog(LockUi);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } && editDialog.NewLockUi != LockUi)
        {
            return new UiLockEvent
            {
                Time = Time,
                LockUi = editDialog.NewLockUi
            };
        }

        return null;
    }

    public override string ToEventString()
    {
        return $"UI,{Time + Chart.Metadata.ChartOffset},ForceLockedUI:{LockUi}";
    }
}