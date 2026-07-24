using System.Collections.Generic;
using System.Threading.Tasks;
using UNBEATABLEChartEditor.Input;

namespace UNBUGGABLE.Keybinds;

public class AddBpmChangeAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.AddBpmChange();
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
        ChartBuilder.AddLabel();
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