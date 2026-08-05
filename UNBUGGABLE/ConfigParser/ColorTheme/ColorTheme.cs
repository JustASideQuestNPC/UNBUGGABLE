using System;
using System.Text.RegularExpressions;
using Avalonia.Media;

namespace UNBUGGABLE.Resources;

public class ColorThemeException(string message) : Exception(message);

public static partial class ThemeUtils
{
    [GeneratedRegex("^#(?:(?:[\\da-fA-F]{3}){1,2}|(?:[\\da-fA-F]{4}){1,2})$")]
    private static partial Regex HexColorRegex();
    
    public static Color ParseColor(string hex, string keyName)
    {
        if (!HexColorRegex().IsMatch(hex))
        {
            throw new ColorThemeException($"Invalid hex color for key {keyName}: {hex}");
        }
        return Color.Parse(hex);
    }
}

public class MainWindowTheme(MainWindowThemeJson json)
{
    public class EventIndicatorTheme
    {
        public readonly Color BackgroundColor;
        public readonly Color TextColor;
        public readonly double TextSize;
        
        public EventIndicatorTheme(MainWindowThemeJson.EventIndicatorThemeJson json)
        {
            BackgroundColor = ThemeUtils.ParseColor(json.BackgroundColor,
                                                      "mainWindow.eventIndicator.backgroundColor");
            TextColor = ThemeUtils.ParseColor(json.TextColor,
                                              "mainWindow.eventIndicator.textColor");
            TextSize = json.TextSize;
            if (TextSize <= 0)
            {
                throw new ColorThemeException(
                    "mainWindow.eventIndicator.textSize must be positive");
            }
        }
    }
    
    public readonly Color BackgroundColor = ThemeUtils.ParseColor(json.BackgroundColor,
                                                                  "mainWindow.backgroundColor");
    public readonly EventIndicatorTheme EventIndicator = new(json.EventIndicator);
}

public class ElementTheme
{
    public readonly Color BackgroundColor;
    public readonly Color OutlineColor;
    public readonly double OutlineThickness;
    public readonly double CornerRadius;

    protected ElementTheme(ElementThemeJson json, string keyName)
    {
        BackgroundColor = ThemeUtils.ParseColor(json.BackgroundColor, $"{keyName}.backgroundColor");
        OutlineColor = ThemeUtils.ParseColor(json.OutlineColor, $"{keyName}.outlineColor");
        OutlineThickness = json.OutlineThickness;
        CornerRadius = json.CornerRadius;
        
        if (OutlineThickness < 0)
        {
            throw new ColorThemeException($"{keyName}.outlineThickness cannot be negative");
        }
        
        if (CornerRadius < 0)
        {
            throw new ColorThemeException($"{keyName}.cornerRadius cannot be negative");
        }
    }
}

public class TextElementTheme : ElementTheme
{
    public readonly Color TextColor;
    public readonly double TextSize;
        
    public TextElementTheme(TextElementThemeJson json, string keyName) : base(json, keyName)
    {
        TextColor = ThemeUtils.ParseColor(json.TextColor, $"{keyName}.textColor");
        TextSize = json.TextSize;
        if (TextSize <= 0)
        {
            throw new ColorThemeException($"{keyName}.textSize must be positive");
        }
    }
}

public class ButtonTheme : ElementTheme
{
    public class HoveredTheme(Color background, Color outline, Color icon)
    {
        public readonly Color BackgroundColor = background;
        public readonly Color OutlineColor = outline;
        public readonly Color IconColor = icon;
    }
    
    public ButtonTheme(ButtonThemeJson json, string keyName) : base(json, keyName)
    {
        IconColor = ThemeUtils.ParseColor(json.IconColor, $"{keyName}.iconColor");
        
        Hovered = new HoveredTheme(
            json.Hovered.BackgroundColor != "" ?
                ThemeUtils.ParseColor(json.Hovered.BackgroundColor,
                                      $"{keyName}.selected.backgroundColor") : BackgroundColor,
            json.Hovered.OutlineColor != "" ?
                ThemeUtils.ParseColor(json.Hovered.OutlineColor,
                                      $"{keyName}.selected.outlineColor") : OutlineColor,
            json.Hovered.IconColor != "" ?
                ThemeUtils.ParseColor(json.Hovered.IconColor,
                                      $"{keyName}.selected.iconColor") : IconColor);
    }

    public readonly Color IconColor;
    public readonly HoveredTheme Hovered;
}

