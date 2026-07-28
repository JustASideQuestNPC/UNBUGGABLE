using System.Collections.Generic;
using System.Threading.Tasks;
using UNBEATABLEChartEditor;
using UNBEATABLEChartEditor.Input;
using UNBUGGABLE.Resources;

namespace UNBUGGABLE.Keybinds;

public class AddBpmChangeAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        await ChartBuilder.AddBpmChange();
    }
}

public class RemoveBpmChangeAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.RemoveBpmChange();
    }
}

public class AddLabelAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        await ChartBuilder.AddLabel();
    }
}

public class RemoveLabelAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.RemoveLabel();
    }
}

public class AddMarker1Action(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.AddMarker(0);
    }
}

public class AddMarker2Action(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.AddMarker(1);
    }
}

public class AddMarker3Action(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.AddMarker(2);
    }
}

public class SetBreakpointAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.SetBreakpoint();
    }
}

public class RemoveBreakpointAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.RemoveBreakpoint();
    }
}

public class EmergencyReloadAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        if (Config.Settings.DefaultSaveToBeatFiles)
        {
            await ChartBuilder.SaveToBeatPath(UserData.LastOpenedChartFile);
        }
        else
        {
            await ChartBuilder.SaveToStandardPath(UserData.LastOpenedChartFile);
        }
        await ChartBuilder.TryLoadChartFile(UserData.LastOpenedChartFile);
    }
}