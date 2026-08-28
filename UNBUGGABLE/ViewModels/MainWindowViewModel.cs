using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaDialogs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Dialogs;
using UNBUGGABLE.Commands;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.ViewModels;

public class PlacementPriorityListEntry : ViewModelBase
{
    public NoteBase? Note { get; set; }

    public string DisplayName
    {
        get
        {
            if (Note is null)
            {
                return "";
            }
            
            var laneName = Note.Lane switch
            {
                NoteLane.TOP => "Top",
                NoteLane.CENTER => "Middle",
                NoteLane.BOTTOM => "Bottom",
                _ => "Camera" // markers will never be in the list
            };

            return Note.Type switch
            {
                NoteType.SINGLE => $"{laneName} Single",
                NoteType.HOLD => $"{laneName} Hold",
                NoteType.SPIKE => $"{laneName} Spike",
                NoteType.DOUBLE => $"{laneName} Double",
                NoteType.FREESTYLE => "Freestyle",
                NoteType.MASH => "Mash",
                NoteType.NEGATIVE_MASH => "Negative Mash",
                NoteType.CAMERA_SWAP => "Camera Swap",
                NoteType.CAMERA_ZOOM => "Camera Zoom In/Out",
                NoteType.CAMERA_INSTANT => "Instant Camera Swap",
                NoteType.CAMERA_SWAP_AND_ZOOM => "Camera Swap and Zoom",
                NoteType.COP_SINGLE => $"{laneName} Cop Single",
                NoteType.COP_HOLD => $"{laneName} Cop Hold",
                _ => $"{laneName} Cop Mash" // markers will never be in the list
            };
        }
    }
}

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    [ObservableProperty] private string? _chartLengthText = "";
    [ObservableProperty] private string? _songTimeText = "";
    [ObservableProperty] private string? _chartTimeText = "";
    [ObservableProperty] private string? _breakpointTimeText = "n/a";
    [ObservableProperty] private string? _songBpmText = "";
    [ObservableProperty] private string? _songNameText = "";
    [ObservableProperty] private string? _artistNameText = "";
    [ObservableProperty] private string? _difficultyText = "";
    [ObservableProperty] private string? _currentNoteTypeText = "notes";
    [ObservableProperty] private string? _currentZoomText = "1.0";
    [ObservableProperty] private string? _saveButtonToolTip = "";
    [ObservableProperty] private string? _cop1State = "";
    [ObservableProperty] private string? _cop2State = "";
    [ObservableProperty] private string? _cop3State = "";
    [ObservableProperty] private string? _cop4State = "";
    [ObservableProperty] private string? _lockedFlagsText = "";
    [ObservableProperty] private bool _songLoaded = false;
    [ObservableProperty] private bool _editorUiEnabled = false;
    [ObservableProperty] private bool _placementPriorityListEnabled = false;
    [ObservableProperty] private int _sliderIncrement = 5;
    [ObservableProperty] private Border _eventIndicator = new();
    
    [ObservableProperty]
    private ObservableCollection<PlacementPriorityListEntry> _activePriorityListEntries = [];
    
    public int SongVolume
    {
        get => Chart.SongVolume;
        set => Chart.SongVolume = value;
    }
    
    public int SfxVolume
    {
        get => Chart.SfxVolume;
        set => Chart.SfxVolume = value;
    }
    
    public int PlaySpeed
    {
        get => Chart.PlaySpeed;
        set => Chart.PlaySpeed = value;
    }

    private bool _canSave = false;
    public bool CanSave
    {
        get => _canSave;
        set
        {
            _canSave = value;
            SaveButtonToolTip = value ? "Save" : "Cannot save until metadata is set";
        }
    }

    private bool _updatingPriorityList = false;
    private List<(NoteBase, int)> _initialNoteOrder = [];

    // used to disable the "reloaded config" event indicator when the app starts
    private bool _firstConfigReload = true;

    public MainWindowViewModel()
    {
        var frameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1 / 60.0)
        };
        frameTimer.Tick += (sender, e) =>
        {
            // most of the ui only needs to change when the song time changes, but LibVLC only
            // update MediaPlayer.Position every tenth of a second or so, so it's easier to just
            // force an update every frame
            App.MainWindow.NoteViewer.InvalidateVisual();
            App.MainWindow.GamePreview.InvalidateVisual();
            App.MainWindow.DebugOverlay.InvalidateVisual();
            if (Chart.SongLoaded)
            {
                var songTimeText = TimeSpan.FromMilliseconds(Chart.CurrentTimeRaw)
                                            .ToString(@"mm\:ss\.fff");
                SongTimeText = Chart.CurrentTimeRaw < 0 ? $"-{songTimeText}" : songTimeText;
                
                var chartTimeText = TimeSpan.FromMilliseconds(
                                                Chart.CurrentTimeRaw + Chart.Metadata.ChartOffset)
                                            .ToString(@"mm\:ss\.fff");
                ChartTimeText = Chart.CurrentTimeRaw + Chart.Metadata.ChartOffset < 0 ?
                    $"-{chartTimeText}" : chartTimeText;
                ChartLengthText = TimeSpan.FromMilliseconds(Chart.Length).ToString(@"mm\:ss\.fff");
                Cop1State = GamePreview.Cop1State switch
                {
                    CopState.LEFT => "Left",
                    CopState.RIGHT => "Right",
                    _ => "Dead"
                };
                Cop2State = GamePreview.Cop2State switch
                {
                    CopState.LEFT => "Left",
                    CopState.RIGHT => "Right",
                    _ => "Dead"
                };
                Cop3State = GamePreview.Cop3State switch
                {
                    CopState.LEFT => "Left",
                    CopState.RIGHT => "Right",
                    _ => "Dead"
                };
                Cop4State = GamePreview.Cop4State switch
                {
                    CopState.LEFT => "Left",
                    CopState.RIGHT => "Right",
                    _ => "Dead"
                };
            }
        };
        frameTimer.Start();
        Logger.Info("started frame timer: runs every {0}", frameTimer.Interval);

        var tickTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0 / Config.Settings.HitSoundTickRate)
        };
        tickTimer.Tick += (sender, args) => Chart.PerTickUpdate();
        tickTimer.Start();
        Logger.Info("started tick timer: runs every {0}", tickTimer.Interval);

        if (Config.Settings.AutosaveInterval > 0)
        {
            var autosaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(Config.Settings.AutosaveInterval)
            };
            autosaveTimer.Tick += async (sender, args) => await Chart.TryAutosave();
            autosaveTimer.Start();
            Logger.Info("started autosave timer: runs every {0}", autosaveTimer.Interval);
        }
        else
        {
            Logger.Info("autosaves are disabled");
        }

        ActivePriorityListEntries = [];
        ActivePriorityListEntries.CollectionChanged += OnPriorityListReorder;
    }

    public void ShowEventIndicator(string text)
    {
        EventIndicator = new Border
        {
            Classes = { "EventIndicator" },
            Child = new TextBlock { Text = text }
        };
    }

    public void UpdatePriorityListEntries() =>
        UpdatePriorityListEntries(Chart.GetNotesAtCurrentTime());
    public void UpdatePriorityListEntries(List<(NoteBase, int)> notes)
    {
        _updatingPriorityList = true;
        ActivePriorityListEntries.Clear();
        if (notes.Count == 0)
        {
            return;
        }

        _initialNoteOrder = notes.ToList();
        foreach (var (note, _) in notes)
        {
            ActivePriorityListEntries.Add(new PlacementPriorityListEntry
            {
                Note = note
            });
        }

        PlacementPriorityListEnabled = (ActivePriorityListEntries.Count > 1);
        _updatingPriorityList = false;
    }
    
    public void ClearPriorityListEntries()
    {
        ActivePriorityListEntries.Clear();
    }

    private bool _forceClose = false;
    public async Task OnWindowClosed(object? sender, WindowClosingEventArgs e)
    {
        if (Chart.SongLoaded && Chart.UnsavedChanges && !_forceClose)
        {
            e.Cancel = true;
            if (!App.DialogIsOpen)
            {
                var dialog = new ThreefoldDialog
                {
                    Message =
                        "Do you want to save the current chart? Unsaved changes will be lost.",
                    PositiveText = "Save",
                    NegativeText = "Discard",
                    NeutralText = "Cancel",
                };
                var result = await dialog.ShowAsync();
                if (result == ThreefoldDialog.ButtonType.Positive)
                {
                    if (Config.Settings.DefaultSaveToBeatFiles)
                    {
                        await SaveBeatFile();
                    }
                    else
                    {
                        await SaveStandardFile();
                    }
                }
                
                if (result != ThreefoldDialog.ButtonType.Neutral)
                {
                    _forceClose = true;
                    e.Cancel = false;
                    App.MainWindow.Close();
                }
            }
        }
    }

    // private bool _skipEvent = false;
    private void OnPriorityListReorder(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_updatingPriorityList)
        {
            return;
        }

        // reordering the list fires 2 events for some reason? and which one needs to skipped isn't
        // consistent?? why???
        if (ActivePriorityListEntries.Count != _initialNoteOrder.Count)
        {
            return;
        }
        
        Console.WriteLine("Reordered priority list");
        ChartBuilderCommandInvoker.Execute(
            new ReorderNotesCommand(_initialNoteOrder,
                                    ActivePriorityListEntries.Select(x => x.Note!).ToList()));
        
        List<string> orderedLaneNames = [];
        foreach (var entry in ActivePriorityListEntries)
        {
            orderedLaneNames.Add(entry.Note!.Lane.ToString());
        }
    }
    
    [RelayCommand]
    private void OpenConfigFolder()
    {
        if (App.TopLevel == null)
        {
            Console.WriteLine("No top level window!");
            return;
        }

        var startFolder = Directory.CreateDirectory(Environment.CurrentDirectory + "/configs/");
        Process.Start("explorer.exe", startFolder.FullName);
    }
    
    [RelayCommand]
    private void OpenThemesFolder()
    {
        if (App.TopLevel == null)
        {
            Console.WriteLine("No top level window!");
            return;
        }

        var startFolder = Directory.CreateDirectory(Environment.CurrentDirectory + "/themes/");
        Process.Start("explorer.exe", startFolder.FullName);
    }
    
    [RelayCommand]
    private async Task LoadFile()
    {
        if (Chart.SongLoaded && Chart.UnsavedChanges)
        {
            if (!App.DialogIsOpen)
            {
                var dialog = new ThreefoldDialog
                {
                    Message =
                        "Do you want to save the current chart? Unsaved changes will be lost.",
                    PositiveText = "Save",
                    NegativeText = "Discard",
                    NeutralText = "Cancel",
                };
                var result = await dialog.ShowAsync();
                
                if (result == ThreefoldDialog.ButtonType.Neutral)
                {
                    return;
                }
                
                if (result == ThreefoldDialog.ButtonType.Positive)
                {
                    if (Config.Settings.DefaultSaveToBeatFiles)
                    {
                        await SaveBeatFile();
                    }
                    else
                    {
                        await SaveStandardFile();
                    }
                }
            }
        }
        
        if (App.TopLevel == null)
        {
            Logger.Error("No top level window!");
            return;
        }
        
        var customSongsFolder = await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
            new Uri(Config.CustomSongsDirectory));
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose chart to import",
            SuggestedStartLocation = customSongsFolder,
            FileTypeFilter = [new FilePickerFileType("Audio or UNBEATABLE Chart")
            {
                Patterns =
                [
                    "*.txt",
                    "*.osu",
                    "*.auto",
                    "*.mp3",
                    "*.wav"
                ]
            }]
        });

        if (files.Count > 0)
        {
            var path = files[0].Path;
            
            bool loaded;
            if (path.AbsoluteUri.EndsWith(".mp3") || path.AbsoluteUri.EndsWith(".wav"))
            {
                loaded = await ChartBuilder.TryCreateChartFromAudio(path.LocalPath);
            }
            else
            {
                loaded = await ChartBuilder.TryLoadChartFile(path.LocalPath);
            }
            
            if (loaded)
            {
                SongNameText = Chart.Metadata.SongName;
                ArtistNameText = Chart.Metadata.ArtistName;
                var difficultySlotName = Chart.Metadata.DifficultySlot switch
                {
                    DifficultySlot.BEGINNER => "Beginner",
                    DifficultySlot.NORMAL => "Normal",
                    DifficultySlot.HARD => "Hard",
                    DifficultySlot.EXPERT => "Expert",
                    DifficultySlot.UNBEATABLE => "UNBEATABLE",
                    _ => "Star"
                };
                DifficultyText = $"{difficultySlotName} {Chart.Metadata.DifficultyLevel}";
                App.MainWindow.BeatSnapText.Text = Chart.BeatSnap.ToString();
            }
        }
    }

    [RelayCommand]
    private async Task DefaultSave()
    {
        if (Config.Settings.DefaultSaveToBeatFiles)
        {
            await SaveBeatFile();
        }
        else
        {
            await SaveStandardFile();
        }
    }

    [RelayCommand]
    private async Task SaveBeatFile()
    {
        if (!CanSave)
        {
            ShowEventIndicator("Cannot save until metadata is set");
        }

        var fullPath = Path.Combine(Path.Combine(Config.CustomSongsDirectory,
                                                 Chart.ChartFolderName),
                                    $"{Chart.ChartFileName}.beat.txt");
        if (Chart.ChartFileName == "" || !File.Exists(fullPath))
        {
            await SaveNewBeatFile();
        }
        else
        {
            await ChartBuilder.SaveToBeatPath(fullPath);
            ShowEventIndicator($"Saved to {Chart.ChartFileName}.beat.txt");
        }
    }
    
    [RelayCommand]
    private async Task SaveNewBeatFile()
    {
        if (!CanSave)
        {
            ShowEventIndicator("Cannot save until metadata is set");
        }
        
        if (App.TopLevel == null)
        {
            Logger.Error("No top level window!");
            return;
        }
        
        var customSongsFolder = await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
            new Uri(Config.CustomSongsDirectory));
        var file = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save sheet as...",
            DefaultExtension = ".beat.txt",
            SuggestedFileName = Chart.ChartFileName == "" ? "" : Chart.ChartFileName + ".beat.txt",
            SuggestedStartLocation = customSongsFolder
        });

        if (file != null)
        {
            var fullPath = file.Path.LocalPath;
            await ChartBuilder.SaveToBeatPath(fullPath);
            ShowEventIndicator($"Saved to {Chart.ChartFileName}.beat.txt");
        }
    }
    
    [RelayCommand]
    private async Task SaveStandardFile()
    {
        if (!CanSave)
        {
            ShowEventIndicator("Cannot save until metadata is set");
        }

        var fullPath = Path.Combine(Config.CustomSongsDirectory,
                                    $"{Chart.ChartFileName}.txt");
        if (Chart.ChartFileName == "" || !File.Exists(fullPath))
        {
            await SaveNewStandardFile();
        }
        else
        {
            var success = await ChartBuilder.SaveToStandardPath(fullPath);
            ShowEventIndicator(success ? $"Saved to {Chart.ChartFileName}.txt" : "Failed to save");
        }
    }
    
    [RelayCommand]
    private async Task SaveNewStandardFile()
    {
        if (!CanSave)
        {
            ShowEventIndicator("Cannot save until metadata is set");
        }
        
        if (App.TopLevel == null)
        {
            Logger.Error("No top level window!");
            return;
        }
        
        var customSongsFolder = await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
            new Uri(Config.CustomSongsDirectory));
        var file = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save sheet as...",
            DefaultExtension = ".txt",
            SuggestedFileName = Chart.ChartFileName == "" ? "" : Chart.ChartFileName + ".txt",
            SuggestedStartLocation = customSongsFolder
        });

        if (file != null)
        {
            var fullPath = file.Path.LocalPath;
            var success = await ChartBuilder.SaveToStandardPath(fullPath);
            ShowEventIndicator(success ? $"Saved to {Chart.ChartFileName}.txt" : "Failed to save");
        }
    }

    [RelayCommand]
    private async Task EditChartMetadata()
    {
        var result = await new ChartMetadataDialog(Chart.Metadata).ShowAsync();
        if (result.HasValue)
        {
            Chart.Metadata = result.Value;
            SongNameText = Chart.Metadata.SongName;
            ArtistNameText = Chart.Metadata.ArtistName;
            var difficultySlotName = Chart.Metadata.DifficultySlot switch
            {
                DifficultySlot.BEGINNER => "Beginner",
                DifficultySlot.NORMAL => "Normal",
                DifficultySlot.HARD => "Hard",
                DifficultySlot.EXPERT => "Expert",
                DifficultySlot.UNBEATABLE => "UNBEATABLE",
                _ => "Star"
            };
            DifficultyText = $"{difficultySlotName} {Chart.Metadata.DifficultyLevel}";
        }
    }

    [RelayCommand]
    private async Task TryReloadConfig()
    {
        Config.TryReloadAllConfigs();
        if (Config.LoadError)
        {
            var dialog = new MessageDialog("One or more errors occured while loading config " +
                                           "files. Check the log file for a detailed error list.");
            await dialog.ShowAsync();
        }
        else
        {
            NoteViewer.UpdateNoteColumnPositions();
            if (Chart.SongLoaded)
            {
                Chart.RebuildSnapLineSets();
            }
            
            // the reload config command gets executed as soon as the app starts because it's the
            // easiest way to get an error message popup without crashing everything, so we skip the
            // event indicator on the first go-around
            if (!_firstConfigReload)
            {
                ShowEventIndicator("Reloaded config");
            }
        }
        
        _firstConfigReload = false;
    }

    [RelayCommand]
    private void ResetPlaySpeed()
    {
        PlaySpeed = 100;
        App.MainWindow.PlaySpeedSlider.Value = 100;
    }
}