public class TopBarTheme(TopBarThemeJson json)
{
    public class SliderTheme
    {
        public readonly Color TopColor;
        public readonly Color BottomColor;
        public readonly Color IconColor;
        public readonly Color HandleColor;
        public readonly double HandleWidth;
        public readonly double HandleHeight;
        public readonly double TopThickness;
        public readonly double BottomThickness;

        public SliderTheme(TopBarThemeJson.SliderThemeJson json)
        {
            TopColor = ThemeUtils.ParseColor(json.TopColor, "topBar.sliders.topColor");
            BottomColor = ThemeUtils.ParseColor(json.BottomColor, "topBar.sliders.bottomColor");
            IconColor = ThemeUtils.ParseColor(json.IconColor, "topBar.sliders.iconColor");
            HandleColor = ThemeUtils.ParseColor(json.HandleColor, "topBar.sliders.handleColor");
            
            HandleWidth = json.HandleWidth;
            HandleHeight = json.HandleHeight;
            TopThickness = json.TopThickness;
            BottomThickness = json.BottomThickness;
            
            if (HandleWidth <= 0)
            {
                throw new ColorThemeException("topBar.sliders.handleWidth must be positive");
            }

            if (HandleHeight <= 0)
            {
                throw new ColorThemeException("topBar.sliders.handleHeight must be positive");
            }

            if (TopThickness < 0)
            {
                throw new ColorThemeException("topBar.sliders.topThickness cannot be negative");
            }
            
            if (BottomThickness < 0)
            {
                throw new ColorThemeException("topBar.sliders.bottomThickness cannot be negative");
            }
        }
    }

    public readonly Color BackgroundColor = ThemeUtils.ParseColor(json.BackgroundColor,
                                                                  "topBar.backgroundColor");
    public readonly SliderTheme Sliders = new(json.Sliders);
    public readonly ButtonTheme Buttons = new(json.Buttons, "topBar.buttons");
    public readonly TextElementTheme Tooltips = new(json.Tooltips, "topBar.tooltips");
    public readonly TextElementTheme SaveFileContextMenu = new(json.SaveFileContextMenu,
                                                               "topBar.saveFileContextMenu");
}

public class DialogTheme(DialogThemeJson json) : TextElementTheme(json, "dialogs")
{
    public readonly TextElementTheme InputBoxes = new(json.InputBoxes, "dialogs.inputBoxes");
    public readonly ButtonTheme Buttons = new(json.Buttons, "dialogs.buttons");
}

public class QuickInfoTheme
{
    public readonly Color TitleColor;
    public readonly Color InfoColor;
    public readonly double TitleSize;
    public readonly double InfoSize;
    
    public QuickInfoTheme(QuickInfoThemeJson json)
    {
        TitleColor = ThemeUtils.ParseColor(json.TitleColor, "quickInfo.titleColor");
        InfoColor = ThemeUtils.ParseColor(json.InfoColor, "quickInfo.infoColor");
        TitleSize = json.TitleSize;
        InfoSize = json.InfoSize;
        
        if (TitleSize <= 0)
        {
            throw new ColorThemeException("quickInfo.titleSize must be positive");
        }
        
        if (InfoSize <= 0)
        {
            throw new ColorThemeException("quickInfo.infoSize must be positive");
        }
    }
}

public class LineTheme
{
    public readonly Color Color;
    public readonly double Thickness;
    
    public LineTheme(LineThemeJson json, string keyName)
    {
        Color = ThemeUtils.ParseColor(json.Color, $"{keyName}.color");
        Thickness = json.Thickness;

        if (Thickness < 0)
        {
            throw new ColorThemeException($"{keyName}.thickness cannot be negative");
        }
    }
}

public class NoteViewerTheme(NoteViewerThemeJson json) : ElementTheme(json, "noteViewer")
{
    public class LaneNumberTheme
    {
        public readonly Color Color;
        public readonly Color OutlineColor;
        public readonly double OutlineThickness;
        public readonly double TextSize;
        
        public LaneNumberTheme(NoteViewerThemeJson.LaneNumberThemeJson json)
        {
            Color = ThemeUtils.ParseColor(json.Color, "noteViewer.laneNumbers.color");
            OutlineColor = ThemeUtils.ParseColor(json.OutlineColor,
                                                 "noteViewer.laneNumbers.outlineColor");
            OutlineThickness = json.OutlineThickness;
            TextSize = json.TextSize;
            
            if (OutlineThickness < 0)
            {
                throw new ColorThemeException(
                    "noteViewer.laneNumbers.outlineThickness cannot be negative");
            }
            if (TextSize <= 0)
            {
                throw new ColorThemeException("noteViewer.laneNumbers.textSize must be positive");
            }
        }
    }
    
