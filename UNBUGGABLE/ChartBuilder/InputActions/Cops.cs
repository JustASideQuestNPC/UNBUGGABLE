using System.Collections.Generic;
using System.Threading.Tasks;
using UNBEATABLEChartEditor.Input;

namespace UNBUGGABLE.Keybinds;

public class CopId0Action(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.SetCopId(0);
    }
}

public class CopId1Action(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.SetCopId(1);
    }
}

public class CopId2Action(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.SetCopId(2);
    }
}

public class CopId3Action(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.SetCopId(3);
    }
}

public class CopId4Action(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        
        ChartBuilder.SetCopId(4);
    }
}

public class PrevCopAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.PrevCop();
    }
}

public class NextCopAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.NextCop();
    }
}