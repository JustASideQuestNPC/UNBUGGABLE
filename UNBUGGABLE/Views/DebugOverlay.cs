using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
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
        if (!Config.Settings.DebugMode)
        {
            return;
        }
        
        var commandInvokerDebug = ChartBuilderCommandInvoker.DebugInfo;
        var undoStackString = commandInvokerDebug.UndoStackNames.Count == 0 ? "empty" :
            string.Join(", ", commandInvokerDebug.UndoStackNames);
        var redoStackString = commandInvokerDebug.RedoStackNames.Count == 0 ? "empty" :
            string.Join(", ", commandInvokerDebug.RedoStackNames);
        
        var column1Str = $"""
                          --- command invoker ---
                          undo stack: {undoStackString}
                          redo stack: {redoStackString}
                          """;
        
        var column1Text = new FormattedText(column1Str, CultureInfo.CurrentCulture,
                                            FlowDirection.LeftToRight, _typeface, 14,
                                            Brushes.White);
        dc.DrawRectangle(_textBackground, null, new Rect(0, 0, column1Text.Width + 6,
                                                         column1Text.Height));
        dc.DrawText(column1Text, new Point(2, -10));
    }
}