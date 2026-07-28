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

            if ((Config.Settings.NegativeMashConversion && Note.Type == NoteType.FREESTYLE &&
                 Note.Flags.F) || (Note.Type == NoteType.MASH && Note.EndTime < Note.Time))
            {
                return "Negative Mash";
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
                NoteType.CAMERA_SWAP => "Camera Swap",
                NoteType.CAMERA_WIDE => "Camera Zoom In/Out",
                NoteType.CAMERA_INSTANT => "Instant Camera Swap",
                NoteType.COP_SINGLE => $"{laneName} Cop Single",
                NoteType.COP_HOLD => $"{laneName} Cop Hold",
                _ => $"{laneName} Cop Mash" // markers will never be in the list
            };
        }
    }
};

public partial class MainWindowViewModel : ViewModelBase
{
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
    [ObservableProperty] private bool _songLoaded = false;
    [ObservableProperty] private bool _editorUiEnabled = false;
    [ObservableProperty] private bool _placementPriorityListEnabled = false;
    [ObservableProperty] private Border _eventIndicator = new();
    
    [ObservableProperty]
    private ObservableCollection<PlacementPriorityListEntry> _activePriorityListEntries = [];

    private bool _updatingPriorityList = false;
    private List<(NoteBase, int)> _initialNoteOrder = [];
    
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
            // Trace.WriteLine("frame");
            App.MainWindow.NoteViewer.InvalidateVisual();
            App.MainWindow.GamePreview.InvalidateVisual();
            App.MainWindow.DebugOverlay.InvalidateVisual();
            if (Chart.SongLoaded)
            {
                var songTimeText = TimeSpan.FromMilliseconds(Chart.CurrentTime)
                                            .ToString(@"mm\:ss\.fff");
                SongTimeText = Chart.CurrentTime < 0 ? $"-{songTimeText}" : songTimeText;
                
                var chartTimeText = TimeSpan.FromMilliseconds(
                                                Chart.CurrentTime + Chart.Metadata.ChartOffset)
                                            .ToString(@"mm\:ss\.fff");
                ChartTimeText = Chart.CurrentTime + Chart.Metadata.ChartOffset < 0 ?
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
        Trace.WriteLine($"started frame timer: runs every {frameTimer.Interval}");

        var tickTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1 / Config.Settings.HitSoundTickRate)
        };
        tickTimer.Tick += (sender, args) => Chart.PerTickUpdate();
        tickTimer.Start();
        Trace.WriteLine($"started tick timer: runs every {tickTimer.Interval}");

        if (Config.Settings.AutosaveInterval > 0)
        {
            var autosaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(Config.Settings.AutosaveInterval)
            };
            autosaveTimer.Tick += async (sender, args) => await Chart.TryAutosave();
            autosaveTimer.Start();
            Trace.WriteLine($"started autosave timer: runs every {autosaveTimer.Interval}");
        }
        else
        {
            Trace.WriteLine("autosaves are disabled");
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
            Trace.WriteLine("skipping priority list reorder event");
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
        
        Trace.WriteLine($"Lane order: {string.Join(",", orderedLaneNames)}");
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
            Trace.WriteLine("No top level window!");
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
            Trace.WriteLine($"Loading from {path}");
            
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
                DifficultyText = $"{Chart.Metadata.DifficultyName} " +
                                 $"{Chart.Metadata.DifficultyLevel}";
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
            return;
        }

        if (Chart.ChartFileName == "")
        {
            await SaveNewBeatFile();
        }
        else
        {
            var fullPath = Path.Combine(Path.Combine(Config.CustomSongsDirectory,
                                                     Chart.ChartFolderName),
                                        $"{Chart.ChartFileName}.beat.txt");
            Trace.WriteLine($"Saving to {fullPath}");
            await ChartBuilder.SaveToBeatPath(fullPath);
            ShowEventIndicator($"Saved to {Chart.ChartFileName}.beat.txt");
        }
    }
    
    [RelayCommand]
    private async Task SaveNewBeatFile()
    {
        if (!CanSave)
        {
            return;
        }
        
        if (App.TopLevel == null)
        {
            Trace.WriteLine("No top level window!");
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
            Trace.WriteLine($"Saving to {fullPath}");
            await ChartBuilder.SaveToBeatPath(fullPath);
            ShowEventIndicator($"Saved to {Chart.ChartFileName}.beat.txt");
        }
    }
    
    [RelayCommand]
    private async Task SaveStandardFile()
    {
        if (!CanSave)
        {
            return;
        }

        if (Chart.ChartFileName == "")
        {
            await SaveNewStandardFile();
        }
        else
        {
            var fullPath = Path.Combine(Config.CustomSongsDirectory,
                                        $"{Chart.ChartFileName}.txt");
            Trace.WriteLine($"Saving to {fullPath}");
            await ChartBuilder.SaveToStandardPath(fullPath);
            ShowEventIndicator($"Saved to {Chart.ChartFileName}.txt");
        }
    }
    
    [RelayCommand]
    private async Task SaveNewStandardFile()
    {
        if (!CanSave)
        {
            return;
        }
        
        if (App.TopLevel == null)
        {
            Trace.WriteLine("No top level window!");
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
            Trace.WriteLine($"Saving to {fullPath}");
            await ChartBuilder.SaveToStandardPath(fullPath);
            ShowEventIndicator($"Saved to {Chart.ChartFileName}.txt");
        }
    }

    [RelayCommand]
    private async Task EditChartMetadata()
    {
        var result = await new ChartMetadataDialog(Chart.Metadata).ShowAsync();
        if (result.HasValue)
        {
            Trace.WriteLine("Chart metadata updated.");
            Chart.Metadata = result.Value;
            SongNameText = Chart.Metadata.SongName;
            ArtistNameText = Chart.Metadata.ArtistName;
            DifficultyText = $"{Chart.Metadata.DifficultyName} " +
                             $"{Chart.Metadata.DifficultyLevel}";
        }
    }

    [RelayCommand]
    private void ReloadConfig()
    {
        Config.LoadConfig();
        Config.LoadKeybinds();
        
        NoteViewer.UpdateNoteColumnPositions();
        Chart.RebuildSnapLineSets();
        ShowEventIndicator("Reloaded config.");
    }

    [RelayCommand]
    private void ResetPlaySpeed()
    {
        PlaySpeed = 100;
        App.MainWindow.PlaySpeedSlider.Value = 100;
    }
}