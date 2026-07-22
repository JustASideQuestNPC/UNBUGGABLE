using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Audio;
using UNBUGGABLE.Resources;
using UNBUGGABLE.ViewModels;
using UNBUGGABLE.Views;

namespace UNBUGGABLE;

public partial class App : Application
{
    public static TopLevel? TopLevel =>
        Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ?
            desktop.MainWindow : null;
    
    public static MainWindow MainWindow { get; private set; }
    public static MainWindowViewModel MainWindowViewModel =>
        MainWindow.DataContext as MainWindowViewModel;

    private static bool _dialogIsOpen = false;
    public static bool DialogIsOpen
    {
        get => _dialogIsOpen;
        set
        {
            _dialogIsOpen = value;
            ChartBuilder.ResetKeyStates();
        }
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Applies a color theme.
    /// </summary>
    public void ApplyColorTheme(Dictionary<string, Color> colorTheme)
    {
        foreach (var (brushName, brushColor) in colorTheme)
        {
            if (Resources[brushName] is not SolidColorBrush brush)
            {
                Resources[brushName] = brush = new SolidColorBrush(brushColor);
            }
            brush.Color = brushColor;
            Console.WriteLine($"Applied {brushColor} to {brushName}");
        }
        
        // There's a bug in avalonia that makes parts of the app theme impossible to override for
        // combo boxes (and some other controls). Normally I'd have to completely re-template those
        // controls, but I'm not actually using the app theme so it's safe to just hack it
        // Resources["ThemeBackgroundBrush"] = new SolidColorBrush(
        //     colorTheme["WindowBackgroundSecondary"]);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // load configs and apply UI settings
        SfxEngine.Init();
        UserData.LoadData();
        Config.LoadFiles(Resources);
        ApplyColorTheme(Config.CurrentTheme);
        Chart.Init();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            desktop.MainWindow = MainWindow;
        }

        base.OnFrameworkInitializationCompleted();

        ChartBuilder.TryAutoLoadChartFile();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}