    public class LabeledLineTheme
    {
        public readonly Color Color;
        public readonly double LineThickness;
        public readonly double TextSize;
        
        public LabeledLineTheme(NoteViewerThemeJson.LabeledLineThemeJson json, string keyName)
        {
            Color = ThemeUtils.ParseColor(json.Color, $"{keyName}.color");
            LineThickness = json.LineThickness;
            TextSize = json.TextSize;
            
            if (LineThickness < 0)
            {
                throw new ColorThemeException($"{keyName}.lineThickness cannot be negative");
            }
            
            if (TextSize <= 0)
            {
                throw new ColorThemeException($"{keyName}.textSize must be positive");
            }
        }
    }

    public class FullBeatSnapLineTheme : LineTheme
    {
        public readonly double TextSize;
        
        public FullBeatSnapLineTheme(NoteViewerThemeJson.FullBeatSnapLineThemeJson json) :
            base(json, "noteViewer.fullBeatSnapLine")
        {
            TextSize = json.TextSize;
            
            if (TextSize <= 0)
            {
                throw new ColorThemeException(
                    "noteViewer.fullBeatSnapLine.textSize must be positive");
            }
        }
    }
    
    public class MarkersTheme
    {
        public readonly Color Color1;
        public readonly Color Color2;
        public readonly Color Color3;
        public readonly double ArrowScale;
        
        public MarkersTheme(NoteViewerThemeJson.MarkersThemeJson json)
        {
            Color1 = ThemeUtils.ParseColor(json.Color1, "noteViewer.markers.color1");
            Color2 = ThemeUtils.ParseColor(json.Color2, "noteViewer.markers.color2");
            Color3 = ThemeUtils.ParseColor(json.Color3, "noteViewer.markers.color3");
            ArrowScale = json.ArrowScale;
            if (ArrowScale < 0)
            {
                throw new ColorThemeException("noteViewer.markers.arrowScale cannot be negative");
            }
        }
    }
    
    public class BreakpointTheme : LineTheme
    {
        public readonly Color Color;
        public readonly double ArrowScale;
        
        public BreakpointTheme(NoteViewerThemeJson.BreakpointThemeJson json) :
            base(json, "noteViewer.breakpoint")
        {
            Color = ThemeUtils.ParseColor(json.Color, "noteViewer.breakpoint.color");
            ArrowScale = json.ArrowScale;
            if (ArrowScale < 0)
            {
                throw new ColorThemeException(
                    "noteViewer.breakpoint.arrowScale cannot be negative");
            }
        }
    }
    
    public readonly Color SelectDragColor = ThemeUtils.ParseColor(json.SelectDragColor,
                                                                  "noteViewer.selectDragColor");
    public readonly Color DeleteDragColor = ThemeUtils.ParseColor(json.DeleteDragColor,
                                                                  "noteViewer.deleteDragColor");
    public readonly LaneNumberTheme LaneNumbers = new(json.LaneNumbers);
    public readonly LabeledLineTheme BpmChange = new(json.BpmChanges, "noteViewer.bpmChange");
    public readonly LabeledLineTheme Label = new(json.Labels, "noteViewer.label");
    public readonly FullBeatSnapLineTheme FullBeatSnapLine = new(json.FullBeatSnapLine);
    public readonly LineTheme SubBeatSnapLine = new(json.SubBeatSnapLine,
                                                    "noteViewer.subBeatSnapLine");
    public readonly LineTheme CurrentTimeLine = new(json.CurrentTimeLine,
                                                    "noteViewer.currentTimeLine");
    public readonly MarkersTheme Markers = new(json.Markers);
    public readonly BreakpointTheme Breakpoint = new(json.Breakpoint);
}

public class GamePreviewTheme : ElementTheme
{
    public class ViewableAreaTheme
    {
        public readonly Color OutlineColor;
        public readonly double OutlineThickness;
        
        public ViewableAreaTheme(GamePreviewThemeJson.ViewableAreaThemeJson json)
        {
            OutlineColor = ThemeUtils.ParseColor(json.OutlineColor,
                                                 "gamePreview.viewableArea.outlineColor");
            OutlineThickness = json.OutlineThickness;
            
            if (OutlineThickness < 0)
            {
                throw new ColorThemeException(
                    "gamePreviewer.viewableArea.outlineColor cannot be negative");
            }
        }
    }

