using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaDialogs.Views;
using UNBUGGABLE;
using UNBUGGABLE.ConfigParser;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.Notes.UnanimatedTriggers;

public partial class UnanimatedTriggerBaseDialog : BaseDialog
{
    public UnanimatedTriggerBaseDialog()
    {
        InitializeComponent();
    }
    
    private void ConfirmButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void DeleteButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

/// <summary>
/// All available trigger types. Camera triggers with both offset and target modes are merged into
/// a single enum value.
/// </summary>
public enum TriggerType
{
    CAMERA_RESET_ALL,
    CAMERA_POSITION,
    CAMERA_HORIZONTAL,
    CAMERA_CUSTOM_POSITION,
    CAMERA_ZOOM,
    CAMERA_ROTATE,
    CAMERA_CUSTOM_ROTATE,
    CAMERA_FOV,
    CAMERA_EASE_MODE,
    CAMERA_EASE_TIME,
    GAMEPLAY_MOTION,
    CHARACTER_CHANGE,
    STAGE_CHANGE,
    UI_LOCK,
    UI_TOGGLE,
    UI_SLIDE
}

public abstract class UnanimatedTriggerBase : NoteBase
{
    public override NoteType Type => NoteType.UNANIMATED_TRIGGER;
    public override NoteLane Lane => NoteLane.CAMERA;
    
    public abstract TriggerType TriggerType { get; }
    
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
    private static SolidColorBrush _selectedOutlineBrush;
    private static SolidColorBrush _selectedFillBrush;
    private static double _selectedOutlineThickness;
    
    private readonly Geometry _shape = new PolylineGeometry(Vertices, true);

    public static void UpdateStyles()
    {
        _fillBrush = (SolidColorBrush)App.Current.Resources["Notes.UnanimatedTrigger.FillColor"];
        _outlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.UnanimatedTrigger.OutlineColor"];
        // thickness is always the same on all sides
        _outlineThickness =
            ((Thickness)App.Current.Resources["Notes.UnanimatedTrigger.OutlineThickness"]).Top;
        _selectedFillBrush =
            (SolidColorBrush)App.Current.Resources["Notes.UnanimatedTrigger.Selected.FillColor"];
        _selectedOutlineBrush =
            (SolidColorBrush)App.Current.Resources["Notes.UnanimatedTrigger.Selected.OutlineColor"];
        _selectedOutlineThickness =
            ((Thickness)App.Current.Resources["Notes.UnanimatedTrigger.Selected.OutlineThickness"])
            .Top;
    }
    
    public override void Render(DrawingContext dc, bool selected)
    {
        var x = NoteViewer.GetNoteX(Lane);
        var y = NoteViewer.TimeToScreenCoords(Time);

        if (y < -50 || y > NoteViewer.ViewerHeight + 50)
        {
            return;
        }

        var shape = _shape.Clone();
        shape.Transform = new TranslateTransform(x, y);
        
        var pen = selected ?
            new Pen(_selectedOutlineBrush, _selectedOutlineThickness) :
            new Pen(_outlineBrush, _outlineThickness);
        dc.DrawGeometry(selected ? _selectedFillBrush : _fillBrush, pen, shape);
        
        RenderFlags(dc, x, y);
        
        RenderDebugTime(dc, x, y);
    }
    
    public override void RenderPreview(DrawingContext dc) { }

    public override long? ShouldPlayHitSound(double rangeStart, double rangeEnd)
    {
        if (Time > rangeStart && Time <= rangeEnd && Config.Settings.HitSounds.UnanimatedTrigger)
        {
            return (long)(Time - rangeStart);
        }

        return null;
    }

    public static UnanimatedTriggerBase? FromEventString(string str)
    {
        return null;
    }

    public static async void ShowEditDialog()
    {
        
    }
    
    public abstract string ToEventString();
}