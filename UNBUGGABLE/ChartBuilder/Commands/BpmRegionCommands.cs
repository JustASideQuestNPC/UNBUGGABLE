namespace UNBUGGABLE.Commands;

public class AddBpmRegionCommand(long time, double bpm) : ICommand
{
    public string Name => "Add BPM Region";
    
    public bool UpdatesPriorityList => false;
    
    private readonly BpmRegion _bpmRegion = new(time, bpm);
    
    public void Execute()
    {
        Chart.AddBpmRegion(_bpmRegion);
    }

    public void Undo()
    {
        Chart.RemoveBpmRegion(_bpmRegion);
    }
    
    public override string ToString() => $"{Name} at {time} ({bpm} BPM)";
}

public class RemoveBpmRegionCommand(BpmRegion region) : ICommand
{
    public string Name => "Remove BPM Region";

    public bool UpdatesPriorityList => false;

    public void Execute()
    {
        Chart.RemoveBpmRegion(region);
    }

    public void Undo()
    {
        Chart.AddBpmRegion(region);
    }

    public override string ToString() => $"{Name} at {region.StartTime} ({region.Bpm} BPM)";
}

public class EditBpmRegionCommand(BpmRegion region, double newBpm) : ICommand
{
    public string Name => "Edit BPM Region";
    
    public bool UpdatesPriorityList => false;
    
    private readonly double _oldBpm = region.Bpm;
    
    public void Execute()
    {
        Chart.EditBpmRegion(region, newBpm);
    }

    public void Undo()
    {
        Chart.EditBpmRegion(region, _oldBpm);
    }
    
    public override string ToString() => $"{Name} at {region.StartTime}: {_oldBpm} BPM -> " +
                                         $"{newBpm} BPM";
}