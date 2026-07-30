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

public class AddMarkerAction(List<string> keybinds, int type) : InputActionBase(keybinds)
{
    public override bool CanUseWhilePlaying => Config.Settings.EnableLivePlacement;
    
    public override async Task OnPress()
    {
        ChartBuilder.AddMarker(type);
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