using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Logging;
using CommandLine;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Audio;
using UNBUGGABLE.Resources;

namespace UNBUGGABLE;

sealed class Program
{
    private class Options
    {
        [Option('c', "config", Required = false, HelpText = "Alternate path to a config file.")]
        public string ConfigPath { get; set; } = "";
        [Option('k', "keybinds", Required = false, HelpText = "Alternate path to a keybinds file.")]
        public string KeybindsPath { get; set; } = "";
        [Option('v', "verbose", Required = false,
                HelpText = "Enable verbose logging. Overrides the config file setting.")]
        public bool VerboseLogging { get; set; } = false;
    }
    
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var verboseLogging = false;
        
        Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
        {
            if (o.ConfigPath != "")
            {
                Config.ConfigFilePath = Path.Combine(Environment.CurrentDirectory, o.ConfigPath);
            }
            
            if (o.KeybindsPath != "")
            {
                Config.KeybindFilePath = Path.Combine(Environment.CurrentDirectory, o.KeybindsPath);
            }
            
            verboseLogging = o.VerboseLogging;
        });
        
        Config.CheckConfigFileUpdate();
        
        // load settings now because i need to know if verbose logging is enabled
        Config.TryLoadSettings();

        if (Config.Settings.DebugToggles.VerboseLogging)
        {
            verboseLogging = true;
        }
        
        var logConfig = new NLog.Config.LoggingConfiguration();

        const string logMessageLayout = "[${time} | ${level:uppercase=true} | ${logger}] " +
                                        "${message:withexception=true}";
        
        var logFile = new NLog.Targets.FileTarget("logfile")
        {
            Layout = logMessageLayout,
            FileName = Path.Combine(Environment.CurrentDirectory,
                                    $"logs/log_{DateTime.Now:MM_dd_yyyy_h_mm_tt}.log"),
            MaxArchiveFiles = 25,
        };
        var logConsole = new NLog.Targets.ColoredConsoleTarget("console")
        {
            Layout = logMessageLayout
        };

        if (verboseLogging)
        {
            logConfig.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logFile);
            logConfig.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logConsole);
        }
        else
        {
            logConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logFile);
            logConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logConsole);
        }
        NLog.LogManager.Configuration = logConfig;

        var appLogger = NLog.LogManager.GetLogger("Application");
        appLogger.Info("Output from UNBUGGABLE version {0}",
                       Assembly.GetExecutingAssembly().GetName().Version);
        
        Logger.Info($"Logging to {logFile.FileName}");

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            appLogger.Error(e.ExceptionObject as Exception, "Unhandled application exception");
            
            // ensure logs are written before application dies
            NLog.LogManager.Flush();
            NLog.LogManager.Shutdown();
            
            // add an underscore so the file is at the top of the log folder
            var newFilePath = Path.Combine(Environment.CurrentDirectory,
                                           $"logs/_FATAL_{DateTime.Now:MM_dd_yyyy_h_mm_tt}.log");
            File.Move(logFile.FileName.ToString(), newFilePath);
        };
        
        TaskScheduler.UnobservedTaskException += (s, e) => {
            appLogger.Error(e.Exception, "Unobserved task exception");
            
            NLog.LogManager.Flush();
            NLog.LogManager.Shutdown();
            
            var newFilePath = Path.Combine(Environment.CurrentDirectory,
                                           $"logs/_FATAL_{DateTime.Now:MM_dd_yyyy_h_mm_tt}.log");
            File.Move(logFile.FileName.ToString(), newFilePath);
        };

        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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