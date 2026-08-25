using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Media;

namespace UNBUGGABLE.Resources;

// public class ColorThemeException(string message) : Exception(message);

public static partial class ThemeUtils
{
    [GeneratedRegex("^#(?:(?:[\\da-fA-F]{3}){1,2}|(?:[\\da-fA-F]{4}){1,2})$")]
    private static partial Regex HexColorRegex();
    
    public static Color ParseColor(string hex, string keyName, ref List<string> errorMessages)
    {
        if (!HexColorRegex().IsMatch(hex))
        {
            errorMessages.Add($"Invalid hex color for key {keyName}: {hex}");
            // what color we return here doesn't matter because the theme will get discarded anyway
            return Colors.Transparent;
        }
        
        // for some godawful reason, avalonia uses #aarrggbb instead of #rrggbbaa, so now i have to
        // shuffle around the characters myself
        var colors = hex[1..].ToList();
        if (colors.Count == 4)
        {
            colors.Insert(0, colors[3]);
            colors.RemoveAt(4);
        }
        else if (colors.Count == 8)
        {
            colors.Insert(0, colors[7]);
            colors.RemoveAt(8);
            colors.Insert(0, colors[7]);
            colors.RemoveAt(8);
        }

        hex = $"#{string.Join("", colors)}";
        return Color.Parse(hex);
    }
}

public class MainWindowTheme(MainWindowThemeJson json, ref List<string> errorMessages)
{
    public class EventIndicatorTheme
    {
        public readonly Color BackgroundColor;
        public readonly Color TextColor;
        public readonly double TextSize;
        
        public EventIndicatorTheme(MainWindowThemeJson.EventIndicatorThemeJson json,
            ref List<string> errorMessages)
        {
            BackgroundColor = ThemeUtils.ParseColor(json.BackgroundColor,
                                                    "mainWindow.eventIndicator.backgroundColor",
                                                    ref errorMessages);
            TextColor = ThemeUtils.ParseColor(json.TextColor,
                                              "mainWindow.eventIndicator.textColor",
                                              ref errorMessages);
            TextSize = json.TextSize;
            if (TextSize <= 0)
            {
                errorMessages.Add("mainWindow.eventIndicator.textSize must be positive");
            }
        }
    }
    
    public readonly Color BackgroundColor = ThemeUtils.ParseColor(json.BackgroundColor,
                                                                  "mainWindow.backgroundColor",
                                                                  ref errorMessages);
    public readonly EventIndicatorTheme EventIndicator = new(json.EventIndicator,
                                                             ref errorMessages);
}

public class ElementTheme
{
    public readonly Color BackgroundColor;
    public readonly Color OutlineColor;
    public readonly double OutlineThickness;
    public readonly double CornerRadius;

    protected ElementTheme(ElementThemeJson json, string keyName, ref List<string> errorMessages)
    {
        BackgroundColor = ThemeUtils.ParseColor(json.BackgroundColor, $"{keyName}.backgroundColor",
                                                ref errorMessages);
        OutlineColor = ThemeUtils.ParseColor(json.OutlineColor, $"{keyName}.outlineColor",
                                             ref errorMessages);
        OutlineThickness = json.OutlineThickness;
        CornerRadius = json.CornerRadius;
        
        if (OutlineThickness < 0)
        {
            errorMessages.Add($"{keyName}.outlineThickness cannot be negative");
        }
        
        if (CornerRadius < 0)
        {
            errorMessages.Add($"{keyName}.cornerRadius cannot be negative");
        }
    }
}

public class TextElementTheme : ElementTheme
{
    public readonly Color TextColor;
    public readonly double TextSize;
        
