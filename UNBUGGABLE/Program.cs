using Avalonia;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
        // apparently, running the app by double-clicking a file will make the working directory the
        // same place as that file, not the location of the exe
        if (!Environment.CurrentDirectory.EndsWith("UNBUGGABLE"))
        {
            Environment.CurrentDirectory =
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                Environment.CurrentDirectory;
        }
        
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