    public class NoteTargetsTheme
    {
        public class TargetCirclesTheme
        {
            public readonly Color FillColor;
            public readonly Color OutlineColor;
            public readonly double OutlineThickness;
            public readonly double Radius;
            
            public TargetCirclesTheme(
                GamePreviewThemeJson.NoteTargetsThemeJson.TargetCirclesThemeJson json)
            {
                FillColor = ThemeUtils.ParseColor(json.FillColor,
                                                  "gamePreview.noteTargets.circles.fillColor");
                OutlineColor = ThemeUtils.ParseColor(json.OutlineColor,
                                                     "gamePreview.noteTargets.circles.outlineColor");
                
                OutlineThickness = json.OutlineThickness;
                Radius = json.Radius;
                if (OutlineThickness < 0)
                {
                    throw new ColorThemeException(
                        "gamePreview.noteTargets.circles.outlineThickness cannot be negative");
                }
                if (Radius <= 0)
                {
                    throw new ColorThemeException(
                        "gamePreview.noteTargets.circles.radius must be positive");
                }
            }
        }

        public readonly Color LineColor;
        public readonly double LineThickness;
        public readonly TargetCirclesTheme Circles;
        
        public NoteTargetsTheme(GamePreviewThemeJson.NoteTargetsThemeJson json)
        {
            LineColor = ThemeUtils.ParseColor(json.LineColor, "gamePreview.noteTargets.lineColor");
            LineThickness = json.LineThickness;
            Circles = new TargetCirclesTheme(json.TargetCircles);

            if (LineThickness < 0)
            {
                throw new ColorThemeException(
                    "gamePreview.noteTargets.lineThickness cannot be negative");
            }
        }
    }

    public readonly Color CopColor;
    public readonly Color CameraArrowColor;
    public readonly double CameraArrowScale;
    public readonly ViewableAreaTheme ViewableArea;
    public readonly NoteTargetsTheme NoteTargets;
    
    public GamePreviewTheme(GamePreviewThemeJson json) : base(json, "gamePreview")
    {
        CopColor = ThemeUtils.ParseColor(json.CopColor, "gamePreview.copColor");
        CameraArrowColor = ThemeUtils.ParseColor(json.CameraArrowColor,
                                                 "gamePreview.cameraArrowColor");
        CameraArrowScale = json.CameraArrowScale;
        ViewableArea = new ViewableAreaTheme(json.ViewableArea);
        NoteTargets = new NoteTargetsTheme(json.NoteTargets);
        
        if (CameraArrowScale < 0)
        {
            throw new ColorThemeException("gamePreview.cameraArrowScale cannot be negative");
        }
    }
}

public class PlacementPriorityListTheme : ElementTheme
{
    public class ListEntryTheme(PlacementPriorityListThemeJson.ListEntryThemeJson json) :
        TextElementTheme(json, "placementPriorityList.listEntries")
    {
        public readonly Color ReorderIconColor =
            ThemeUtils.ParseColor(json.ReorderIconColor,
                                  "placementPriorityList.listEntries.reorderIconColor");
    }

    public readonly Color TitleColor;
    public readonly double TitleSize;
    public readonly ListEntryTheme ListEntries;
    
    public PlacementPriorityListTheme(PlacementPriorityListThemeJson json) :
        base(json, "placementPriorityList")
    {
        TitleColor = ThemeUtils.ParseColor(json.TitleColor, "placementPriorityList.titleColor");
        TitleSize = json.TitleSize;
        ListEntries = new ListEntryTheme(json.ListEntries);

        if (TitleSize <= 0)
        {
            throw new ColorThemeException("placementPriorityList.titleSize must be positive");
        }
    }
}

public class InstantNoteTheme
{
    public class SelectedTheme(Color fill, Color outline, double outlineThickness)
    {
        public readonly Color FillColor = fill;
        public readonly Color OutlineColor = outline;
        public readonly double OutlineThickness = outlineThickness;
    }
    
    public readonly Color FillColor;
    public readonly Color OutlineColor;
    public readonly double OutlineThickness;
    public readonly SelectedTheme Selected;
    
