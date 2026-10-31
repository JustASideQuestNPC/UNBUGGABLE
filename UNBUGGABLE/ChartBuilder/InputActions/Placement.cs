using System.Collections.Generic;
using System.Threading.Tasks;
using UNBUGGABLE.ConfigParser;
using UNBUGGABLE.Input;

namespace UNBUGGABLE.ChartBuilder.InputActions;

public class PlaceTopLaneAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override bool CanUseWhilePlaying => Config.Settings.EnableLivePlacement;
    public override bool IgnoreModifiers => true;
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
    public override bool CanUseWhilePlaying => Config.Settings.EnableLivePlacement;
    public override bool IgnoreModifiers => true;
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
    public override bool CanUseWhilePlaying => Config.Settings.EnableLivePlacement;
    public override bool IgnoreModifiers => true;
    public override async Task OnPress()
    {
        ChartBuilder.PlaceCameraChange();
    }
}

public class PlaceCenterLaneAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override bool CanUseWhilePlaying => Config.Settings.EnableLivePlacement;
    public override bool IgnoreModifiers => true;
    public override async Task OnPress()
    {
        ChartBuilder.StartCenterLanePlacement();
    }
    
    public override async Task OnRelease()
    {
        ChartBuilder.EndCenterLanePlacement();
    }
}

public class NudgeAction(List<string> keybinds, int mode, int direction) :
    InputActionBase(keybinds)
{
    public override bool CanUseWhilePlacingNotes => false;

    public override async Task OnPress()
    {
        // 0 = nudge only heads, 1 = nudge only tails, 2 = nudge all
        switch (mode)
        {
            case 0:
                ChartBuilder.NudgeNoteHeads(direction);
                break;
            case 1:
                ChartBuilder.NudgeNoteTails(direction);
                break;
            case 2:
                ChartBuilder.NudgeNotes(direction);
                break;
        }
    }
}

public class PlaceUnanimatedTriggerAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        await ChartBuilder.AddOrEditUnanimatedEvent();
    }
}

public class RemoveUnanimatedTriggerAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.TryRemoveUnanimatedEvent();
    }
}