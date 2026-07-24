using System.Collections.Generic;
using System.Threading.Tasks;
using UNBEATABLEChartEditor.Input;

namespace UNBUGGABLE.Keybinds;

public class SelectAllAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.SelectAll();
    }
}

public class CutAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.Cut();
    }
}

public class CopyAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.Copy();
    }
}

public class PasteAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.Paste();
    }
}

public class ClearSelectionAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.ClearSelection();
    }
}

public class DeleteSelectionAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.DeleteSelection();
    }
}

public class MirrorSelectionAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.MirrorSelection();
    }
}

public class MoveSelectionForwardAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.MoveSelectionForward();
    }
}

public class MoveSelectionBackAction(List<string> keybinds) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.MoveSelectionBack();
    }
}

public class SetNoteFlagAction(List<string> keybinds, char flag) : InputActionBase(keybinds)
{
    public override async Task OnPress()
    {
        ChartBuilder.SetNoteFlags(flag);
    }
}