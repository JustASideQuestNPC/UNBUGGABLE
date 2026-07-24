using System.Collections.Generic;
using System.Threading.Tasks;
using UNBEATABLEChartEditor.Input;
using UNBUGGABLE.Views;

namespace UNBUGGABLE.Keybinds;

public class MoveForwardAction(List<string> keybinds) : InputActionBase(keybinds, true)
{
    public override async Task OnPress()
    {
        Chart.MoveToNextSnap();
    }
}

public class MoveBackAction(List<string> keybinds) : InputActionBase(keybinds, true)
{
    public override async Task OnPress()
    {
        Chart.MoveToPreviousSnap();
    }
}


public class PlayPauseAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        Chart.PlayOrPauseSong();
    }
}

public class ZoomInAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        NoteViewer.IncreaseZoom();
    }
}

public class ZoomOutAction(List<string> keybinds) : InputActionBase(keybinds)
{
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