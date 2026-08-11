using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using UNBEATABLEChartEditor.Input;
using UNBUGGABLE.Resources;

namespace UNBUGGABLE.Views;

public class DebugOverlay : Control
{
    private double _width;
    private double _height;

    private static SolidColorBrush BackgroundBrush;
    private static SolidColorBrush TextBrush;
    private static double TextSize;
    private static readonly Typeface Typeface =
        new((FontFamily)App.Current.Resources["RobotoMono"]);

    public static void UpdateStyles()
    {
        BackgroundBrush =
            (SolidColorBrush)App.Current.Resources["DebugInfo.OverlayBackgroundColor"];
        TextBrush = (SolidColorBrush)App.Current.Resources["DebugInfo.OverlayTextColor"];
        TextSize = (double)App.Current.Resources["DebugInfo.OverlayTextSize"];
    }

    public override void Render(DrawingContext dc)
    {
        if (!Config.Settings.DebugToggles.Enabled)
        {
            return;
        }

        List<string> column1Strings = [];

        if (Config.Settings.DebugToggles.CommandStacks)
        {
            var commandInvokerDebug = ChartBuilderCommandInvoker.DebugInfo;
            var undoStackString = commandInvokerDebug.UndoStackNames.Count == 0 ? "empty" :
                string.Join(", ", commandInvokerDebug.UndoStackNames);
            var redoStackString = commandInvokerDebug.RedoStackNames.Count == 0 ? "empty" :
                string.Join(", ", commandInvokerDebug.RedoStackNames);
            column1Strings.Add($"""
                                --- command invoker ---
                                undo stack: {undoStackString}
                                redo stack: {redoStackString}
                                """);
        }

        if (Config.Settings.DebugToggles.InputData)
        {
            column1Strings.Add($"""
                                --- key states ---
                                ctrl: {InputManager.CtrlPressed}
                                shift: {InputManager.ShiftPressed}
                                alt: {InputManager.AltPressed}
                                last pressed: {InputManager.LastPressedKey}
                                """);
        }

        if (Config.Settings.DebugToggles.MediaPlayer)
        {
            var chartDebug = Chart.DebugInfo;
            column1Strings.Add($"""
                                --- chart ---
                                playing: {chartDebug.Playing}
                                song loaded: {chartDebug.SongLoaded}
                                media time: {chartDebug.MediaPlayerTime}
                                media state: {chartDebug.MediaPlayerState}
                                last vlc output: {chartDebug.LastVlcOutput}
                                chart time: {chartDebug.ChartTime}
                                play speed: {chartDebug.PlaySpeed}
                                """);
        }

        if (column1Strings.Count > 0)
        {
            var column1Text = new FormattedText(string.Join('\n', column1Strings),
                                                CultureInfo.CurrentCulture,
                                                FlowDirection.LeftToRight, Typeface, TextSize,
                                                TextBrush);
            dc.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, column1Text.Width + 6, 
                                                             column1Text.Height - 4));
            dc.DrawText(column1Text, new Point(0, -12));
        }
    }
}