using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaDialogs.Views;
using CommunityToolkit.Mvvm.Input;
using UNBUGGABLE;

namespace UNBEATABLEChartEditor.Dialogs;

public partial class NumberEntryDialog : BaseDialog<double>
{
    private readonly double _initialValue;
    
    public NumberEntryDialog(string title, double initialValue=0)
    {
        InitializeComponent();

        Title.Text = title;
        NumberBox.Value = (decimal)initialValue;
        _initialValue = initialValue;

        NumberBox.KeyBindings.Add(new KeyBinding
        {
            Command = NumberBoxEnterPressCommand,
            Gesture = new KeyGesture(Key.Enter)
        });
    }
    
    public async Task<Optional<double>> ShowAsync()
    {
        App.DialogIsOpen = true;
        var result = await base.ShowAsync();
        App.DialogIsOpen = false;
        return result;
    }
    
    [RelayCommand]
    private void NumberBoxEnterPress()
    {
        Close((double)NumberBox.Value);
    }
    
    private void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        Close((double)NumberBox.Value);
    }
    
    private void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(_initialValue);
    }
}