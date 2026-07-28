using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaDialogs.Views;
using UNBUGGABLE;

namespace UNBEATABLEChartEditor.Dialogs;

public partial class MessageDialog : BaseDialog
{
    public MessageDialog(string message)
    {
        InitializeComponent();
        Message.Text = message;
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
}