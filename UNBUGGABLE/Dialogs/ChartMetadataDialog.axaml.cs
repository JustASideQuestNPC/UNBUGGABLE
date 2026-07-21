using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using AvaloniaDialogs.Views;
using UNBUGGABLE;
using UNBUGGABLE.Resources;

namespace UNBEATABLEChartEditor.Dialogs;

public partial class ChartMetadataDialog : BaseDialog<Chart.MetadataContainer>
{
    private DifficultySlot _difficultySlot;
    private double _initialOffset;
    
    public ChartMetadataDialog(Chart.MetadataContainer currentMetadata)
    {
        InitializeComponent();

        _initialOffset = currentMetadata.ChartOffset;
        SongNameBox.Text = currentMetadata.SongName;
        ArtistNameBox.Text = currentMetadata.ArtistName;
        CoverArtistNameBox.Text = currentMetadata.CoverArtistName;
        CharterNameBox.Text = currentMetadata.CharterName;
        _difficultySlot = currentMetadata.DifficultySlot;
        DifficultyNameBox.Text = currentMetadata.DifficultyName;
        DifficultyLevelBox.Value = currentMetadata.DifficultyLevel;
        OffsetBox.Text = currentMetadata.ChartOffset.ToString();
        
        FlavorTextBox.Text = currentMetadata.FlavorText;
        
        DifficultySlotBox.SelectedIndex = (int)_difficultySlot;
        
        if (_difficultySlot == DifficultySlot.STAR || Config.AlwaysEnableCustomDifficultyName)
        {
            DifficultyNameBox.IsEnabled = true;
        }
        else
        {
            DifficultyNameBox.IsEnabled = false;
            DifficultyNameBox.Text = 
                _difficultySlot == DifficultySlot.UNBEATABLE ? "UNBEATABLE" :
                    CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                        _difficultySlot.ToString().ToLower());
        }
    }
    
    public async Task<Optional<Chart.MetadataContainer>> ShowAsync()
    {
        App.DialogIsOpen = true;
        var result = await base.ShowAsync();
        App.DialogIsOpen = false;
        return result;
    }

    private void DifficultySlotChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e == null || e.AddedItems.Count == 0)
        {
            return;
        }
        
        var newSelection = ((ComboBoxItem)e.AddedItems[0])!.Content;
        _difficultySlot = newSelection switch
        {
            "Beginner"   => DifficultySlot.BEGINNER,
            "Normal"     => DifficultySlot.NORMAL,
            "Hard"       => DifficultySlot.HARD,
            "Expert"     => DifficultySlot.EXPERT,
            "UNBEATABLE" => DifficultySlot.UNBEATABLE,
            "Star"       => DifficultySlot.STAR,
            _ => _difficultySlot
        };
        Console.WriteLine($"Difficulty slot changed to {newSelection}");
        if (DifficultyNameBox != null)
        {
            if (_difficultySlot == DifficultySlot.STAR || Config.AlwaysEnableCustomDifficultyName)
            {
                DifficultyNameBox.IsEnabled = true;
            }
            else
            {
                DifficultyNameBox.IsEnabled = false;
                DifficultyNameBox.Text = 
                    _difficultySlot == DifficultySlot.UNBEATABLE ? "UNBEATABLE" :
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                            _difficultySlot.ToString().ToLower());
            }
        }
    }

    private void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        var metadata = new Chart.MetadataContainer()
        {
            SongName = SongNameBox.Text,
            ArtistName = ArtistNameBox.Text,
            CoverArtistName = CoverArtistNameBox.Text,
            CharterName = CharterNameBox.Text,
            FlavorText = FlavorTextBox.Text,
            DifficultySlot = _difficultySlot,
            DifficultyName = DifficultyNameBox.Text,
            DifficultyLevel = (int)DifficultyLevelBox.Value,
            ChartOffset = double.Parse(OffsetBox.Text)
            
        };
        Close(metadata);
    }
    
    private void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OffsetBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(OffsetBox.Text) || !double.TryParse(OffsetBox.Text, out _))
        {
            OffsetBox.Text = _initialOffset.ToString();
        }
    }
}