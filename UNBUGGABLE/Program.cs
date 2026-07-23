using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Transactions;
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
        Trace.Listeners.Clear();

        var filePath = Path.Combine(Environment.CurrentDirectory,
                                    $"logs/log_{DateTime.Now:MM_dd_yyyy_h_mm_tt}.log");
        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory("logs");
            File.Create(filePath);
        }
        
        var fileListener = new TextWriterTraceListener(filePath);
        fileListener.Name = "TextLogger";
        fileListener.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;

        var consoleListener = new ConsoleTraceListener(false);
        consoleListener.TraceOutputOptions = TraceOptions.DateTime;

        Trace.Listeners.Add(fileListener);
        Trace.Listeners.Add(consoleListener);
        Trace.AutoFlush = true;
        
        Trace.WriteLine($"logging to {filePath}");

        try
        {
            // apparently, running the app by double-clicking a file will make the working directory the
            // same place as that file, not the location of the exe
            if (!Environment.CurrentDirectory.EndsWith("UNBUGGABLE"))
            {
                Environment.CurrentDirectory =
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                    Environment.CurrentDirectory;
            }

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Trace.WriteLine($"FATAL ERROR!!!\n{e}");
        }
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