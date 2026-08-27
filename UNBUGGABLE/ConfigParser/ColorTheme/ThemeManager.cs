using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.Resources;

public static class ThemeManager
{
    private static Dictionary<string, SolidColorBrush> _themeColors = new();
    private static ColorTheme _currentTheme;
    private static IResourceDictionary _resources;

    public static void Init(IResourceDictionary resources)
    {
        _resources = resources;
        
        _themeColors["MainWindow.BackgroundColor"] = new SolidColorBrush();
        _resources["MainWindow.BackgroundColor"] =
            _themeColors["MainWindow.BackgroundColor"];
        
        _themeColors["MainWindow.EventIndicator.BackgroundColor"] = new SolidColorBrush();
        _resources["MainWindow.EventIndicator.BackgroundColor"] =
            _themeColors["MainWindow.EventIndicator.BackgroundColor"];
        
        _themeColors["MainWindow.EventIndicator.TextColor"] = new SolidColorBrush();
        _resources["MainWindow.EventIndicator.TextColor"] =
            _themeColors["MainWindow.EventIndicator.TextColor"];
        
        _themeColors["TopBar.BackgroundColor"] = new SolidColorBrush();
        _resources["TopBar.BackgroundColor"] =
            _themeColors["TopBar.BackgroundColor"];
        
        _themeColors["TopBar.Sliders.TopColor"] = new SolidColorBrush();
        _resources["TopBar.Sliders.TopColor"] =
            _themeColors["TopBar.Sliders.TopColor"];
        
        _themeColors["TopBar.Sliders.BottomColor"] = new SolidColorBrush();
        _resources["TopBar.Sliders.BottomColor"] =
            _themeColors["TopBar.Sliders.BottomColor"];
        
        _themeColors["TopBar.Sliders.IconColor"] = new SolidColorBrush();
        _resources["TopBar.Sliders.IconColor"] =
            _themeColors["TopBar.Sliders.IconColor"];
        
        _themeColors["TopBar.Sliders.HandleColor"] = new SolidColorBrush();
        _resources["TopBar.Sliders.HandleColor"] =
            _themeColors["TopBar.Sliders.HandleColor"];
        
        _themeColors["TopBar.Buttons.BackgroundColor"] = new SolidColorBrush();
        _resources["TopBar.Buttons.BackgroundColor"] =
            _themeColors["TopBar.Buttons.BackgroundColor"];
        
        _themeColors["TopBar.Buttons.OutlineColor"] = new SolidColorBrush();
        _resources["TopBar.Buttons.OutlineColor"] =
            _themeColors["TopBar.Buttons.OutlineColor"];
        
        _themeColors["TopBar.Buttons.IconColor"] = new SolidColorBrush();
        _resources["TopBar.Buttons.IconColor"] =
            _themeColors["TopBar.Buttons.IconColor"];
        
        _themeColors["TopBar.Buttons.Hovered.BackgroundColor"] = new SolidColorBrush();
        _resources["TopBar.Buttons.Hovered.BackgroundColor"] =
            _themeColors["TopBar.Buttons.Hovered.BackgroundColor"];
        
        _themeColors["TopBar.Buttons.Hovered.OutlineColor"] = new SolidColorBrush();
        _resources["TopBar.Buttons.Hovered.OutlineColor"] =
            _themeColors["TopBar.Buttons.Hovered.OutlineColor"];
        
        _themeColors["TopBar.Buttons.Hovered.IconColor"] = new SolidColorBrush();
        _resources["TopBar.Buttons.Hovered.IconColor"] =
            _themeColors["TopBar.Buttons.Hovered.IconColor"];
        
        _themeColors["TopBar.Tooltips.BackgroundColor"] = new SolidColorBrush();
        _resources["TopBar.Tooltips.BackgroundColor"] =
            _themeColors["TopBar.Tooltips.BackgroundColor"];
        
        _themeColors["TopBar.Tooltips.OutlineColor"] = new SolidColorBrush();
        _resources["TopBar.Tooltips.OutlineColor"] =
            _themeColors["TopBar.Tooltips.OutlineColor"];
        
        _themeColors["TopBar.Tooltips.TextColor"] = new SolidColorBrush();
        _resources["TopBar.Tooltips.TextColor"] =
            _themeColors["TopBar.Tooltips.TextColor"];
        
        _themeColors["TopBar.SaveFileContextMenu.BackgroundColor"] = new SolidColorBrush();
        _resources["TopBar.SaveFileContextMenu.BackgroundColor"] =
            _themeColors["TopBar.SaveFileContextMenu.BackgroundColor"];
        
        _themeColors["TopBar.SaveFileContextMenu.OutlineColor"] = new SolidColorBrush();
        _resources["TopBar.SaveFileContextMenu.OutlineColor"] =
            _themeColors["TopBar.SaveFileContextMenu.OutlineColor"];
        
        _themeColors["TopBar.SaveFileContextMenu.TextColor"] = new SolidColorBrush();
        _resources["TopBar.SaveFileContextMenu.TextColor"] =
            _themeColors["TopBar.SaveFileContextMenu.TextColor"];
        
        _themeColors["Dialogs.BackgroundColor"] = new SolidColorBrush();
        _resources["Dialogs.BackgroundColor"] =
            _themeColors["Dialogs.BackgroundColor"];
        
        _themeColors["Dialogs.OutlineColor"] = new SolidColorBrush();
        _resources["Dialogs.OutlineColor"] =
            _themeColors["Dialogs.OutlineColor"];
        
        _themeColors["Dialogs.TextColor"] = new SolidColorBrush();
        _resources["Dialogs.TextColor"] =
            _themeColors["Dialogs.TextColor"];
        
        _themeColors["Dialogs.InputBoxes.BackgroundColor"] = new SolidColorBrush();
        _resources["Dialogs.InputBoxes.BackgroundColor"] =
            _themeColors["Dialogs.InputBoxes.BackgroundColor"];
        
        _themeColors["Dialogs.InputBoxes.OutlineColor"] = new SolidColorBrush();
        _resources["Dialogs.InputBoxes.OutlineColor"] =
            _themeColors["Dialogs.InputBoxes.OutlineColor"];
        
        _themeColors["Dialogs.InputBoxes.TextColor"] = new SolidColorBrush();
        _resources["Dialogs.InputBoxes.TextColor"] =
            _themeColors["Dialogs.InputBoxes.TextColor"];
        
        _themeColors["Dialogs.Buttons.BackgroundColor"] = new SolidColorBrush();
        _resources["Dialogs.Buttons.BackgroundColor"] =
            _themeColors["Dialogs.Buttons.BackgroundColor"];
        
        _themeColors["Dialogs.Buttons.OutlineColor"] = new SolidColorBrush();
        _resources["Dialogs.Buttons.OutlineColor"] =
            _themeColors["Dialogs.Buttons.OutlineColor"];
        
        _themeColors["Dialogs.Buttons.IconColor"] = new SolidColorBrush();
        _resources["Dialogs.Buttons.IconColor"] =
            _themeColors["Dialogs.Buttons.IconColor"];
        
        _themeColors["Dialogs.Buttons.Hovered.BackgroundColor"] = new SolidColorBrush();
        _resources["Dialogs.Buttons.Hovered.BackgroundColor"] =
            _themeColors["Dialogs.Buttons.Hovered.BackgroundColor"];
        
        _themeColors["Dialogs.Buttons.Hovered.OutlineColor"] = new SolidColorBrush();
        _resources["Dialogs.Buttons.Hovered.OutlineColor"] =
            _themeColors["Dialogs.Buttons.Hovered.OutlineColor"];
        
        _themeColors["Dialogs.Buttons.Hovered.IconColor"] = new SolidColorBrush();
        _resources["Dialogs.Buttons.Hovered.IconColor"] =
            _themeColors["Dialogs.Buttons.Hovered.IconColor"];
        
        _themeColors["QuickInfo.TitleColor"] = new SolidColorBrush();
        _resources["QuickInfo.TitleColor"] =
            _themeColors["QuickInfo.TitleColor"];
        
        _themeColors["QuickInfo.InfoColor"] = new SolidColorBrush();
        _resources["QuickInfo.InfoColor"] =
            _themeColors["QuickInfo.InfoColor"];
        
        _themeColors["NoteViewer.BackgroundColor"] = new SolidColorBrush();
        _resources["NoteViewer.BackgroundColor"] =
            _themeColors["NoteViewer.BackgroundColor"];
        
        _themeColors["NoteViewer.OutlineColor"] = new SolidColorBrush();
        _resources["NoteViewer.OutlineColor"] =
            _themeColors["NoteViewer.OutlineColor"];
        
        _themeColors["NoteViewer.SelectDragColor"] = new SolidColorBrush();
        _resources["NoteViewer.SelectDragColor"] =
            _themeColors["NoteViewer.SelectDragColor"];
        
        _themeColors["NoteViewer.DeleteDragColor"] = new SolidColorBrush();
        _resources["NoteViewer.DeleteDragColor"] =
            _themeColors["NoteViewer.DeleteDragColor"];
        
        _themeColors["NoteViewer.NoteDirectionArrowColor"] = new SolidColorBrush();
        _resources["NoteViewer.NoteDirectionArrowColor"] =
            _themeColors["NoteViewer.NoteDirectionArrowColor"];
        
        _themeColors["NoteViewer.NoteLanes.TopColor"] = new SolidColorBrush();
        _resources["NoteViewer.NoteLanes.TopColor"] =
            _themeColors["NoteViewer.NoteLanes.TopColor"];
        
        _themeColors["NoteViewer.NoteLanes.BottomColor"] = new SolidColorBrush();
        _resources["NoteViewer.NoteLanes.BottomColor"] =
            _themeColors["NoteViewer.NoteLanes.BottomColor"];
        
        _themeColors["NoteViewer.NoteLanes.CenterColor"] = new SolidColorBrush();
        _resources["NoteViewer.NoteLanes.CenterColor"] =
            _themeColors["NoteViewer.NoteLanes.CenterColor"];
        
        _themeColors["NoteViewer.NoteLanes.CameraColor"] = new SolidColorBrush();
        _resources["NoteViewer.NoteLanes.CameraColor"] =
            _themeColors["NoteViewer.NoteLanes.CameraColor"];
        
        _themeColors["NoteViewer.LaneNumbers.Color"] = new SolidColorBrush();
        _resources["NoteViewer.LaneNumbers.Color"] =
            _themeColors["NoteViewer.LaneNumbers.Color"];
        
        _themeColors["NoteViewer.LaneNumbers.OutlineColor"] = new SolidColorBrush();
        _resources["NoteViewer.LaneNumbers.OutlineColor"] =
            _themeColors["NoteViewer.LaneNumbers.OutlineColor"];
        
        _themeColors["NoteViewer.FullBeatSnapLine.Color"] = new SolidColorBrush();
        _resources["NoteViewer.FullBeatSnapLine.Color"] =
            _themeColors["NoteViewer.FullBeatSnapLine.Color"];
        
        _themeColors["NoteViewer.SubBeatSnapLine.Color"] = new SolidColorBrush();
        _resources["NoteViewer.SubBeatSnapLine.Color"] =
            _themeColors["NoteViewer.SubBeatSnapLine.Color"];
        
        _themeColors["NoteViewer.CurrentTimeLine.Color"] = new SolidColorBrush();
        _resources["NoteViewer.CurrentTimeLine.Color"] =
            _themeColors["NoteViewer.CurrentTimeLine.Color"];
        
        _themeColors["NoteViewer.Breakpoint.Color"] = new SolidColorBrush();
        _resources["NoteViewer.Breakpoint.Color"] =
            _themeColors["NoteViewer.Breakpoint.Color"];
        
        _themeColors["NoteViewer.BpmChange.Color"] = new SolidColorBrush();
        _resources["NoteViewer.BpmChange.Color"] =
            _themeColors["NoteViewer.BpmChange.Color"];
        
        _themeColors["NoteViewer.Label.Color"] = new SolidColorBrush();
        _resources["NoteViewer.Label.Color"] =
            _themeColors["NoteViewer.Label.Color"];
        
        _themeColors["NoteViewer.Markers.Color1"] = new SolidColorBrush();
        _resources["NoteViewer.Markers.Color1"] =
            _themeColors["NoteViewer.Markers.Color1"];
        
        _themeColors["NoteViewer.Markers.Color2"] = new SolidColorBrush();
        _resources["NoteViewer.Markers.Color2"] =
            _themeColors["NoteViewer.Markers.Color2"];
        
        _themeColors["NoteViewer.Markers.Color3"] = new SolidColorBrush();
        _resources["NoteViewer.Markers.Color3"] =
            _themeColors["NoteViewer.Markers.Color3"];
        
        _themeColors["GamePreview.BackgroundColor"] = new SolidColorBrush();
        _resources["GamePreview.BackgroundColor"] =
            _themeColors["GamePreview.BackgroundColor"];
        
        _themeColors["GamePreview.OutlineColor"] = new SolidColorBrush();
        _resources["GamePreview.OutlineColor"] =
            _themeColors["GamePreview.OutlineColor"];
        
        _themeColors["GamePreview.CopColor"] = new SolidColorBrush();
        _resources["GamePreview.CopColor"] =
            _themeColors["GamePreview.CopColor"];
        
        _themeColors["GamePreview.ViewableArea.OutlineColor"] = new SolidColorBrush();
        _resources["GamePreview.ViewableArea.OutlineColor"] =
            _themeColors["GamePreview.ViewableArea.OutlineColor"];
        
        _themeColors["GamePreview.CameraArrowColor"] = new SolidColorBrush();
        _resources["GamePreview.CameraArrowColor"] =
            _themeColors["GamePreview.CameraArrowColor"];
        
        _themeColors["GamePreview.NoteTargets.LineColor"] = new SolidColorBrush();
        _resources["GamePreview.NoteTargets.LineColor"] =
            _themeColors["GamePreview.NoteTargets.LineColor"];
        
        _themeColors["GamePreview.NoteTargets.Circles.FillColor"] = new SolidColorBrush();
        _resources["GamePreview.NoteTargets.Circles.FillColor"] =
            _themeColors["GamePreview.NoteTargets.Circles.FillColor"];
        
        _themeColors["GamePreview.NoteTargets.Circles.OutlineColor"] = new SolidColorBrush();
        _resources["GamePreview.NoteTargets.Circles.OutlineColor"] =
            _themeColors["GamePreview.NoteTargets.Circles.OutlineColor"];
        
        _themeColors["PlacementPriorityList.BackgroundColor"] = new SolidColorBrush();
        _resources["PlacementPriorityList.BackgroundColor"] =
            _themeColors["PlacementPriorityList.BackgroundColor"];
        
        _themeColors["PlacementPriorityList.OutlineColor"] = new SolidColorBrush();
        _resources["PlacementPriorityList.OutlineColor"] =
            _themeColors["PlacementPriorityList.OutlineColor"];
        
        _themeColors["PlacementPriorityList.TitleColor"] = new SolidColorBrush();
        _resources["PlacementPriorityList.TitleColor"] =
            _themeColors["PlacementPriorityList.TitleColor"];
        
        _themeColors["PlacementPriorityList.ListEntries.BackgroundColor"] = new SolidColorBrush();
        _resources["PlacementPriorityList.ListEntries.BackgroundColor"] =
            _themeColors["PlacementPriorityList.ListEntries.BackgroundColor"];
        
        _themeColors["PlacementPriorityList.ListEntries.OutlineColor"] = new SolidColorBrush();
        _resources["PlacementPriorityList.ListEntries.OutlineColor"] =
            _themeColors["PlacementPriorityList.ListEntries.OutlineColor"];
        
        _themeColors["PlacementPriorityList.ListEntries.TextColor"] = new SolidColorBrush();
        _resources["PlacementPriorityList.ListEntries.TextColor"] =
            _themeColors["PlacementPriorityList.ListEntries.TextColor"];
        
        _themeColors["PlacementPriorityList.ListEntries.ReorderIconColor"] = new SolidColorBrush();
        _resources["PlacementPriorityList.ListEntries.ReorderIconColor"] =
            _themeColors["PlacementPriorityList.ListEntries.ReorderIconColor"];
        
        _themeColors["Notes.Common.FlagTextColor"] = new SolidColorBrush();
        _resources["Notes.Common.FlagTextColor"] =
            _themeColors["Notes.Common.FlagTextColor"];
        
        _themeColors["Notes.Common.FlagTextOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Common.FlagTextOutlineColor"] =
            _themeColors["Notes.Common.FlagTextOutlineColor"];
        
        _themeColors["Notes.Single.FillColor"] = new SolidColorBrush();
        _resources["Notes.Single.FillColor"] =
            _themeColors["Notes.Single.FillColor"];
        
        _themeColors["Notes.Single.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Single.OutlineColor"] =
            _themeColors["Notes.Single.OutlineColor"];
        
        _themeColors["Notes.Single.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Single.Selected.FillColor"] =
            _themeColors["Notes.Single.Selected.FillColor"];
        
        _themeColors["Notes.Single.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Single.Selected.OutlineColor"] =
            _themeColors["Notes.Single.Selected.OutlineColor"];
        
        _themeColors["Notes.Spike.FillColor"] = new SolidColorBrush();
        _resources["Notes.Spike.FillColor"] =
            _themeColors["Notes.Spike.FillColor"];
        
        _themeColors["Notes.Spike.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Spike.OutlineColor"] =
            _themeColors["Notes.Spike.OutlineColor"];
        
        _themeColors["Notes.Spike.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Spike.Selected.FillColor"] =
            _themeColors["Notes.Spike.Selected.FillColor"];
        
        _themeColors["Notes.Spike.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Spike.Selected.OutlineColor"] =
            _themeColors["Notes.Spike.Selected.OutlineColor"];
        
        _themeColors["Notes.Hold.FillColor"] = new SolidColorBrush();
        _resources["Notes.Hold.FillColor"] =
            _themeColors["Notes.Hold.FillColor"];
        
        _themeColors["Notes.Hold.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Hold.OutlineColor"] =
            _themeColors["Notes.Hold.OutlineColor"];
        
        _themeColors["Notes.Hold.TailColor"] = new SolidColorBrush();
        _resources["Notes.Hold.TailColor"] =
            _themeColors["Notes.Hold.TailColor"];
        
        _themeColors["Notes.Hold.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Hold.TailOutlineColor"] =
            _themeColors["Notes.Hold.TailOutlineColor"];
        
        _themeColors["Notes.Hold.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Hold.Selected.FillColor"] =
            _themeColors["Notes.Hold.Selected.FillColor"];
        
        _themeColors["Notes.Hold.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Hold.Selected.OutlineColor"] =
            _themeColors["Notes.Hold.Selected.OutlineColor"];
        
        _themeColors["Notes.Hold.Selected.TailColor"] = new SolidColorBrush();
        _resources["Notes.Hold.Selected.TailColor"] =
            _themeColors["Notes.Hold.Selected.TailColor"];
        
        _themeColors["Notes.Hold.Selected.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Hold.Selected.TailOutlineColor"] =
            _themeColors["Notes.Hold.Selected.TailOutlineColor"];
        
        _themeColors["Notes.Double.FillColor"] = new SolidColorBrush();
        _resources["Notes.Double.FillColor"] =
            _themeColors["Notes.Double.FillColor"];
        
        _themeColors["Notes.Double.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Double.OutlineColor"] =
            _themeColors["Notes.Double.OutlineColor"];
        
        _themeColors["Notes.Double.TailColor"] = new SolidColorBrush();
        _resources["Notes.Double.TailColor"] =
            _themeColors["Notes.Double.TailColor"];
        
        _themeColors["Notes.Double.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Double.TailOutlineColor"] =
            _themeColors["Notes.Double.TailOutlineColor"];
        
        _themeColors["Notes.Double.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Double.Selected.FillColor"] =
            _themeColors["Notes.Double.Selected.FillColor"];
        
        _themeColors["Notes.Double.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Double.Selected.OutlineColor"] =
            _themeColors["Notes.Double.Selected.OutlineColor"];
        
        _themeColors["Notes.Double.Selected.TailColor"] = new SolidColorBrush();
        _resources["Notes.Double.Selected.TailColor"] =
            _themeColors["Notes.Double.Selected.TailColor"];
        
        _themeColors["Notes.Double.Selected.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Double.Selected.TailOutlineColor"] =
            _themeColors["Notes.Double.Selected.TailOutlineColor"];
        
        _themeColors["Notes.Freestyle.FillColor"] = new SolidColorBrush();
        _resources["Notes.Freestyle.FillColor"] =
            _themeColors["Notes.Freestyle.FillColor"];
        
        _themeColors["Notes.Freestyle.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Freestyle.OutlineColor"] =
            _themeColors["Notes.Freestyle.OutlineColor"];
        
        _themeColors["Notes.Freestyle.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Freestyle.Selected.FillColor"] =
            _themeColors["Notes.Freestyle.Selected.FillColor"];
        
        _themeColors["Notes.Freestyle.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Freestyle.Selected.OutlineColor"] =
            _themeColors["Notes.Freestyle.Selected.OutlineColor"];
        
        _themeColors["Notes.Mash.FillColor"] = new SolidColorBrush();
        _resources["Notes.Mash.FillColor"] =
            _themeColors["Notes.Mash.FillColor"];
        
        _themeColors["Notes.Mash.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Mash.OutlineColor"] =
            _themeColors["Notes.Mash.OutlineColor"];
        
        _themeColors["Notes.Mash.TailColor"] = new SolidColorBrush();
        _resources["Notes.Mash.TailColor"] =
            _themeColors["Notes.Mash.TailColor"];
        
        _themeColors["Notes.Mash.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Mash.TailOutlineColor"] =
            _themeColors["Notes.Mash.TailOutlineColor"];
        
        _themeColors["Notes.Mash.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Mash.Selected.FillColor"] =
            _themeColors["Notes.Mash.Selected.FillColor"];
        
        _themeColors["Notes.Mash.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Mash.Selected.OutlineColor"] =
            _themeColors["Notes.Mash.Selected.OutlineColor"];
        
        _themeColors["Notes.Mash.Selected.TailColor"] = new SolidColorBrush();
        _resources["Notes.Mash.Selected.TailColor"] =
            _themeColors["Notes.Mash.Selected.TailColor"];
        
        _themeColors["Notes.Mash.Selected.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Mash.Selected.TailOutlineColor"] =
            _themeColors["Notes.Mash.Selected.TailOutlineColor"];
        
        _themeColors["Notes.Camera.FillColor"] = new SolidColorBrush();
        _resources["Notes.Camera.FillColor"] =
            _themeColors["Notes.Camera.FillColor"];
        
        _themeColors["Notes.Camera.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Camera.OutlineColor"] =
            _themeColors["Notes.Camera.OutlineColor"];
        
        _themeColors["Notes.Camera.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Camera.Selected.FillColor"] =
            _themeColors["Notes.Camera.Selected.FillColor"];
        
        _themeColors["Notes.Camera.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Camera.Selected.OutlineColor"] =
            _themeColors["Notes.Camera.Selected.OutlineColor"];
        
        _themeColors["Notes.Cop1.FillColor"] = new SolidColorBrush();
        _resources["Notes.Cop1.FillColor"] =
            _themeColors["Notes.Cop1.FillColor"];
        
        _themeColors["Notes.Cop1.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop1.OutlineColor"] =
            _themeColors["Notes.Cop1.OutlineColor"];
        
        _themeColors["Notes.Cop1.TailColor"] = new SolidColorBrush();
        _resources["Notes.Cop1.TailColor"] =
            _themeColors["Notes.Cop1.TailColor"];
        
        _themeColors["Notes.Cop1.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop1.TailOutlineColor"] =
            _themeColors["Notes.Cop1.TailOutlineColor"];
        
        _themeColors["Notes.Cop1.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Cop1.Selected.FillColor"] =
            _themeColors["Notes.Cop1.Selected.FillColor"];
        
        _themeColors["Notes.Cop1.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop1.Selected.OutlineColor"] =
            _themeColors["Notes.Cop1.Selected.OutlineColor"];
        
        _themeColors["Notes.Cop1.Selected.TailColor"] = new SolidColorBrush();
        _resources["Notes.Cop1.Selected.TailColor"] =
            _themeColors["Notes.Cop1.Selected.TailColor"];
        
        _themeColors["Notes.Cop1.Selected.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop1.Selected.TailOutlineColor"] =
            _themeColors["Notes.Cop1.Selected.TailOutlineColor"];
        
        _themeColors["Notes.Cop2.FillColor"] = new SolidColorBrush();
        _resources["Notes.Cop2.FillColor"] =
            _themeColors["Notes.Cop2.FillColor"];
        
        _themeColors["Notes.Cop2.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop2.OutlineColor"] =
            _themeColors["Notes.Cop2.OutlineColor"];
        
        _themeColors["Notes.Cop2.TailColor"] = new SolidColorBrush();
        _resources["Notes.Cop2.TailColor"] =
            _themeColors["Notes.Cop2.TailColor"];
        
        _themeColors["Notes.Cop2.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop2.TailOutlineColor"] =
            _themeColors["Notes.Cop2.TailOutlineColor"];
        
        _themeColors["Notes.Cop2.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Cop2.Selected.FillColor"] =
            _themeColors["Notes.Cop2.Selected.FillColor"];
        
        _themeColors["Notes.Cop2.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop2.Selected.OutlineColor"] =
            _themeColors["Notes.Cop2.Selected.OutlineColor"];
        
        _themeColors["Notes.Cop2.Selected.TailColor"] = new SolidColorBrush();
        _resources["Notes.Cop2.Selected.TailColor"] =
            _themeColors["Notes.Cop2.Selected.TailColor"];
        
        _themeColors["Notes.Cop2.Selected.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop2.Selected.TailOutlineColor"] =
            _themeColors["Notes.Cop2.Selected.TailOutlineColor"];
        
        _themeColors["Notes.Cop3.FillColor"] = new SolidColorBrush();
        _resources["Notes.Cop3.FillColor"] =
            _themeColors["Notes.Cop3.FillColor"];
        
        _themeColors["Notes.Cop3.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop3.OutlineColor"] =
            _themeColors["Notes.Cop3.OutlineColor"];
        
        _themeColors["Notes.Cop3.TailColor"] = new SolidColorBrush();
        _resources["Notes.Cop3.TailColor"] =
            _themeColors["Notes.Cop3.TailColor"];
        
        _themeColors["Notes.Cop3.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop3.TailOutlineColor"] =
            _themeColors["Notes.Cop3.TailOutlineColor"];
        
        _themeColors["Notes.Cop3.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Cop3.Selected.FillColor"] =
            _themeColors["Notes.Cop3.Selected.FillColor"];
        
        _themeColors["Notes.Cop3.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop3.Selected.OutlineColor"] =
            _themeColors["Notes.Cop3.Selected.OutlineColor"];
        
        _themeColors["Notes.Cop3.Selected.TailColor"] = new SolidColorBrush();
        _resources["Notes.Cop3.Selected.TailColor"] =
            _themeColors["Notes.Cop3.Selected.TailColor"];
        
        _themeColors["Notes.Cop3.Selected.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop3.Selected.TailOutlineColor"] =
            _themeColors["Notes.Cop3.Selected.TailOutlineColor"];
        
        _themeColors["Notes.Cop4.FillColor"] = new SolidColorBrush();
        _resources["Notes.Cop4.FillColor"] =
            _themeColors["Notes.Cop4.FillColor"];
        
        _themeColors["Notes.Cop4.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop4.OutlineColor"] =
            _themeColors["Notes.Cop4.OutlineColor"];
        
        _themeColors["Notes.Cop4.TailColor"] = new SolidColorBrush();
        _resources["Notes.Cop4.TailColor"] =
            _themeColors["Notes.Cop4.TailColor"];
        
        _themeColors["Notes.Cop4.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop4.TailOutlineColor"] =
            _themeColors["Notes.Cop4.TailOutlineColor"];
        
        _themeColors["Notes.Cop4.Selected.FillColor"] = new SolidColorBrush();
        _resources["Notes.Cop4.Selected.FillColor"] =
            _themeColors["Notes.Cop4.Selected.FillColor"];
        
        _themeColors["Notes.Cop4.Selected.OutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop4.Selected.OutlineColor"] =
            _themeColors["Notes.Cop4.Selected.OutlineColor"];
        
        _themeColors["Notes.Cop4.Selected.TailColor"] = new SolidColorBrush();
        _resources["Notes.Cop4.Selected.TailColor"] =
            _themeColors["Notes.Cop4.Selected.TailColor"];
        
        _themeColors["Notes.Cop4.Selected.TailOutlineColor"] = new SolidColorBrush();
        _resources["Notes.Cop4.Selected.TailOutlineColor"] =
            _themeColors["Notes.Cop4.Selected.TailOutlineColor"];
        
        _themeColors["DebugInfo.OverlayBackgroundColor"] = new SolidColorBrush();
        _resources["DebugInfo.OverlayBackgroundColor"] =
            _themeColors["DebugInfo.OverlayBackgroundColor"];
        
        _themeColors["DebugInfo.OverlayTextColor"] = new SolidColorBrush();
        _resources["DebugInfo.OverlayTextColor"] =
            _themeColors["DebugInfo.OverlayTextColor"];
        
        _themeColors["DebugInfo.NoteTimestampTextColor"] = new SolidColorBrush();
        _resources["DebugInfo.NoteTimestampTextColor"] =
            _themeColors["DebugInfo.NoteTimestampTextColor"];
        
        _themeColors["DebugInfo.NoteTimestampTextOutlineColor"] = new SolidColorBrush();
        _resources["DebugInfo.NoteTimestampTextOutlineColor"] =
            _themeColors["DebugInfo.NoteTimestampTextOutlineColor"];
    }

