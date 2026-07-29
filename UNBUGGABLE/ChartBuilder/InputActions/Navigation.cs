using System.Collections.Generic;
using System.Threading.Tasks;
using UNBEATABLEChartEditor.Input;
using UNBUGGABLE.Resources;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.Keybinds;

public class MoveForwardAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override bool IgnoreModifiers => true;

    public override async Task OnPress()
    {
        if (ChartBuilder.QuickScroll)
        {
            Chart.QuickScroll(Config.Settings.QuickScrollBeats);
        }
        else
        {
            Chart.MoveToNextSnap();
        }
    }
}

public class MoveBackAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override bool IgnoreModifiers => true;
    public override async Task OnPress()
    {
        if (ChartBuilder.QuickScroll)
        {
            Chart.QuickScroll(-Config.Settings.QuickScrollBeats);
        }
        else
        {
            Chart.MoveToPreviousSnap();
        }
    }
}

public class QuickScrollModifierAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.QuickScroll = true;
    }
    
    public override async Task OnRelease()
    {
        ChartBuilder.QuickScroll = false;
    }
}

public class PlayPauseAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override bool CanUseWhilePlaying => true;

    public override async Task OnPress()
    {
        Chart.PlayOrPauseSong();
    }
}

public class ZoomInAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override bool CanUseWhilePlacingNotes => false;
    
    public override async Task OnPress()
    {
        NoteViewer.IncreaseZoom();
    }
}

public class ZoomOutAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override bool CanUseWhilePlacingNotes => false;
    
    public override async Task OnPress()
    {
        NoteViewer.DecreaseZoom();
    }
}

public class PrevLabelAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        Chart.MoveToPreviousLabel();
    }
}

public class NextLabelAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        Chart.MoveToNextLabel();
    }
}

public class PrevNoteSnapAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        Chart.DecreaseBeatSnap();
    }
}

public class NextNoteSnapAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        Chart.IncreaseBeatSnap();
    }
}