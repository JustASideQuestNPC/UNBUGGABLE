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
    
    private SolidColorBrush _textBackground = new(Colors.Black, 0.5);
    private readonly Typeface _typeface = new((FontFamily)App.Current.Resources["RobotoMono"]);

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
                                """);
        }

        if (column1Strings.Count > 0)
        {
            var column1Text = new FormattedText(string.Join('\n', column1Strings),
                                                CultureInfo.CurrentCulture,
                                                FlowDirection.LeftToRight, _typeface, 14,
                                                Brushes.White);
            dc.DrawRectangle(_textBackground, null, new Rect(0, 0, column1Text.Width + 6,
                                                             column1Text.Height - 4));
            dc.DrawText(column1Text, new Point(0, -12));
        }
    }
}