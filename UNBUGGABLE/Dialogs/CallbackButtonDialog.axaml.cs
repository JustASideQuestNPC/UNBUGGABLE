using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaDialogs.Views;
using UNBUGGABLE;

namespace UNBEATABLEChartEditor.Dialogs;

public partial class CallbackButtonDialog : BaseDialog
{
    private readonly Action _callback;
    
    public CallbackButtonDialog(string message, string callbackButtonName, Action callback)
    {
        InitializeComponent();
        Message.Text = message;
        CallbackButtonName.Text = callbackButtonName;
        _callback = callback;
    }
    
    public async Task ShowAsync()
    {
        App.DialogIsOpen = true;
        await base.ShowAsync();
        App.DialogIsOpen = false;
    }
    
    private void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void CallbackButtonClick(object? sender, RoutedEventArgs e)
    {
        _callback();
        Close();
    }
}