    public InstantNoteTheme(InstantNoteThemeJson json, string keyName)
    {
        FillColor = ThemeUtils.ParseColor(json.FillColor, $"{keyName}.fillColor");
        OutlineColor = ThemeUtils.ParseColor(json.OutlineColor, $"{keyName}.outlineColor");
        OutlineThickness = json.OutlineThickness;

        if (OutlineThickness < 0)
        {
            throw new ColorThemeException($"{keyName}.outlineThickness cannot be negative");
        }

        if (json.Selected.OutlineThickness < 0 && json.Selected.OutlineThickness != -1)
        {
            throw new ColorThemeException(
                $"{keyName}.selected.outlineThickness cannot be negative");
        }
        
        Selected = new SelectedTheme(
            json.Selected.FillColor != "" ?
                ThemeUtils.ParseColor(json.Selected.FillColor,
                                      $"{keyName}.selected.fillColor") : FillColor,
            json.Selected.OutlineColor != "" ?
                ThemeUtils.ParseColor(json.Selected.OutlineColor,
                                      $"{keyName}.selected.outlineColor") : OutlineColor,
            json.Selected.OutlineThickness != -1 ? json.Selected.OutlineThickness :
                OutlineThickness);
    }
}

public class NonInstantNoteTheme
{
    public class SelectedTheme(Color fill, Color outline, double outlineThickness, Color tailFill,
        Color tailOutline, double tailOutlineThickness)
    {
        public readonly Color FillColor = fill;
        public readonly Color OutlineColor = outline;
        public readonly double OutlineThickness = outlineThickness;
        public readonly Color TailColor = tailFill;
        public readonly Color TailOutlineColor = tailOutline;
        public readonly double TailOutlineThickness = tailOutlineThickness;
    }
    
    public readonly Color FillColor;
    public readonly Color OutlineColor;
    public readonly double OutlineThickness;
    public readonly Color TailColor;
    public readonly Color TailOutlineColor;
    public readonly double TailOutlineThickness;
    public readonly SelectedTheme Selected;
    
    public NonInstantNoteTheme(NonInstantNoteThemeJson json, string keyName)
    {
        FillColor = ThemeUtils.ParseColor(json.FillColor, $"{keyName}.fillColor");
        OutlineColor = ThemeUtils.ParseColor(json.OutlineColor, $"{keyName}.outlineColor");
        OutlineThickness = json.OutlineThickness;
        TailColor = ThemeUtils.ParseColor(json.TailColor, $"{keyName}.tailColor");
        TailOutlineColor = ThemeUtils.ParseColor(json.TailOutlineColor,
                                                 $"{keyName}.tailOutlineColor");
        TailOutlineThickness = json.TailOutlineThickness;

        if (OutlineThickness < 0)
        {
            throw new ColorThemeException($"{keyName}.outlineThickness cannot be negative");
        }
        
        if (TailOutlineThickness < 0)
        {
            throw new ColorThemeException($"{keyName}.tailOutlineThickness cannot be negative");
        }
        
        if (json.Selected.OutlineThickness < 0 && json.Selected.OutlineThickness != -1)
        {
            throw new ColorThemeException(
                $"{keyName}.selected.outlineThickness cannot be negative");
        }
        
        if (json.Selected.TailOutlineThickness < 0 && json.Selected.TailOutlineThickness != -1)
        {
            throw new ColorThemeException(
                $"{keyName}.selected.tailOutlineThickness cannot be negative");
        }
        
        Selected = new SelectedTheme(
            json.Selected.FillColor != "" ?
                ThemeUtils.ParseColor(json.Selected.FillColor,
                                      $"{keyName}.selected.fillColor") : FillColor,
            json.Selected.OutlineColor != "" ?
                ThemeUtils.ParseColor(json.Selected.OutlineColor,
                                      $"{keyName}.selected.outlineColor") : OutlineColor,
            json.Selected.OutlineThickness != -1 ?
                json.Selected.OutlineThickness : OutlineThickness,
            json.Selected.TailColor != "" ?
                ThemeUtils.ParseColor(json.Selected.TailColor,
                                      $"{keyName}.selected.tailColor") : TailColor,
            json.Selected.TailOutlineColor != "" ?
                ThemeUtils.ParseColor(json.Selected.TailOutlineColor,
                                      $"{keyName}.selected.tailOutlineColor") : TailOutlineColor,
            json.Selected.OutlineThickness != -1 ?
                json.Selected.OutlineThickness : OutlineThickness);
    }
}

public class NoteThemes(NoteThemesJson json)
{
    public class CommonTheme
    {
        public readonly Color FlagTextColor;
        public readonly Color FlagTextOutlineColor;
        public readonly double FlagTextOutlineThickness;
        public readonly double FlagTextSize;

