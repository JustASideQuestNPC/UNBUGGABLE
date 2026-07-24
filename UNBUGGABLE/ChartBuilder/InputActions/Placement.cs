using System.Collections.Generic;
using System.Threading.Tasks;
using UNBEATABLEChartEditor.Input;

namespace UNBUGGABLE.Keybinds;

public class PlaceTopLaneAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.StartTopLanePlacement();
    }
    
    public override async Task OnRelease()
    {
        ChartBuilder.EndTopLanePlacement();
    }
}

public class PlaceBottomLaneAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.StartBottomLanePlacement();
    }
    
    public override async Task OnRelease()
    {
        ChartBuilder.EndBottomLanePlacement();
    }
}

public class PlaceCameraLaneAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.PlaceCameraChange();
    }
}

public class PlaceCenterLaneAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.StartCenterLanePlacement();
    }
    
    public override async Task OnRelease()
    {
        ChartBuilder.EndCenterLanePlacement();
    }
}