using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaDialogs.Views;
using UNBUGGABLE.ConfigParser;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public partial class UnanimatedEventBaseDialog : BaseDialog<bool>
{
    public UnanimatedEventBaseDialog()
    {
        InitializeComponent();
    }
    
    public async Task<Optional<bool>> ShowAsync()
    {
        App.DialogIsOpen = true;
        var result = await base.ShowAsync();
        App.DialogIsOpen = false;
        return result;
    }
    
    protected void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
    
    protected void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}

public abstract class UnanimatedEventBase : CameraChange
{
    public override NoteType Type => NoteType.UNANIMATED_EVENT;
    
    public abstract EventType EventType { get; }
    
    private static readonly List<Point> Vertices =
    [
        new(-33.412, -10.920),
        new(-20.500, -5.973),
        new(-20.500, -14.000),
        new(20.500, -14.000),
        new(20.500, 14.000),
        new(-20.500, 14.000),
        new(-20.500, 5.973),
        new(-33.412, 10.920)
    ];
    
    private static SolidColorBrush _fillBrush;
    private static SolidColorBrush _outlineBrush;
    private static double _outlineThickness;
    private static SolidColorBrush _tailFillBrush;
    private static SolidColorBrush _tailOutlineBrush;
    private static double _tailOutlineThickness;
    
    private static SolidColorBrush _selectedOutlineBrush;
    private static SolidColorBrush _selectedFillBrush;
    private static double _selectedOutlineThickness;
    private static SolidColorBrush _selectedTailFillBrush;
    private static SolidColorBrush _selectedTailOutlineBrush;
    private static double _selectedTailOutlineThickness;
    
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    private readonly Geometry _shape = new PolylineGeometry(Vertices, true);

    public static void UpdateStyles()
    {
        _fillBrush = (SolidColorBrush)App.Current.Resources["Notes.UnanimatedEvent.FillColor"];
        _outlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.UnanimatedEvent.OutlineColor"];
        // thickness is always the same on all sides
        _outlineThickness =
            ((Thickness)App.Current.Resources["Notes.UnanimatedEvent.OutlineThickness"]).Top;
        _tailFillBrush =
            (SolidColorBrush)App.Current.Resources["Notes.UnanimatedEvent.TailColor"];
        _tailOutlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.UnanimatedEvent.TailOutlineColor"];
        _tailOutlineThickness =
            ((Thickness)App.Current.Resources["Notes.UnanimatedEvent.TailOutlineThickness"]).Top;
        
        _selectedFillBrush =
            (SolidColorBrush)App.Current.Resources["Notes.UnanimatedEvent.Selected.FillColor"];
        _selectedOutlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.UnanimatedEvent.Selected.OutlineColor"];
        _selectedOutlineThickness =
            ((Thickness)App.Current.Resources["Notes.UnanimatedEvent.Selected.OutlineThickness"])
            .Top;
        _selectedTailFillBrush =
            (SolidColorBrush)App.Current.Resources["Notes.UnanimatedEvent.Selected.TailColor"];
        _selectedTailOutlineBrush =
            (SolidColorBrush)App.Current
                                .Resources["Notes.UnanimatedEvent.Selected.TailOutlineColor"];
        _selectedTailOutlineThickness =
            ((Thickness)App.Current
                           .Resources["Notes.UnanimatedEvent.Selected.TailOutlineThickness"]).Top;
    }

