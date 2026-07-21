using Avalonia;
using System;
using System.Linq;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Audio;

namespace UNBUGGABLE;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .UsePlatformDetect()
                     .WithInterFont()
                     .LogToTrace();
    
    private static void OnProcessExit(object? sender, EventArgs e)
    {
        UserData.SaveData();
        SfxEngine.DisposeInstances();
    }
}