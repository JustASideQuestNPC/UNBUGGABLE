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
using UNBEATABLEChartEditor.Input;
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
            InputManager.ResetInputStates();
        }
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // load configs and apply UI settings
        ThemeManager.Init(Resources);
        
        // settings are loaded pretty much as soon as the program starts
        Config.TryLoadKeybinds();
        Config.TryLoadColorThemes();
        Config.ApplyCurrentTheme();
        
        SfxEngine.Init(Config.Settings.MaxConcurrentSfx);
        UserData.LoadData();
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
            
            // "reload" the config again to give an error message after
            MainWindow.Loaded += (sender, e) =>
            {
                MainWindowViewModel.TryReloadConfigCommand.Execute(null);
                // this doesn't *need* to be done after the main window loads, but doing it here
                // ensures that the logs for loading the file will come after the logs for loading
                // the config settings
                ChartBuilder.TryAutoLoadChartFile();
            };
            MainWindow.Closing += (sender, e) => MainWindowViewModel.OnWindowClosed(sender, e);
        }

        base.OnFrameworkInitializationCompleted();
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