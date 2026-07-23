namespace UNBUGGABLE.Commands;

public class AddBpmRegionCommand(double time, double bpm) : ICommand
{
    public string Name => "Add BPM Region";
    
    private readonly BpmRegion _bpmRegion = new(time, bpm);
    
    public void Execute()
    {
        Chart.AddBpmRegion(_bpmRegion);
    }

    public void Undo()
    {
        Chart.RemoveBpmRegion(_bpmRegion);
    }
}

public class RemoveBpmRegionCommand(BpmRegion region) : ICommand
{
    public string Name => "Remove BPM Region";
    
    public void Execute()
    {
        Chart.RemoveBpmRegion(region);
    }

    public void Undo()
    {
        Chart.AddBpmRegion(region);
    }
}

public class EditBpmRegionCommand(BpmRegion region, double newBpm) : ICommand
{
    public string Name => "Edit BPM Region";
    
    private readonly double _oldBpm = region.Bpm;
    
    public void Execute()
    {
        Chart.EditBpmRegion(region, newBpm);
    }

    public void Undo()
    {
        Chart.EditBpmRegion(region, _oldBpm);
    }
}