    public static bool ParseEventStrings(string[] lines, ref int i, ref List<string> errors)
    {
        var hadError = false;
        List<UnanimatedEventBase> events = [];
        for (; i < lines.Length; ++i)
        {
            var str = lines[i].Trim();
            if (string.IsNullOrEmpty(str))
            {
                break;
            }
            
            var chunks = str.Split(',').Select(s => s.Trim()).ToArray();
            if (chunks.Length < 2 || !long.TryParse(chunks[1], out var startTime))
            {
                errors.Add($"Error parsing event \"{str}\" at line {i + 1}");
                hadError = true;
                continue;
            }
            
            var eventParams = chunks.Length > 2 ? chunks[^1].Split(':') : [];
            UnanimatedEventBase? eventNote = null;
            
            switch (chunks[0])
            {
                case "Camera":
                    eventNote = ParseCameraEventString(chunks, startTime);
                    break;
                case "Visuals":
                    eventNote = ParseVisualEventString(chunks, startTime);
                    break;
                case "Character":
                    if (chunks.Length != 3 || eventParams is not ["SetCharacter", _, _])
                    {
                        break;
                    }

                    Character? primaryCharacter = null;
                    Character? assistCharacter = null;
                    foreach (var c in Enum.GetValues<Character>())
                    {
                        if (c.Name() == eventParams[1])
                        {
                            primaryCharacter = c;
                        }

                        if (c.Name() == eventParams[2])
                        {
                            assistCharacter = c;
                        }
                    }

                    if (primaryCharacter == null || assistCharacter == null)
                    {
                        break;
                    }

                    eventNote = new ChangeCharacterEvent
                    {
                        Time = startTime,
                        PrimaryCharacter = primaryCharacter.Value,
                        AssistCharacter = assistCharacter.Value
                    };
                    break;
                case "StageScene":
                    if (chunks.Length != 3)
                    {
                        break;
                    }

                    Stage? stage = null;
                    foreach (var s in Enum.GetValues<Stage>())
                    {
                        if (s.InternalName() == chunks[2])
                        {
                            stage = s;
                        }
                    }

                    if (stage == null)
                    {
                        break;
                    }

                    eventNote = new ChangeStageEvent
                    {
                        Time = startTime,
                        Stage = stage.Value
                    };
                    break;
                case "Gameplay":
                    // unanimated requires each gameplay setting to be on its own line, so do some
                    // iteration to merge everything into a single note
                    var screenShake = false;
                    var rotation = false;
                    var zoom = false;
                    for (; i < lines.Length; ++i)
                    {
                        if (lines[i].StartsWith("Gameplay"))
                        {
                            eventParams = chunks.Length > 2 ? chunks[^1].Split(':') : [];
                            if (eventParams is ["ScreenShake", _] &&
                                (bool.TryParse(eventParams[1], out _) || eventParams[1] == "0" ||
                                 eventParams[1] == "1"))
                            {
                                screenShake = eventParams[1].ToLower() == "true" ||
                                              eventParams[1] == "1";
                            }
                            else if (eventParams is ["ScreenRot", _] &&
                                     (bool.TryParse(eventParams[1], out _) || eventParams[1] == "0" ||
                                      eventParams[1] == "1"))
                            {
                                rotation = eventParams[1].ToLower() == "true" ||
                                           eventParams[1] == "1";
                            }
                            else if (eventParams is ["ScreenZoom", _] &&
                                     (bool.TryParse(eventParams[1], out _) || eventParams[1] == "0" ||
                                      eventParams[1] == "1"))
                            {
                                zoom = eventParams[1].ToLower() == "true" || eventParams[1] == "1";
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    eventNote = new GameplayMotionEvent
                    {
                        Time = startTime,
                        ScreenShake = screenShake,
                        Rotation = rotation,
                        Zoom = zoom
                    };

                    break;
                case "UI":
                    long endTime;
                    if (long.TryParse(chunks[2], out var time))
                    {
                        endTime = time;
                    }
                    else
                    {
                        endTime = startTime;
                    }
                    
                    if (eventParams.Length != 2)
                    {
                        break;
                    }
                    var affectedElements = eventParams[1].Split('|');

                    if (eventParams[0] is "Show" or "Hide")
                    {
                        if (affectedElements.Any(e => !UiShowHideEvent.UiElementNames.Contains(e)))
                        {
                            break;
                        }

                        // invert the ui elements back to which ones we want to turn on
                        if (eventParams[0] == "Hide")
                        {
                            affectedElements =
                                UiShowHideEvent.UiElementNames
                                               .Where(e => !affectedElements.Contains(e))
                                               .ToArray();
                        }

                        eventNote = new UiShowHideEvent
                        {
                            Time = startTime,
                            EndTime = endTime,
                            EnabledElements = affectedElements.ToList()
                        };
                        
                        // unanimated doesn't allow showing and hiding ui elements on the same line,
                        // so unbuggable does it across two lines and we need to skip the second
                        if (i + 1 < lines.Length && lines[i + 1].StartsWith("UI"))
                        {
                            var nextEventParams = lines[i + 1].Split(',').Select(s => s.Trim())
                                                              .ToArray();
                            if (nextEventParams is ["UI", _] &&
                                (nextEventParams[1] == "Show" || nextEventParams[1] == "Hide"))
                            {
                                ++i;
                            }
                        }
                    }
                    else if (eventParams[0] is "SlideIn" or "SlideOut")
                    {
                        if (affectedElements.Any(e => !UiSlideEvent.UiElementNames.Contains(e)))
                        {
                            break;
                        }

                        // invert the ui elements back to which ones we want to turn on
                        if (eventParams[0] == "Hide")
                        {
                            affectedElements =
                                UiSlideEvent.UiElementNames
                                            .Where(e => !affectedElements.Contains(e))
                                            .ToArray();
                        }

                        eventNote = new UiSlideEvent
                        {
                            Time = startTime,
                            EndTime = endTime,
                            SlideInElements = affectedElements.ToList()
                        };
                        
                        // unanimated doesn't allow showing and hiding ui elements on the same line,
                        // so unbuggable does it across two lines and we need to skip the second
                        if (i + 1 < lines.Length && lines[i + 1].StartsWith("UI"))
                        {
                            var nextEventParams = lines[i + 1].Split(',').Select(s => s.Trim())
                                                              .ToArray();
                            if (nextEventParams is ["UI", _] &&
                                (nextEventParams[1] == "SlideIn" ||
                                 nextEventParams[1] == "SlideOut"))
                            {
                                ++i;
                            }
                        }
                    }
                    break;
            }
            
            if (eventNote == null)
            {
                errors.Add($"Error parsing event \"{str}\" at line {i + 1}");
                hadError = true;
            }
            else
            {
                events.Add(eventNote);
            }
        }

        if (hadError)
        {
            return false;
        }

        foreach (var e in events)
        {
            Chart.AddNote(e);
        }

        return true;
    }

    private static UnanimatedEventBase? ParseVisualEventString(string[] chunks, long startTime)
    {
        long endTime;
        if (long.TryParse(chunks[2], out var time))
        {
            endTime = time;
        }
        else
        {
            endTime = startTime;
        }
        
        var i = endTime == startTime ? 2 : 3;
        var eventParams = chunks[i + 1].Split(':');
        
        if (chunks[2] == "TimeAndWeather")
        {
            if (eventParams.Length != 2)
            {
                return null;
            }
            
            Weather? weather = null;
            foreach (var w in Enum.GetValues<Weather>())
            {
                if (w.InternalName() == eventParams[1])
                {
                    weather = w;
                }
            }

            if (weather == null)
            {
                return null;
            }

            return new WeatherEvent
            {
                Time = startTime,
                Weather = weather.Value
            };
        }

        return null;
    }

    private static UnanimatedEventBase? ParseCameraEventString(string[] chunks, long startTime)
    {
        long endTime;
        if (long.TryParse(chunks[2], out var time))
        {
            endTime = time;
        }
        else
        {
            endTime = startTime;
        }
        
        // find the correct camera note type
        var i = endTime == startTime ? 2 : 3;
        var eventParams = chunks[i].Split(':');
        if (chunks[i].StartsWith("CameraTarget"))
        {
            PresetPosition? position = null;
            foreach (var p in Enum.GetValues<PresetPosition>())
            {
                if (p.InternalName() == eventParams[1])
                {
                    position = p;
                }
            }

            if (position == null)
            {
                return null;
            }

            return new CameraPresetPositionEvent
            {
                Time = startTime,
                EndTime = endTime,
                Position = (PresetPosition)position
            };
        }

        if (chunks[i].StartsWith("HorizontalTarget") || chunks[i].StartsWith("HorizontalOffset") ||
            chunks[i].StartsWith("RotTarget") || chunks[i].StartsWith("RotOffset") ||
            chunks[i].StartsWith("ZoomTarget") || chunks[i].StartsWith("ZoomOffset") ||
            chunks[i].StartsWith("FovTarget") || chunks[i].StartsWith("FovOffset"))
        {
            if (eventParams.Length < 2 || !double.TryParse(eventParams[1], out var amount) ||
                eventParams.Length > 3 || (eventParams.Length == 3 && eventParams[2] != "Reverse"))
            {
                return null;
            }
            
            if (eventParams is [_, _, "Reverse"])
            {
                amount *= -1;
            }

            UnanimatedEventBase eventNote = eventParams[0] switch
            {
                "HorizontalTarget" or "HorizontalOffset" => new CameraHorizontalEvent
                {
                    Time = startTime,
                    EndTime = endTime,
                    Distance = amount,
                    Offset = eventParams[0] == "HorizontalOffset"
                },
                "RotTarget" or "RotOffset" => new CameraRotateEvent
                {
                    Time = startTime,
                    EndTime = endTime,
                    Angle = amount,
                    Offset = eventParams[0] == "RotOffset"
                },
                "ZoomTarget" or "ZoomOffset" => new CameraZoomEvent
                {
                    Time = startTime,
                    EndTime = endTime,
                    Zoom = amount,
                    Offset = eventParams[0] == "ZoomOffset"
                },
                _ => new CameraFovEvent
                {
                    Time = startTime,
                    EndTime = endTime,
                    Fov = amount,
                    Offset = eventParams[0] == "HorizontalOffset"
                }
            };

            return eventNote;
        }

        if (chunks[i] == "CustomCameraTarget" ||
            chunks[i] == "CustomCameraOffset" ||
            chunks[i] == "CustomRotTarget" ||
            chunks[i] == "CustomRotOffset")
        {
            if (chunks.Length < i + 1)
            {
                return null;
            }
            
            
            eventParams = chunks[i + 1].Split(':');
            // param format: x:y:z[:Reverse,:Instant,:StartPoint:x:y:z]
            if (eventParams.Length < 3 || !double.TryParse(eventParams[0], out var x) ||
                !double.TryParse(eventParams[1], out var y) ||
                !double.TryParse(eventParams[2], out var z))
            {
                return null;
            }
            
            double? startX = null, startY = null, startZ = null;
            if (eventParams.Length > 7 && eventParams.Contains("StartPoint"))
            {
                var si = Array.FindIndex(eventParams, s => s == "StartPoint");
                if (si == -1 || si + 3 >= eventParams.Length)
                {
                    return null;
                }

                if (double.TryParse(eventParams[si + 1], out var _x) &&
                    double.TryParse(eventParams[si + 2], out var _y) &&
                    double.TryParse(eventParams[si + 3], out var _z))
                {
                    startX = _x;
                    startY = _y;
                    startZ = _z;
                }
                else
                {
                    return null;
                }
            }

            if (eventParams.Contains("Reverse") && startX != null && startY != null &&
                startZ != null)
            {
                (x, startX) = ((double)startX, x);
                (y, startY) = ((double)startY, y);
                (z, startZ) = ((double)startZ, z);
            }

            UnanimatedEventBase eventNote = eventParams[0] switch
            {
                "CustomCameraTarget" or "CustomCameraOffset" => new CameraCustomPositionEvent
                {
                    Time = startTime,
                    EndTime = endTime,
                    Position = new Vector3D(x, y, z),
                    UseStart = eventParams[0] == "StartPoint",
                    StartPosition = new Vector3D(startX ?? x, startY ?? y, startZ ?? z),
                    Instant = eventParams.Contains("Instant")
                },
                _ => new CameraCustomRotateEvent
                {
                    Time = startTime,
                    EndTime = endTime,
                    AngleVector = new Vector3D(x, y, z),
                    UseStart = eventParams[0] == "StartPoint",
                    StartAngleVector = new Vector3D(startX ?? x, startY ?? y, startZ ?? z),
                    Instant = eventParams.Contains("Instant")
                }
            };

            return eventNote;
        }

        if (chunks[i].StartsWith("EaseMode"))
        {
            if (eventParams.Length < 2)
            {
                return null;
            }
            
            EaseMode? easeMode = null;
            foreach (var e in Enum.GetValues<EaseMode>())
            {
                if (e.InternalName() == eventParams[1])
                {
                    easeMode = e;
                }
            }

            if (easeMode == null)
            {
                return null;
            }

            return new CameraEaseModeEvent
            {
                Time = startTime,
                EaseMode = (EaseMode)easeMode
            };
        }

        if (chunks[i].StartsWith("EaseTime"))
        {
            if (eventParams.Length < 2 || !long.TryParse(chunks[1], out var duration))
            {
                return null;
            }

            return new CameraEaseDurationEvent
            {
                Time = startTime,
                Duration = duration
            };
        }

        if (chunks[i] == "Reset")
        {
            return new CameraResetAllEvent
            {
                Time = startTime,
                EndTime = endTime
            };
        }

        return null;
    }
    
    public override void Render(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var startY = NoteViewer.TimeToScreenCoords(Time);
        var endY = NoteViewer.TimeToScreenCoords(EndTime);

        if ((startY < -50 && endY < -50) ||
            (startY > NoteViewer.ViewerHeight + 50 && endY > NoteViewer.ViewerHeight + 50))
        {
            return;
        }

        if (endY > startY)
        {
            var tailPen = selected ?
                new Pen(_selectedTailOutlineBrush, _selectedTailOutlineThickness) :
                new Pen(_tailOutlineBrush, _tailOutlineThickness);
            dc.DrawRectangle(selected ? _selectedTailFillBrush : _tailFillBrush, tailPen,
                new Rect(x - 8, startY, 16, endY - startY));
        }

        var shape = _shape.Clone();
        shape.Transform = new TranslateTransform(x, startY);
        
        var pen = selected ?
            new Pen(_selectedOutlineBrush, _selectedOutlineThickness) :
            new Pen(_outlineBrush, _outlineThickness);
        dc.DrawGeometry(selected ? _selectedFillBrush : _fillBrush, pen, shape);
        
        RenderFlags(dc, x, startY);
        
        RenderDebugTime(dc, x, startY, endY);
    }
    
    public override void RenderPreview(DrawingContext dc) { }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time > rangeStart && Time <= rangeEnd && Config.Settings.HitSounds.UnanimatedEvent)
        {
            return (long)(Time - rangeStart);
        }

        return null;
    }

    public override string ToHitObjectString(bool isFirstNote, bool isStandardFile) => "";

    public abstract Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false);

    public abstract string ToEventString();
}