    public TextElementTheme(TextElementThemeJson json, string keyName,
        ref List<string> errorMessages) : base(json, keyName, ref errorMessages)
    {
        TextColor = ThemeUtils.ParseColor(json.TextColor, $"{keyName}.textColor",
                                          ref errorMessages);
        TextSize = json.TextSize;
        if (TextSize <= 0)
        {
            errorMessages.Add($"{keyName}.textSize must be positive");
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
    
    public ButtonTheme(ButtonThemeJson json, string keyName, ref List<string> errorMessages) :
        base(json, keyName, ref errorMessages)
    {
        IconColor = ThemeUtils.ParseColor(json.IconColor, $"{keyName}.iconColor",
                                          ref errorMessages);
        
        Hovered = new HoveredTheme(
            json.Hovered.BackgroundColor != "" ?
                ThemeUtils.ParseColor(json.Hovered.BackgroundColor,
                                      $"{keyName}.selected.backgroundColor", ref errorMessages) :
                BackgroundColor,
            json.Hovered.OutlineColor != "" ?
                ThemeUtils.ParseColor(json.Hovered.OutlineColor,
                                      $"{keyName}.selected.outlineColor", ref errorMessages) :
                OutlineColor,
            json.Hovered.IconColor != "" ?
                ThemeUtils.ParseColor(json.Hovered.IconColor,
                                      $"{keyName}.selected.iconColor", ref errorMessages) :
                IconColor);
    }

    public readonly Color IconColor;
    public readonly HoveredTheme Hovered;
}

public class TopBarTheme(TopBarThemeJson json, ref List<string> errorMessages)
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

        public SliderTheme(TopBarThemeJson.SliderThemeJson json, ref List<string> errorMessages)
        {
            TopColor = ThemeUtils.ParseColor(json.TopColor, "topBar.sliders.topColor",
                                             ref errorMessages);
            BottomColor = ThemeUtils.ParseColor(json.BottomColor, "topBar.sliders.bottomColor",
                                                ref errorMessages);
            IconColor = ThemeUtils.ParseColor(json.IconColor, "topBar.sliders.iconColor",
                                              ref errorMessages);
            HandleColor = ThemeUtils.ParseColor(json.HandleColor, "topBar.sliders.handleColor",
                                                ref errorMessages);
            
            HandleWidth = json.HandleWidth;
            HandleHeight = json.HandleHeight;
            TopThickness = json.TopThickness;
            BottomThickness = json.BottomThickness;
            
            if (HandleWidth <= 0)
            {
                errorMessages.Add("topBar.sliders.handleWidth must be positive");
            }

            if (HandleHeight <= 0)
            {
                errorMessages.Add("topBar.sliders.handleHeight must be positive");
            }

            if (TopThickness < 0)
            {
                errorMessages.Add("topBar.sliders.topThickness cannot be negative");
            }
            
            if (BottomThickness < 0)
            {
                errorMessages.Add("topBar.sliders.bottomThickness cannot be negative");
            }
        }
    }

    public readonly Color BackgroundColor = ThemeUtils.ParseColor(json.BackgroundColor,
                                                                  "topBar.backgroundColor",
                                                                  ref errorMessages);
    public readonly SliderTheme Sliders = new(json.Sliders, ref errorMessages);
    public readonly ButtonTheme Buttons = new(json.Buttons, "topBar.buttons", ref errorMessages);
    public readonly TextElementTheme Tooltips = new(json.Tooltips, "topBar.tooltips",
                                                    ref errorMessages);
    public readonly TextElementTheme SaveFileContextMenu = new(json.SaveFileContextMenu,
                                                               "topBar.saveFileContextMenu",
                                                               ref errorMessages);
}

public class DialogTheme(DialogThemeJson json, ref List<string> errorMessages) :
    TextElementTheme(json, "dialogs", ref errorMessages)
{
    public readonly TextElementTheme InputBoxes = new(json.InputBoxes, "dialogs.inputBoxes",
                                                      ref errorMessages);
    public readonly ButtonTheme Buttons = new(json.Buttons, "dialogs.buttons", ref errorMessages);
}

public class QuickInfoTheme
{
    public readonly Color TitleColor;
    public readonly Color InfoColor;
    public readonly double TitleSize;
    public readonly double InfoSize;
    
    public QuickInfoTheme(QuickInfoThemeJson json, ref List<string> errorMessages)
    {
        TitleColor = ThemeUtils.ParseColor(json.TitleColor, "quickInfo.titleColor",
                                           ref errorMessages);
        InfoColor = ThemeUtils.ParseColor(json.InfoColor, "quickInfo.infoColor", ref errorMessages);
        TitleSize = json.TitleSize;
        InfoSize = json.InfoSize;
        
        if (TitleSize <= 0)
        {
            errorMessages.Add("quickInfo.titleSize must be positive");
        }
        
        if (InfoSize <= 0)
        {
            errorMessages.Add("quickInfo.infoSize must be positive");
        }
    }
}

public class LineTheme
{
    public readonly Color Color;
    public readonly double Thickness;
    
