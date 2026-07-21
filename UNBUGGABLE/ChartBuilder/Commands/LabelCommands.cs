namespace UNBUGGABLE.Commands;

public class AddLabelCommand(double time, string text) : ICommand
{
    private readonly Chart.Label _label = new(time, text);
    
    public void Execute()
    {
        Chart.AddLabel(_label);
    }
    
    public void Undo()
    {
        Chart.RemoveLabel(_label);
    }
}

public class RemoveLabelCommand(Chart.Label label) : ICommand
{
    public void Execute()
    {
        Chart.RemoveLabel(label);
    }
    
    public void Undo()
    {
        Chart.AddLabel(label);
    }
}

public class EditLabelCommand(Chart.Label oldLabel, string newText) : ICommand
{
    private readonly Chart.Label _newLabel = new(oldLabel.Time, newText);

    public void Execute()
    {
        Chart.RemoveLabel(oldLabel);
        Chart.AddLabel(_newLabel);
    }
    
    public void Undo()
    {
        Chart.RemoveLabel(_newLabel);
        Chart.AddLabel(oldLabel);
    }
}