        public CommonTheme(NoteThemesJson.CommonThemeJson json)
        {
            FlagTextColor = ThemeUtils.ParseColor(json.FlagTextColor, "notes.common.flagTextColor");
            FlagTextOutlineColor = ThemeUtils.ParseColor(json.FlagTextOutlineColor,
                                                         "notes.common.flagTextOutlineColor");
            FlagTextOutlineThickness = json.FlagTextOutlineThickness;
            FlagTextSize = json.FlagTextSize;

            if (FlagTextOutlineThickness < 0)
            {
                throw new ColorThemeException(
                    "notes.common.flagTextOutlineThickness cannot be negative");
            }
            if (FlagTextSize <= 0)
            {
                throw new ColorThemeException("notes.common.flagTextSize must be positive");
            }
        }
    }
    
    public readonly CommonTheme Common = new(json.Common);
    public readonly InstantNoteTheme Single = new(json.Single, "notes.single");
    public readonly InstantNoteTheme Spike = new(json.Spike, "notes.spike");
    public readonly NonInstantNoteTheme Hold = new(json.Hold, "notes.hold");
    public readonly NonInstantNoteTheme Double = new(json.Double, "notes.double");
    public readonly InstantNoteTheme Freestyle = new(json.Freestyle, "notes.freestyle");
    public readonly InstantNoteTheme Camera = new (json.Camera, "notes.camera");
    public readonly NonInstantNoteTheme Mash = new(json.Mash, "notes.mash");
    public readonly NonInstantNoteTheme Cop1 = new(json.Cop1, "notes.cop1");
    public readonly NonInstantNoteTheme Cop2 = new(json.Cop2, "notes.cop2");
    public readonly NonInstantNoteTheme Cop3 = new(json.Cop3, "notes.cop3");
    public readonly NonInstantNoteTheme Cop4 = new(json.Cop4, "notes.cop4");
}

public class DebugInfoTheme
{
    public readonly Color OverlayBackgroundColor;
    public readonly Color OverlayTextColor;
    public readonly double OverlayTextSize;
    public readonly Color NoteTimestampTextColor;
    public readonly Color NoteTimestampTextOutlineColor;
    public readonly double NoteTimestampTextSize;
    public readonly double NoteTimestampTextOutlineThickness;

    public DebugInfoTheme(DebugInfoThemeJson json)
    {
        OverlayBackgroundColor = ThemeUtils.ParseColor(json.OverlayBackgroundColor,
                                                       "debugInfo.overlayBackgroundColor");
        OverlayTextColor = ThemeUtils.ParseColor(json.OverlayTextColor,
                                                 "debugInfo.overlayTextColor");
        OverlayTextSize = json.OverlayTextSize;
        NoteTimestampTextColor = ThemeUtils.ParseColor(json.NoteTimestampTextColor,
                                                       "debugInfo.noteTimestampTextColor");
        NoteTimestampTextOutlineColor =
            ThemeUtils.ParseColor(json.NoteTimestampTextOutlineColor,
                                  "debugInfo.noteTimestampTextOutlineColor");
        NoteTimestampTextSize = json.NoteTimestampTextSize;
        NoteTimestampTextOutlineThickness = json.NoteTimestampTextOutlineThickness;
        
        if (OverlayTextSize <= 0)
        {
            throw new ColorThemeException("debugInfo.overlayTextSize must be positive");
        }
        if (NoteTimestampTextSize <= 0)
        {
            throw new ColorThemeException("debugInfo.noteTimestampTextSize must be positive");
        }
        if (NoteTimestampTextOutlineThickness < 0)
        {
            throw new ColorThemeException(
                "debugInfo.noteTimestampTextOutlineThickness cannot be negative");
        }
    }
}

public class ColorTheme(ColorThemeJson json)
{
    public readonly MainWindowTheme MainWindow = new(json.MainWindow);
    public readonly TopBarTheme TopBar = new(json.TopBar);
    public readonly DialogTheme Dialogs = new(json.Dialogs);
    public readonly QuickInfoTheme QuickInfo = new(json.QuickInfo);
    public readonly NoteViewerTheme NoteViewer = new(json.NoteViewer);
    public readonly GamePreviewTheme GamePreview = new(json.GamePreview);
    public readonly PlacementPriorityListTheme PlacementPriorityList =
        new(json.PlacementPriorityList);
    public readonly NoteThemes Notes = new(json.NoteThemes);
    public readonly DebugInfoTheme DebugInfo = new(json.DebugInfo);
}