    public LineTheme(LineThemeJson json, string keyName, ref List<string> errorMessages)
    {
        Color = ThemeUtils.ParseColor(json.Color, $"{keyName}.color", ref errorMessages);
        Thickness = json.Thickness;

        if (Thickness < 0)
        {
            errorMessages.Add($"{keyName}.thickness cannot be negative");
        }
    }
}

public class NoteViewerTheme : ElementTheme
{
    public class LaneNumberTheme
    {
        public readonly Color Color;
        public readonly Color OutlineColor;
        public readonly double OutlineThickness;
        public readonly double TextSize;
        
        public LaneNumberTheme(NoteViewerThemeJson.LaneNumberThemeJson json,
            ref List<string> errorMessages)
        {
            Color = ThemeUtils.ParseColor(json.Color, "noteViewer.laneNumbers.color",
                                          ref errorMessages);
            OutlineColor = ThemeUtils.ParseColor(json.OutlineColor,
                                                 "noteViewer.laneNumbers.outlineColor",
                                                 ref errorMessages);
            OutlineThickness = json.OutlineThickness;
            TextSize = json.TextSize;
            
            if (OutlineThickness < 0)
            {
                errorMessages.Add(
                    "noteViewer.laneNumbers.outlineThickness cannot be negative");
            }
            if (TextSize <= 0)
            {
                errorMessages.Add("noteViewer.laneNumbers.textSize must be positive");
            }
        }
    }
    
    public class LabeledLineTheme
    {
        public readonly Color Color;
        public readonly double LineThickness;
        public readonly double TextSize;
        
        public LabeledLineTheme(NoteViewerThemeJson.LabeledLineThemeJson json, string keyName,
            ref List<string> errorMessages)
        {
            Color = ThemeUtils.ParseColor(json.Color, $"{keyName}.color", ref errorMessages);
            LineThickness = json.LineThickness;
            TextSize = json.TextSize;
            
            if (LineThickness < 0)
            {
                errorMessages.Add($"{keyName}.lineThickness cannot be negative");
            }
            
            if (TextSize <= 0)
            {
                errorMessages.Add($"{keyName}.textSize must be positive");
            }
        }
    }

    public class FullBeatSnapLineTheme : LineTheme
    {
        public readonly double TextSize;
        