    public static void ApplyTheme(ColorTheme theme)
    {
        // swap out colors
        _themeColors["MainWindow.BackgroundColor"].Color =
            theme.MainWindow.BackgroundColor;
        _themeColors["MainWindow.EventIndicator.BackgroundColor"].Color =
            theme.MainWindow.EventIndicator.BackgroundColor;
        _themeColors["MainWindow.EventIndicator.TextColor"].Color =
            theme.MainWindow.EventIndicator.TextColor;
        _themeColors["TopBar.BackgroundColor"].Color =
            theme.TopBar.BackgroundColor;
        _themeColors["TopBar.Sliders.TopColor"].Color =
            theme.TopBar.Sliders.TopColor;
        _themeColors["TopBar.Sliders.BottomColor"].Color =
            theme.TopBar.Sliders.BottomColor;
        _themeColors["TopBar.Sliders.IconColor"].Color =
            theme.TopBar.Sliders.IconColor;
        _themeColors["TopBar.Sliders.HandleColor"].Color =
            theme.TopBar.Sliders.HandleColor;
        _themeColors["TopBar.Buttons.BackgroundColor"].Color =
            theme.TopBar.Buttons.BackgroundColor;
        _themeColors["TopBar.Buttons.OutlineColor"].Color =
            theme.TopBar.Buttons.OutlineColor;
        _themeColors["TopBar.Buttons.IconColor"].Color =
            theme.TopBar.Buttons.IconColor;
        _themeColors["TopBar.Buttons.Hovered.BackgroundColor"].Color =
            theme.TopBar.Buttons.Hovered.BackgroundColor;
        _themeColors["TopBar.Buttons.Hovered.OutlineColor"].Color =
            theme.TopBar.Buttons.Hovered.OutlineColor;
        _themeColors["TopBar.Buttons.Hovered.IconColor"].Color =
            theme.TopBar.Buttons.Hovered.IconColor;
        _themeColors["TopBar.Tooltips.BackgroundColor"].Color =
            theme.TopBar.Tooltips.BackgroundColor;
        _themeColors["TopBar.Tooltips.OutlineColor"].Color =
            theme.TopBar.Tooltips.OutlineColor;
        _themeColors["TopBar.Tooltips.TextColor"].Color =
            theme.TopBar.Tooltips.TextColor;
        _themeColors["TopBar.SaveFileContextMenu.BackgroundColor"].Color =
            theme.TopBar.SaveFileContextMenu.BackgroundColor;
        _themeColors["TopBar.SaveFileContextMenu.OutlineColor"].Color =
            theme.TopBar.SaveFileContextMenu.OutlineColor;
        _themeColors["TopBar.SaveFileContextMenu.TextColor"].Color =
            theme.TopBar.SaveFileContextMenu.TextColor;
        _themeColors["Dialogs.BackgroundColor"].Color =
            theme.Dialogs.BackgroundColor;
        _themeColors["Dialogs.OutlineColor"].Color =
            theme.Dialogs.OutlineColor;
        _themeColors["Dialogs.TextColor"].Color =
            theme.Dialogs.TextColor;
        _themeColors["Dialogs.InputBoxes.BackgroundColor"].Color =
            theme.Dialogs.InputBoxes.BackgroundColor;
        _themeColors["Dialogs.InputBoxes.OutlineColor"].Color =
            theme.Dialogs.InputBoxes.OutlineColor;
        _themeColors["Dialogs.InputBoxes.TextColor"].Color =
            theme.Dialogs.InputBoxes.TextColor;
        _themeColors["Dialogs.Buttons.BackgroundColor"].Color =
            theme.Dialogs.Buttons.BackgroundColor;
        _themeColors["Dialogs.Buttons.OutlineColor"].Color =
            theme.Dialogs.Buttons.OutlineColor;
        _themeColors["Dialogs.Buttons.IconColor"].Color =
            theme.Dialogs.Buttons.IconColor;
        _themeColors["Dialogs.Buttons.Hovered.BackgroundColor"].Color =
            theme.Dialogs.Buttons.Hovered.BackgroundColor;
        _themeColors["Dialogs.Buttons.Hovered.OutlineColor"].Color =
            theme.Dialogs.Buttons.Hovered.OutlineColor;
        _themeColors["Dialogs.Buttons.Hovered.IconColor"].Color =
            theme.Dialogs.Buttons.Hovered.IconColor;
        _themeColors["QuickInfo.TitleColor"].Color =
            theme.QuickInfo.TitleColor;
        _themeColors["QuickInfo.InfoColor"].Color =
            theme.QuickInfo.InfoColor;
        _themeColors["NoteViewer.BackgroundColor"].Color =
            theme.NoteViewer.BackgroundColor;
        _themeColors["NoteViewer.OutlineColor"].Color =
            theme.NoteViewer.OutlineColor;
        _themeColors["NoteViewer.SelectDragColor"].Color =
            theme.NoteViewer.SelectDragColor;
        _themeColors["NoteViewer.DeleteDragColor"].Color =
            theme.NoteViewer.DeleteDragColor;
        // _themeColors["NoteViewer.NoteDirectionArrowColor"].Color =
        //     theme.NoteViewer.NoteDirectionArrowColor;
        _themeColors["NoteViewer.NoteLanes.TopColor"].Color =
            theme.NoteViewer.NoteLanes.TopColor;
        _themeColors["NoteViewer.NoteLanes.BottomColor"].Color =
            theme.NoteViewer.NoteLanes.BottomColor;
        _themeColors["NoteViewer.NoteLanes.CenterColor"].Color =
            theme.NoteViewer.NoteLanes.CenterColor;
        _themeColors["NoteViewer.NoteLanes.CameraColor"].Color =
            theme.NoteViewer.NoteLanes.CameraColor;
        _themeColors["NoteViewer.LaneNumbers.Color"].Color =
            theme.NoteViewer.LaneNumbers.Color;
        _themeColors["NoteViewer.LaneNumbers.OutlineColor"].Color =
            theme.NoteViewer.LaneNumbers.OutlineColor;
        _themeColors["NoteViewer.FullBeatSnapLine.Color"].Color =
            theme.NoteViewer.FullBeatSnapLine.Color;
        _themeColors["NoteViewer.SubBeatSnapLine.Color"].Color =
            theme.NoteViewer.SubBeatSnapLine.Color;
        _themeColors["NoteViewer.CurrentTimeLine.Color"].Color =
            theme.NoteViewer.CurrentTimeLine.Color;
        _themeColors["NoteViewer.Breakpoint.Color"].Color =
            theme.NoteViewer.Breakpoint.Color;
        _themeColors["NoteViewer.BpmChange.Color"].Color =
            theme.NoteViewer.BpmChange.Color;
        _themeColors["NoteViewer.Label.Color"].Color =
            theme.NoteViewer.Label.Color;
        _themeColors["NoteViewer.Markers.Color1"].Color =
            theme.NoteViewer.Markers.Color1;
        _themeColors["NoteViewer.Markers.Color2"].Color =
            theme.NoteViewer.Markers.Color2;
        _themeColors["NoteViewer.Markers.Color3"].Color =
            theme.NoteViewer.Markers.Color3;
        _themeColors["GamePreview.BackgroundColor"].Color =
            theme.GamePreview.BackgroundColor;
        _themeColors["GamePreview.OutlineColor"].Color =
            theme.GamePreview.OutlineColor;
        _themeColors["GamePreview.CopColor"].Color =
            theme.GamePreview.CopColor;
        _themeColors["GamePreview.ViewableArea.OutlineColor"].Color =
            theme.GamePreview.ViewableArea.OutlineColor;
        _themeColors["GamePreview.CameraArrowColor"].Color =
            theme.GamePreview.CameraArrowColor;
        _themeColors["GamePreview.NoteTargets.LineColor"].Color =
            theme.GamePreview.NoteTargets.LineColor;
        _themeColors["GamePreview.NoteTargets.Circles.FillColor"].Color =
            theme.GamePreview.NoteTargets.Circles.FillColor;
        _themeColors["GamePreview.NoteTargets.Circles.OutlineColor"].Color =
            theme.GamePreview.NoteTargets.Circles.OutlineColor;
        _themeColors["PlacementPriorityList.BackgroundColor"].Color =
            theme.PlacementPriorityList.BackgroundColor;
        _themeColors["PlacementPriorityList.OutlineColor"].Color =
            theme.PlacementPriorityList.OutlineColor;
        _themeColors["PlacementPriorityList.TitleColor"].Color =
            theme.PlacementPriorityList.TitleColor;
        _themeColors["PlacementPriorityList.ListEntries.BackgroundColor"].Color =
            theme.PlacementPriorityList.ListEntries.BackgroundColor;
        _themeColors["PlacementPriorityList.ListEntries.OutlineColor"].Color =
            theme.PlacementPriorityList.ListEntries.OutlineColor;
        _themeColors["PlacementPriorityList.ListEntries.TextColor"].Color =
            theme.PlacementPriorityList.ListEntries.TextColor;
        _themeColors["PlacementPriorityList.ListEntries.ReorderIconColor"].Color =
            theme.PlacementPriorityList.ListEntries.ReorderIconColor;
        _themeColors["Notes.Common.FlagTextColor"].Color =
            theme.Notes.Common.FlagTextColor;
        _themeColors["Notes.Common.FlagTextOutlineColor"].Color =
            theme.Notes.Common.FlagTextOutlineColor;
        _themeColors["Notes.Single.FillColor"].Color =
            theme.Notes.Single.FillColor;
        _themeColors["Notes.Single.OutlineColor"].Color =
            theme.Notes.Single.OutlineColor;
        _themeColors["Notes.Single.Selected.FillColor"].Color =
            theme.Notes.Single.Selected.FillColor;
        _themeColors["Notes.Single.Selected.OutlineColor"].Color =
            theme.Notes.Single.Selected.OutlineColor;
        _themeColors["Notes.Spike.FillColor"].Color =
            theme.Notes.Spike.FillColor;
        _themeColors["Notes.Spike.OutlineColor"].Color =
            theme.Notes.Spike.OutlineColor;
        _themeColors["Notes.Spike.Selected.FillColor"].Color =
            theme.Notes.Spike.Selected.FillColor;
        _themeColors["Notes.Spike.Selected.OutlineColor"].Color =
            theme.Notes.Spike.Selected.OutlineColor;
        _themeColors["Notes.Hold.FillColor"].Color =
            theme.Notes.Hold.FillColor;
        _themeColors["Notes.Hold.OutlineColor"].Color =
            theme.Notes.Hold.OutlineColor;
        _themeColors["Notes.Hold.TailColor"].Color =
            theme.Notes.Hold.TailColor;
        _themeColors["Notes.Hold.TailOutlineColor"].Color =
            theme.Notes.Hold.TailOutlineColor;
        _themeColors["Notes.Hold.Selected.FillColor"].Color =
            theme.Notes.Hold.Selected.FillColor;
        _themeColors["Notes.Hold.Selected.OutlineColor"].Color =
            theme.Notes.Hold.Selected.OutlineColor;
        _themeColors["Notes.Hold.Selected.TailColor"].Color =
            theme.Notes.Hold.Selected.TailColor;
        _themeColors["Notes.Hold.Selected.TailOutlineColor"].Color =
            theme.Notes.Hold.Selected.TailOutlineColor;
        _themeColors["Notes.Double.FillColor"].Color =
            theme.Notes.Double.FillColor;
        _themeColors["Notes.Double.OutlineColor"].Color =
            theme.Notes.Double.OutlineColor;
        _themeColors["Notes.Double.TailColor"].Color =
            theme.Notes.Double.TailColor;
        _themeColors["Notes.Double.TailOutlineColor"].Color =
            theme.Notes.Double.TailOutlineColor;
        _themeColors["Notes.Double.Selected.FillColor"].Color =
            theme.Notes.Double.Selected.FillColor;
        _themeColors["Notes.Double.Selected.OutlineColor"].Color =
            theme.Notes.Double.Selected.OutlineColor;
        _themeColors["Notes.Double.Selected.TailColor"].Color =
            theme.Notes.Double.Selected.TailColor;
        _themeColors["Notes.Double.Selected.TailOutlineColor"].Color =
            theme.Notes.Double.Selected.TailOutlineColor;
        _themeColors["Notes.Freestyle.FillColor"].Color =
            theme.Notes.Freestyle.FillColor;
        _themeColors["Notes.Freestyle.OutlineColor"].Color =
            theme.Notes.Freestyle.OutlineColor;
        _themeColors["Notes.Freestyle.Selected.FillColor"].Color =
            theme.Notes.Freestyle.Selected.FillColor;
        _themeColors["Notes.Freestyle.Selected.OutlineColor"].Color =
            theme.Notes.Freestyle.Selected.OutlineColor;
        _themeColors["Notes.Mash.FillColor"].Color =
            theme.Notes.Mash.FillColor;
        _themeColors["Notes.Mash.OutlineColor"].Color =
            theme.Notes.Mash.OutlineColor;
        _themeColors["Notes.Mash.TailColor"].Color =
            theme.Notes.Mash.TailColor;
        _themeColors["Notes.Mash.TailOutlineColor"].Color =
            theme.Notes.Mash.TailOutlineColor;
        _themeColors["Notes.Mash.Selected.FillColor"].Color =
            theme.Notes.Mash.Selected.FillColor;
        _themeColors["Notes.Mash.Selected.OutlineColor"].Color =
            theme.Notes.Mash.Selected.OutlineColor;
        _themeColors["Notes.Mash.Selected.TailColor"].Color =
            theme.Notes.Mash.Selected.TailColor;
        _themeColors["Notes.Mash.Selected.TailOutlineColor"].Color =
            theme.Notes.Mash.Selected.TailOutlineColor;
        _themeColors["Notes.Camera.FillColor"].Color =
            theme.Notes.Camera.FillColor;
        _themeColors["Notes.Camera.OutlineColor"].Color =
            theme.Notes.Camera.OutlineColor;
        _themeColors["Notes.Camera.Selected.FillColor"].Color =
            theme.Notes.Camera.Selected.FillColor;
        _themeColors["Notes.Camera.Selected.OutlineColor"].Color =
            theme.Notes.Camera.Selected.OutlineColor;
        _themeColors["Notes.Cop1.FillColor"].Color =
            theme.Notes.Cop1.FillColor;
        _themeColors["Notes.Cop1.OutlineColor"].Color =
            theme.Notes.Cop1.OutlineColor;
        _themeColors["Notes.Cop1.TailColor"].Color =
            theme.Notes.Cop1.TailColor;
        _themeColors["Notes.Cop1.TailOutlineColor"].Color =
            theme.Notes.Cop1.TailOutlineColor;
        _themeColors["Notes.Cop1.Selected.FillColor"].Color =
            theme.Notes.Cop1.Selected.FillColor;
        _themeColors["Notes.Cop1.Selected.OutlineColor"].Color =
            theme.Notes.Cop1.Selected.OutlineColor;
        _themeColors["Notes.Cop1.Selected.TailColor"].Color =
            theme.Notes.Cop1.Selected.TailColor;
        _themeColors["Notes.Cop1.Selected.TailOutlineColor"].Color =
            theme.Notes.Cop1.Selected.TailOutlineColor;
        _themeColors["Notes.Cop2.FillColor"].Color =
            theme.Notes.Cop2.FillColor;
        _themeColors["Notes.Cop2.OutlineColor"].Color =
            theme.Notes.Cop2.OutlineColor;
        _themeColors["Notes.Cop2.TailColor"].Color =
            theme.Notes.Cop2.TailColor;
        _themeColors["Notes.Cop2.TailOutlineColor"].Color =
            theme.Notes.Cop2.TailOutlineColor;
        _themeColors["Notes.Cop2.Selected.FillColor"].Color =
            theme.Notes.Cop2.Selected.FillColor;
        _themeColors["Notes.Cop2.Selected.OutlineColor"].Color =
            theme.Notes.Cop2.Selected.OutlineColor;
        _themeColors["Notes.Cop2.Selected.TailColor"].Color =
            theme.Notes.Cop2.Selected.TailColor;
        _themeColors["Notes.Cop2.Selected.TailOutlineColor"].Color =
            theme.Notes.Cop2.Selected.TailOutlineColor;
        _themeColors["Notes.Cop3.FillColor"].Color =
            theme.Notes.Cop3.FillColor;
        _themeColors["Notes.Cop3.OutlineColor"].Color =
            theme.Notes.Cop3.OutlineColor;
        _themeColors["Notes.Cop3.TailColor"].Color =
            theme.Notes.Cop3.TailColor;
        _themeColors["Notes.Cop3.TailOutlineColor"].Color =
            theme.Notes.Cop3.TailOutlineColor;
        _themeColors["Notes.Cop3.Selected.FillColor"].Color =
            theme.Notes.Cop3.Selected.FillColor;
        _themeColors["Notes.Cop3.Selected.OutlineColor"].Color =
            theme.Notes.Cop3.Selected.OutlineColor;
        _themeColors["Notes.Cop3.Selected.TailColor"].Color =
            theme.Notes.Cop3.Selected.TailColor;
        _themeColors["Notes.Cop3.Selected.TailOutlineColor"].Color =
            theme.Notes.Cop3.Selected.TailOutlineColor;
        _themeColors["Notes.Cop4.FillColor"].Color =
            theme.Notes.Cop4.FillColor;
        _themeColors["Notes.Cop4.OutlineColor"].Color =
            theme.Notes.Cop4.OutlineColor;
        _themeColors["Notes.Cop4.TailColor"].Color =
            theme.Notes.Cop4.TailColor;
        _themeColors["Notes.Cop4.TailOutlineColor"].Color =
            theme.Notes.Cop4.TailOutlineColor;
        _themeColors["Notes.Cop4.Selected.FillColor"].Color =
            theme.Notes.Cop4.Selected.FillColor;
        _themeColors["Notes.Cop4.Selected.OutlineColor"].Color =
            theme.Notes.Cop4.Selected.OutlineColor;
        _themeColors["Notes.Cop4.Selected.TailColor"].Color =
            theme.Notes.Cop4.Selected.TailColor;
        _themeColors["Notes.Cop4.Selected.TailOutlineColor"].Color =
            theme.Notes.Cop4.Selected.TailOutlineColor;
        _themeColors["DebugInfo.OverlayBackgroundColor"].Color =
            theme.DebugInfo.OverlayBackgroundColor;
        _themeColors["DebugInfo.OverlayTextColor"].Color =
            theme.DebugInfo.OverlayTextColor;
        _themeColors["DebugInfo.NoteTimestampTextColor"].Color =
            theme.DebugInfo.NoteTimestampTextColor;
        _themeColors["DebugInfo.NoteTimestampTextOutlineColor"].Color =
            theme.DebugInfo.NoteTimestampTextOutlineColor;
        
        // swap out numbers (text size, border radius, etc.)
        _resources["MainWindow.EventIndicator.TextSize"] =
            theme.MainWindow.EventIndicator.TextSize;
        _resources["TopBar.Sliders.TopThickness"] =
            theme.TopBar.Sliders.TopThickness;
        _resources["TopBar.Sliders.BottomThickness"] =
            theme.TopBar.Sliders.BottomThickness;
        _resources["TopBar.Sliders.HandleWidth"] =
            theme.TopBar.Sliders.HandleWidth;
        _resources["TopBar.Sliders.HandleHeight"] =
            theme.TopBar.Sliders.HandleHeight;
        _resources["TopBar.Buttons.OutlineThickness"] =
            new Thickness(theme.TopBar.Buttons.OutlineThickness);
        _resources["TopBar.Buttons.CornerRadius"] =
            new CornerRadius(theme.TopBar.Buttons.CornerRadius);
        _resources["TopBar.Tooltips.OutlineThickness"] =
            new Thickness(theme.TopBar.Tooltips.OutlineThickness);
        _resources["TopBar.Tooltips.CornerRadius"] =
            new CornerRadius(theme.TopBar.Tooltips.CornerRadius);
        _resources["TopBar.Tooltips.TextSize"] =
            theme.TopBar.Tooltips.TextSize;
        _resources["TopBar.SaveFileContextMenu.OutlineThickness"] =
            new Thickness(theme.TopBar.SaveFileContextMenu.OutlineThickness);
        _resources["TopBar.SaveFileContextMenu.CornerRadius"] =
            new CornerRadius(theme.TopBar.SaveFileContextMenu.CornerRadius);
        _resources["TopBar.SaveFileContextMenu.TextSize"] =
            theme.TopBar.SaveFileContextMenu.TextSize;
        _resources["Dialogs.OutlineThickness"] =
            new Thickness(theme.Dialogs.OutlineThickness);
        _resources["Dialogs.CornerRadius"] =
            new CornerRadius(theme.Dialogs.CornerRadius);
        _resources["Dialogs.TextSize"] =
            theme.Dialogs.TextSize;
        _resources["Dialogs.InputBoxes.OutlineThickness"] =
            new Thickness(theme.Dialogs.InputBoxes.OutlineThickness);
        _resources["Dialogs.InputBoxes.CornerRadius"] =
            new CornerRadius(theme.Dialogs.InputBoxes.CornerRadius);
        _resources["Dialogs.InputBoxes.TextSize"] =
            theme.Dialogs.InputBoxes.TextSize;
        _resources["Dialogs.Buttons.OutlineThickness"] =
            new Thickness(theme.Dialogs.Buttons.OutlineThickness);
        _resources["Dialogs.Buttons.CornerRadius"] =
            new CornerRadius(theme.Dialogs.Buttons.CornerRadius);
        _resources["QuickInfo.TitleSize"] =
            theme.QuickInfo.TitleSize;
        _resources["QuickInfo.InfoSize"] =
            theme.QuickInfo.InfoSize;
        _resources["NoteViewer.OutlineThickness"] =
            new Thickness(theme.NoteViewer.OutlineThickness);
        _resources["NoteViewer.CornerRadius"] =
            new CornerRadius(theme.NoteViewer.CornerRadius);
        _resources["NoteViewer.LaneNumbers.OutlineThickness"] =
            new Thickness(theme.NoteViewer.LaneNumbers.OutlineThickness);
        // _resources["NoteViewer.NoteDirectionArrowScale"] =
        //     theme.NoteViewer.NoteDirectionArrowScale;
        _resources["NoteViewer.NoteLanes.TopWidth"] =
            theme.NoteViewer.NoteLanes.TopWidth;
        _resources["NoteViewer.NoteLanes.BottomWidth"] =
            theme.NoteViewer.NoteLanes.BottomWidth;
        _resources["NoteViewer.NoteLanes.CenterWidth"] =
            theme.NoteViewer.NoteLanes.CenterWidth;
        _resources["NoteViewer.NoteLanes.CameraWidth"] =
            theme.NoteViewer.NoteLanes.CameraWidth;
        _resources["NoteViewer.LaneNumbers.TextSize"] =
            theme.NoteViewer.LaneNumbers.TextSize;
        _resources["NoteViewer.FullBeatSnapLine.Thickness"] =
            theme.NoteViewer.FullBeatSnapLine.Thickness;
        _resources["NoteViewer.FullBeatSnapLine.TextSize"] =
            theme.NoteViewer.FullBeatSnapLine.TextSize;
        _resources["NoteViewer.SubBeatSnapLine.Thickness"] =
            theme.NoteViewer.SubBeatSnapLine.Thickness;
        _resources["NoteViewer.CurrentTimeLine.Thickness"] =
            theme.NoteViewer.CurrentTimeLine.Thickness;
        _resources["NoteViewer.Breakpoint.Thickness"] =
            theme.NoteViewer.Breakpoint.Thickness;
        _resources["NoteViewer.Breakpoint.ArrowScale"] =
            theme.NoteViewer.Breakpoint.ArrowScale;
        _resources["NoteViewer.BpmChange.LineThickness"] =
            theme.NoteViewer.BpmChange.LineThickness;
        _resources["NoteViewer.BpmChange.TextSize"] =
            theme.NoteViewer.BpmChange.TextSize;
        _resources["NoteViewer.Label.LineThickness"] =
            theme.NoteViewer.Label.LineThickness;
        _resources["NoteViewer.Label.TextSize"] =
            theme.NoteViewer.Label.TextSize;
        _resources["NoteViewer.Markers.ArrowScale"] =
            theme.NoteViewer.Markers.ArrowScale;
        _resources["GamePreview.OutlineThickness"] =
            new Thickness(theme.GamePreview.OutlineThickness);
        _resources["GamePreview.CornerRadius"] =
            new CornerRadius(theme.GamePreview.CornerRadius);
        _resources["GamePreview.ViewableArea.OutlineThickness"] =
            new Thickness(theme.GamePreview.ViewableArea.OutlineThickness);
        _resources["GamePreview.CameraArrowScale"] =
            theme.GamePreview.CameraArrowScale;
        _resources["GamePreview.NoteTargets.LineThickness"] =
            theme.GamePreview.NoteTargets.LineThickness;
        _resources["GamePreview.NoteTargets.Circles.Radius"] =
            theme.GamePreview.NoteTargets.Circles.Radius;
        _resources["GamePreview.NoteTargets.Circles.OutlineThickness"] =
            new Thickness(theme.GamePreview.NoteTargets.Circles.OutlineThickness);
        _resources["PlacementPriorityList.OutlineThickness"] =
            new Thickness(theme.PlacementPriorityList.OutlineThickness);
        _resources["PlacementPriorityList.CornerRadius"] =
            new CornerRadius(theme.PlacementPriorityList.CornerRadius);
        _resources["PlacementPriorityList.TitleSize"] =
            theme.PlacementPriorityList.TitleSize;
        _resources["PlacementPriorityList.ListEntries.OutlineThickness"] =
            new Thickness(theme.PlacementPriorityList.ListEntries.OutlineThickness);
        _resources["PlacementPriorityList.ListEntries.CornerRadius"] =
            new CornerRadius(theme.PlacementPriorityList.ListEntries.CornerRadius);
        _resources["PlacementPriorityList.ListEntries.TextSize"] =
            theme.PlacementPriorityList.ListEntries.TextSize;
        _resources["Notes.Common.FlagTextSize"] =
            theme.Notes.Common.FlagTextSize;
        _resources["Notes.Common.FlagTextOutlineThickness"] =
            new Thickness(theme.Notes.Common.FlagTextOutlineThickness);
        _resources["Notes.Single.OutlineThickness"] =
            new Thickness(theme.Notes.Single.OutlineThickness);
        _resources["Notes.Single.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Single.Selected.OutlineThickness);
        _resources["Notes.Spike.OutlineThickness"] =
            new Thickness(theme.Notes.Spike.OutlineThickness);
        _resources["Notes.Spike.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Spike.Selected.OutlineThickness);
        _resources["Notes.Hold.OutlineThickness"] =
            new Thickness(theme.Notes.Hold.OutlineThickness);
        _resources["Notes.Hold.TailOutlineThickness"] =
            new Thickness(theme.Notes.Hold.TailOutlineThickness);
        _resources["Notes.Hold.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Hold.Selected.OutlineThickness);
        _resources["Notes.Hold.Selected.TailOutlineThickness"] =
            new Thickness(theme.Notes.Hold.Selected.TailOutlineThickness);
        _resources["Notes.Double.OutlineThickness"] =
            new Thickness(theme.Notes.Double.OutlineThickness);
        _resources["Notes.Double.TailOutlineThickness"] =
            new Thickness(theme.Notes.Double.TailOutlineThickness);
        _resources["Notes.Double.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Double.Selected.OutlineThickness);
        _resources["Notes.Double.Selected.TailOutlineThickness"] =
            new Thickness(theme.Notes.Double.Selected.TailOutlineThickness);
        _resources["Notes.Freestyle.OutlineThickness"] =
            new Thickness(theme.Notes.Freestyle.OutlineThickness);
        _resources["Notes.Freestyle.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Freestyle.Selected.OutlineThickness);
        _resources["Notes.Mash.OutlineThickness"] =
            new Thickness(theme.Notes.Mash.OutlineThickness);
        _resources["Notes.Mash.TailOutlineThickness"] =
            new Thickness(theme.Notes.Mash.TailOutlineThickness);
        _resources["Notes.Mash.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Mash.Selected.OutlineThickness);
        _resources["Notes.Mash.Selected.TailOutlineThickness"] =
            new Thickness(theme.Notes.Mash.Selected.TailOutlineThickness);
        _resources["Notes.Camera.OutlineThickness"] =
            new Thickness(theme.Notes.Camera.OutlineThickness);
        _resources["Notes.Camera.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Camera.Selected.OutlineThickness);
        _resources["Notes.Cop1.OutlineThickness"] =
            new Thickness(theme.Notes.Cop1.OutlineThickness);
        _resources["Notes.Cop1.TailOutlineThickness"] =
            new Thickness(theme.Notes.Cop1.TailOutlineThickness);
        _resources["Notes.Cop1.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Cop1.Selected.OutlineThickness);
        _resources["Notes.Cop1.Selected.TailOutlineThickness"] =
            new Thickness(theme.Notes.Cop1.Selected.TailOutlineThickness);
        _resources["Notes.Cop2.OutlineThickness"] =
            new Thickness(theme.Notes.Cop2.OutlineThickness);
        _resources["Notes.Cop2.TailOutlineThickness"] =
            new Thickness(theme.Notes.Cop2.TailOutlineThickness);
        _resources["Notes.Cop2.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Cop2.Selected.OutlineThickness);
        _resources["Notes.Cop2.Selected.TailOutlineThickness"] =
            new Thickness(theme.Notes.Cop2.Selected.TailOutlineThickness);
        _resources["Notes.Cop3.OutlineThickness"] =
            new Thickness(theme.Notes.Cop3.OutlineThickness);
        _resources["Notes.Cop3.TailOutlineThickness"] =
            new Thickness(theme.Notes.Cop3.TailOutlineThickness);
        _resources["Notes.Cop3.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Cop3.Selected.OutlineThickness);
        _resources["Notes.Cop3.Selected.TailOutlineThickness"] =
            new Thickness(theme.Notes.Cop3.Selected.TailOutlineThickness);
        _resources["Notes.Cop4.OutlineThickness"] =
            new Thickness(theme.Notes.Cop4.OutlineThickness);
        _resources["Notes.Cop4.TailOutlineThickness"] =
            new Thickness(theme.Notes.Cop4.TailOutlineThickness);
        _resources["Notes.Cop4.Selected.OutlineThickness"] =
            new Thickness(theme.Notes.Cop4.Selected.OutlineThickness);
        _resources["Notes.Cop4.Selected.TailOutlineThickness"] =
            new Thickness(theme.Notes.Cop4.Selected.TailOutlineThickness);
        _resources["DebugInfo.OverlayTextSize"] =
            theme.DebugInfo.OverlayTextSize;
        _resources["DebugInfo.NoteTimestampTextOutlineThickness"] =
            new Thickness(theme.DebugInfo.NoteTimestampTextOutlineThickness);
        _resources["DebugInfo.NoteTimestampTextSize"] =
            theme.DebugInfo.NoteTimestampTextSize;
        
        // update code rendering colors
        CameraChange.UpdateStyles();
        CopNote.UpdateStyles();
        FreestyleNote.UpdateStyles();
        HoldNote.UpdateStyles();
        MarkerNote.UpdateStyles();
        MashNote.UpdateStyles();
        SingleNote.UpdateStyles();
        DebugOverlay.UpdateStyles();
        GamePreview.UpdateStyles();
        NoteViewer.UpdateStyles();
    }
}