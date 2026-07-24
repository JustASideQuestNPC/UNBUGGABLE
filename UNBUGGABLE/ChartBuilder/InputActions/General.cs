using System.Collections.Generic;
using System.Threading.Tasks;
using UNBEATABLEChartEditor.Input;

namespace UNBUGGABLE.Keybinds;

public class UndoAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.Undo();
    }
}

public class RedoAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.Redo();
    }
}

public class SaveFileAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        App.MainWindowViewModel.DefaultSaveCommand.Execute(null);
    }
}

public class OpenFileAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        App.MainWindowViewModel.LoadFileCommand.Execute(null);
    }
}

public class ResetPlaySpeedAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        App.MainWindowViewModel.ResetPlaySpeedCommand.Execute(null);
    }
}