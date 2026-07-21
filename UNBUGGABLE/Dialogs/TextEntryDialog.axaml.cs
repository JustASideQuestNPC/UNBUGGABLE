using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Remote.Protocol.Input;
using AvaloniaDialogs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UNBUGGABLE;
using Key = Avalonia.Input.Key;

namespace UNBEATABLEChartEditor.Dialogs;

/// <summary>
/// A dialog with a single text box.
/// </summary>
public partial class TextEntryDialog : BaseDialog<string>
{
    private readonly string _initialText;
    
    public TextEntryDialog(string title, string initialText="")
    {
        InitializeComponent();

        Title.Text = title;
        TextBox.Text = initialText;
        _initialText = initialText;

        TextBox.KeyBindings.Add(new KeyBinding
        {
            Command = TextBoxEnterPressCommand,
            Gesture = new KeyGesture(Key.Enter)
        });
    }
    
    public async Task<Optional<string>> ShowAsync()
    {
        App.DialogIsOpen = true;
        var result = await base.ShowAsync();
        App.DialogIsOpen = false;
        return result;
    }
    
    [RelayCommand]
    private void TextBoxEnterPress()
    {
        Close(TextBox.Text ?? _initialText);
    }
    
    private void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(TextBox.Text ?? _initialText);
    }
    
    private void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(_initialText);
    }
}