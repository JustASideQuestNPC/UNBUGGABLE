namespace UNBUGGABLE.Commands;

public class AddLabelCommand(long time, string text) : ICommand
{
    public string Name => "Add Label";
    
    public bool UpdatesPriorityList => false;
    
    private readonly Chart.Label _label = new(time, text);
    
    public void Execute()
    {
        Chart.AddLabel(_label);
    }
    
    public void Undo()
    {
        Chart.RemoveLabel(_label);
    }
    
    public override string ToString() => $"{Name} at {time} ({text})";
}

public class RemoveLabelCommand(Chart.Label label) : ICommand
{
    public string Name => "Remove Label";
    
    public bool UpdatesPriorityList => false;
    
    public void Execute()
    {
        Chart.RemoveLabel(label);
    }
    
    public void Undo()
    {
        Chart.AddLabel(label);
    }
    
    public override string ToString() => $"{Name} at {label.Time} ({label.Text})";
}

public class EditLabelCommand(Chart.Label oldLabel, string newText) : ICommand
{
    public string Name => "Edit Label";
    
    public bool UpdatesPriorityList => false;
    
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
    
    public override string ToString() => $"{Name} at {oldLabel.Time} ({oldLabel.Text} -> " +
                                         $"{newText})";
}