        public FullBeatSnapLineTheme(NoteViewerThemeJson.FullBeatSnapLineThemeJson json,
            ref List<string> errorMessages) : base(json, "noteViewer.fullBeatSnapLine",
                                                   ref errorMessages)
        {
            TextSize = json.TextSize;
            
            if (TextSize <= 0)
            {
                errorMessages.Add(
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
        
        public MarkersTheme(NoteViewerThemeJson.MarkersThemeJson json,
            ref List<string> errorMessages)
        {
            Color1 = ThemeUtils.ParseColor(json.Color1, "noteViewer.markers.color1",
                                           ref errorMessages);
            Color2 = ThemeUtils.ParseColor(json.Color2, "noteViewer.markers.color2",
                                           ref errorMessages);
            Color3 = ThemeUtils.ParseColor(json.Color3, "noteViewer.markers.color3",
                                           ref errorMessages);
            ArrowScale = json.ArrowScale;
            if (ArrowScale < 0)
            {
                errorMessages.Add("noteViewer.markers.arrowScale cannot be negative");
            }
        }
    }
    
    public class BreakpointTheme : LineTheme
    {
        public readonly double ArrowScale;
        
        public BreakpointTheme(NoteViewerThemeJson.BreakpointThemeJson json,
            ref List<string> errorMessages) : base(json, "noteViewer.breakpoint", ref errorMessages)
        {
            ArrowScale = json.ArrowScale;
            if (ArrowScale < 0)
            {
                errorMessages.Add(
                    "noteViewer.breakpoint.arrowScale cannot be negative");
            }
        }
    }
    
    public class NoteLaneThemes
    {
        public readonly Color TopColor;
        public readonly Color BottomColor;
        public readonly Color CenterColor;
        public readonly Color CameraColor;
    
        public readonly double TopWidth;
        public readonly double BottomWidth;
        public readonly double CenterWidth;
        public readonly double CameraWidth;

        public NoteLaneThemes(NoteLaneThemesJson json, ref List<string> errorMessages)
        {
            TopColor = ThemeUtils.ParseColor(json.TopColor, "noteViewer.noteLanes.topColor",
                                             ref errorMessages);
            BottomColor = ThemeUtils.ParseColor(json.BottomColor,
                                                "noteViewer.noteLanes.bottomColor",
                                                ref errorMessages);
            CenterColor = ThemeUtils.ParseColor(json.CenterColor,
                                                "noteViewer.noteLanes.centerColor",
                                                ref errorMessages);
            CameraColor = ThemeUtils.ParseColor(json.CameraColor,
                                                "noteViewer.noteLanes.cameraColor",
                                                ref errorMessages);
        
            TopWidth = json.TopWidth;
            BottomWidth = json.BottomWidth;
            CenterWidth = json.CenterWidth;
            CameraWidth = json.CameraWidth;
        
            if (TopWidth < 0)
            {
                errorMessages.Add("noteViewer.noteLanes.topWidth cannot be negative");
            }
        
            if (BottomWidth < 0)
            {
                errorMessages.Add(
                    "noteViewer.noteLanes.bottomWidth cannot be negative");
            }
        
            if (CenterWidth < 0)
            {
                errorMessages.Add(
                    "noteViewer.noteLanes.centerWidth cannot be negative");
            }
        
            if (CameraWidth < 0)
            {
                errorMessages.Add(
                    "noteViewer.noteLanes.cameraWidth cannot be negative");
            }
        }
    }
    
    public readonly Color SelectDragColor;
    public readonly Color DeleteDragColor;
    public readonly LaneNumberTheme LaneNumbers;
    public readonly LabeledLineTheme BpmChange;
    public readonly LabeledLineTheme Label;
    public readonly FullBeatSnapLineTheme FullBeatSnapLine;
    public readonly LineTheme SubBeatSnapLine;
    public readonly LineTheme CurrentTimeLine;
    public readonly MarkersTheme Markers;
    public readonly BreakpointTheme Breakpoint;
    public readonly NoteLaneThemes NoteLanes;
    public readonly Color NoteDirectionArrowColor;
    public readonly double NoteDirectionArrowScale;
    
    public NoteViewerTheme(NoteViewerThemeJson json, ref List<string> errorMessages) :
        base(json, "noteViewer", ref errorMessages)
    {
        SelectDragColor = ThemeUtils.ParseColor(json.SelectDragColor, "noteViewer.selectDragColor",
                                                ref errorMessages);
        DeleteDragColor = ThemeUtils.ParseColor(json.DeleteDragColor, "noteViewer.deleteDragColor",
                                                ref errorMessages);
        NoteDirectionArrowColor = ThemeUtils.ParseColor(json.NoteDirectionArrowColor,
                                                   "noteViewer.noteDirectionArrowColor",
                                                   ref errorMessages);
        LaneNumbers = new LaneNumberTheme(json.LaneNumbers, ref errorMessages);
        BpmChange = new LabeledLineTheme(json.BpmChanges, "noteViewer.bpmChange",
                                         ref errorMessages);
        Label = new LabeledLineTheme(json.Labels, "noteViewer.label", ref errorMessages);
        FullBeatSnapLine = new FullBeatSnapLineTheme(json.FullBeatSnapLine, ref errorMessages);
        SubBeatSnapLine = new LineTheme(json.SubBeatSnapLine, "noteViewer.subBeatSnapLine",
                                        ref errorMessages);
        CurrentTimeLine = new LineTheme(json.CurrentTimeLine, "noteViewer.currentTimeLine",
                                        ref errorMessages);
        Markers = new MarkersTheme(json.Markers, ref errorMessages);
        Breakpoint = new BreakpointTheme(json.Breakpoint, ref errorMessages);
        NoteLanes = new NoteLaneThemes(json.NoteLanes, ref errorMessages);
        
        NoteDirectionArrowScale = json.NoteDirectionArrowScale;
        if (NoteDirectionArrowScale < 0)
        {
            errorMessages.Add("noteViewer.noteDirectionArrowScale cannot be negative");
        }
    }
}

public class GamePreviewTheme : ElementTheme
{
    public class ViewableAreaTheme
    {
        public readonly Color OutlineColor;
        public readonly double OutlineThickness;
        
        public ViewableAreaTheme(GamePreviewThemeJson.ViewableAreaThemeJson json,
            ref List<string> errorMessages)
        {
            OutlineColor = ThemeUtils.ParseColor(json.OutlineColor,
                                                 "gamePreview.viewableArea.outlineColor",
                                                 ref errorMessages);
            OutlineThickness = json.OutlineThickness;
            
            if (OutlineThickness < 0)
            {
                errorMessages.Add(
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
                GamePreviewThemeJson.NoteTargetsThemeJson.TargetCirclesThemeJson json,
                ref List<string> errorMessages)
            {
                FillColor = ThemeUtils.ParseColor(json.FillColor,
                                                  "gamePreview.noteTargets.circles.fillColor",
                                                  ref errorMessages);
                OutlineColor = ThemeUtils.ParseColor(json.OutlineColor,
                                                     "gamePreview.noteTargets.circles.outlineColor",
                                                     ref errorMessages);
                
                OutlineThickness = json.OutlineThickness;
                Radius = json.Radius;
                if (OutlineThickness < 0)
                {
                    errorMessages.Add(
                        "gamePreview.noteTargets.circles.outlineThickness cannot be negative");
                }
                if (Radius <= 0)
                {
                    errorMessages.Add(
                        "gamePreview.noteTargets.circles.radius must be positive");
                }
            }
        }

        public readonly Color LineColor;
        public readonly double LineThickness;
        public readonly TargetCirclesTheme Circles;
        
        public NoteTargetsTheme(GamePreviewThemeJson.NoteTargetsThemeJson json,
            ref List<string> errorMessages)
        {
            LineColor = ThemeUtils.ParseColor(json.LineColor, "gamePreview.noteTargets.lineColor",
                                              ref errorMessages);
            LineThickness = json.LineThickness;
            Circles = new TargetCirclesTheme(json.TargetCircles, ref errorMessages);

            if (LineThickness < 0)
            {
                errorMessages.Add(
                    "gamePreview.noteTargets.lineThickness cannot be negative");
            }
        }
    }

    public readonly Color CopColor;
    public readonly Color CameraArrowColor;
    public readonly double CameraArrowScale;
    public readonly ViewableAreaTheme ViewableArea;
    public readonly NoteTargetsTheme NoteTargets;
    
    public GamePreviewTheme(GamePreviewThemeJson json, ref List<string> errorMessages) :
        base(json, "gamePreview", ref errorMessages)
    {
        CopColor = ThemeUtils.ParseColor(json.CopColor, "gamePreview.copColor", ref errorMessages);
        CameraArrowColor = ThemeUtils.ParseColor(json.CameraArrowColor,
                                                 "gamePreview.cameraArrowColor", ref errorMessages);
        CameraArrowScale = json.CameraArrowScale;
        ViewableArea = new ViewableAreaTheme(json.ViewableArea, ref errorMessages);
        NoteTargets = new NoteTargetsTheme(json.NoteTargets, ref errorMessages);
        
        if (CameraArrowScale < 0)
        {
            errorMessages.Add("gamePreview.cameraArrowScale cannot be negative");
        }
    }
}

public class PlacementPriorityListTheme : ElementTheme
{
    public class ListEntryTheme(PlacementPriorityListThemeJson.ListEntryThemeJson json,
        ref List<string> errorMessages) : TextElementTheme(json,
                                                           "placementPriorityList.listEntries",
                                                           ref errorMessages)
    {
        public readonly Color ReorderIconColor =
            ThemeUtils.ParseColor(json.ReorderIconColor,
                                  "placementPriorityList.listEntries.reorderIconColor",
                                  ref errorMessages);
    }

    public readonly Color TitleColor;
    public readonly double TitleSize;
    public readonly ListEntryTheme ListEntries;
    
    public PlacementPriorityListTheme(PlacementPriorityListThemeJson json,
        ref List<string> errorMessages) : base(json, "placementPriorityList", ref errorMessages)
    {
        TitleColor = ThemeUtils.ParseColor(json.TitleColor, "placementPriorityList.titleColor",
                                           ref errorMessages);
        TitleSize = json.TitleSize;
        ListEntries = new ListEntryTheme(json.ListEntries, ref errorMessages);

        if (TitleSize <= 0)
        {
            errorMessages.Add("placementPriorityList.titleSize must be positive");
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
    
    public InstantNoteTheme(InstantNoteThemeJson json, string keyName,
        ref List<string> errorMessages)
    {
        FillColor = ThemeUtils.ParseColor(json.FillColor, $"{keyName}.fillColor",
                                          ref errorMessages);
        OutlineColor = ThemeUtils.ParseColor(json.OutlineColor, $"{keyName}.outlineColor",
                                             ref errorMessages);
        OutlineThickness = json.OutlineThickness;

        if (OutlineThickness < 0)
        {
            errorMessages.Add($"{keyName}.outlineThickness cannot be negative");
        }

        if (json.Selected.OutlineThickness < 0 && json.Selected.OutlineThickness != -1)
        {
            errorMessages.Add(
                $"{keyName}.selected.outlineThickness cannot be negative");
        }
        
        Selected = new SelectedTheme(
            json.Selected.FillColor != "" ?
                ThemeUtils.ParseColor(json.Selected.FillColor,
                                      $"{keyName}.selected.fillColor", ref errorMessages) :
                FillColor,
            json.Selected.OutlineColor != "" ?
                ThemeUtils.ParseColor(json.Selected.OutlineColor,
                                      $"{keyName}.selected.outlineColor", ref errorMessages) :
                OutlineColor,
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
    
    public NonInstantNoteTheme(NonInstantNoteThemeJson json, string keyName,
        ref List<string> errorMessages)
    {
        FillColor = ThemeUtils.ParseColor(json.FillColor, $"{keyName}.fillColor",
                                          ref errorMessages);
        OutlineColor = ThemeUtils.ParseColor(json.OutlineColor, $"{keyName}.outlineColor",
                                             ref errorMessages);
        OutlineThickness = json.OutlineThickness;
        TailColor = ThemeUtils.ParseColor(json.TailColor, $"{keyName}.tailColor",
                                          ref errorMessages);
        TailOutlineColor = ThemeUtils.ParseColor(json.TailOutlineColor,
                                                 $"{keyName}.tailOutlineColor", ref errorMessages);
        TailOutlineThickness = json.TailOutlineThickness;

        if (OutlineThickness < 0)
        {
            errorMessages.Add($"{keyName}.outlineThickness cannot be negative");
        }
        
        if (TailOutlineThickness < 0)
        {
            errorMessages.Add($"{keyName}.tailOutlineThickness cannot be negative");
        }
        
        if (json.Selected.OutlineThickness < 0 && json.Selected.OutlineThickness != -1)
        {
            errorMessages.Add(
                $"{keyName}.selected.outlineThickness cannot be negative");
        }
        
        if (json.Selected.TailOutlineThickness < 0 && json.Selected.TailOutlineThickness != -1)
        {
            errorMessages.Add(
                $"{keyName}.selected.tailOutlineThickness cannot be negative");
        }
        
        Selected = new SelectedTheme(
            json.Selected.FillColor != "" ?
                ThemeUtils.ParseColor(json.Selected.FillColor,
                                      $"{keyName}.selected.fillColor", ref errorMessages) :
                FillColor,
            json.Selected.OutlineColor != "" ?
                ThemeUtils.ParseColor(json.Selected.OutlineColor,
                                      $"{keyName}.selected.outlineColor", ref errorMessages) :
                OutlineColor,
            json.Selected.OutlineThickness != -1 ?
                json.Selected.OutlineThickness : OutlineThickness,
            json.Selected.TailColor != "" ?
                ThemeUtils.ParseColor(json.Selected.TailColor,
                                      $"{keyName}.selected.tailColor", ref errorMessages) :
                TailColor,
            json.Selected.TailOutlineColor != "" ?
                ThemeUtils.ParseColor(json.Selected.TailOutlineColor,
                                      $"{keyName}.selected.tailOutlineColor", ref errorMessages) :
                TailOutlineColor,
            json.Selected.OutlineThickness != -1 ?
                json.Selected.OutlineThickness : OutlineThickness);
    }
}

public class NoteThemes(NoteThemesJson json, ref List<string> errorMessages)
{
    public class CommonTheme
    {
        public readonly Color FlagTextColor;
        public readonly Color FlagTextOutlineColor;
        public readonly double FlagTextOutlineThickness;
        public readonly double FlagTextSize;

        public CommonTheme(NoteThemesJson.CommonThemeJson json, ref List<string> errorMessages)
        {
            FlagTextColor = ThemeUtils.ParseColor(json.FlagTextColor, "notes.common.flagTextColor",
                                                  ref errorMessages);
            FlagTextOutlineColor = ThemeUtils.ParseColor(json.FlagTextOutlineColor,
                                                         "notes.common.flagTextOutlineColor",
                                                         ref errorMessages);
            FlagTextOutlineThickness = json.FlagTextOutlineThickness;
            FlagTextSize = json.FlagTextSize;

            if (FlagTextOutlineThickness < 0)
            {
                errorMessages.Add(
                    "notes.common.flagTextOutlineThickness cannot be negative");
            }
            if (FlagTextSize <= 0)
            {
                errorMessages.Add("notes.common.flagTextSize must be positive");
            }
        }
    }
    
    public readonly CommonTheme Common = new(json.Common, ref errorMessages);
    public readonly InstantNoteTheme Single = new(json.Single, "notes.single", ref errorMessages);
    public readonly InstantNoteTheme Spike = new(json.Spike, "notes.spike", ref errorMessages);
    public readonly NonInstantNoteTheme Hold = new(json.Hold, "notes.hold", ref errorMessages);
    public readonly NonInstantNoteTheme Double = new(json.Double, "notes.double",
                                                     ref errorMessages);
    public readonly InstantNoteTheme Freestyle = new(json.Freestyle, "notes.freestyle",
                                                     ref errorMessages);
    public readonly InstantNoteTheme Camera = new (json.Camera, "notes.camera", ref errorMessages);
    public readonly NonInstantNoteTheme Mash = new(json.Mash, "notes.mash", ref errorMessages);
    public readonly NonInstantNoteTheme Cop1 = new(json.Cop1, "notes.cop1", ref errorMessages);
    public readonly NonInstantNoteTheme Cop2 = new(json.Cop2, "notes.cop2", ref errorMessages);
    public readonly NonInstantNoteTheme Cop3 = new(json.Cop3, "notes.cop3", ref errorMessages);
    public readonly NonInstantNoteTheme Cop4 = new(json.Cop4, "notes.cop4", ref errorMessages);
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

    public DebugInfoTheme(DebugInfoThemeJson json, ref List<string> errorMessages)
    {
        OverlayBackgroundColor = ThemeUtils.ParseColor(json.OverlayBackgroundColor,
                                                       "debugInfo.overlayBackgroundColor",
                                                       ref errorMessages);
        OverlayTextColor = ThemeUtils.ParseColor(json.OverlayTextColor,
                                                 "debugInfo.overlayTextColor", ref errorMessages);
        OverlayTextSize = json.OverlayTextSize;
        NoteTimestampTextColor = ThemeUtils.ParseColor(json.NoteTimestampTextColor,
                                                       "debugInfo.noteTimestampTextColor",
                                                       ref errorMessages);
        NoteTimestampTextOutlineColor =
            ThemeUtils.ParseColor(json.NoteTimestampTextOutlineColor,
                                  "debugInfo.noteTimestampTextOutlineColor", ref errorMessages);
        NoteTimestampTextSize = json.NoteTimestampTextSize;
        NoteTimestampTextOutlineThickness = json.NoteTimestampTextOutlineThickness;
        
        if (OverlayTextSize <= 0)
        {
            errorMessages.Add("debugInfo.overlayTextSize must be positive");
        }
        if (NoteTimestampTextSize <= 0)
        {
            errorMessages.Add("debugInfo.noteTimestampTextSize must be positive");
        }
        if (NoteTimestampTextOutlineThickness < 0)
        {
            errorMessages.Add(
                "debugInfo.noteTimestampTextOutlineThickness cannot be negative");
        }
    }
}

public class ColorTheme(ColorThemeJson json, ref List<string> errorMessages)
{
    public readonly MainWindowTheme MainWindow = new(json.MainWindow, ref errorMessages);
    public readonly TopBarTheme TopBar = new(json.TopBar, ref errorMessages);
    public readonly DialogTheme Dialogs = new(json.Dialogs, ref errorMessages);
    public readonly QuickInfoTheme QuickInfo = new(json.QuickInfo, ref errorMessages);
    public readonly NoteViewerTheme NoteViewer = new(json.NoteViewer, ref errorMessages);
    public readonly GamePreviewTheme GamePreview = new(json.GamePreview, ref errorMessages);
    public readonly PlacementPriorityListTheme PlacementPriorityList =
        new(json.PlacementPriorityList, ref errorMessages);
    public readonly NoteThemes Notes = new(json.NoteThemes, ref errorMessages);
    public readonly DebugInfoTheme DebugInfo = new(json.DebugInfo, ref errorMessages);
}