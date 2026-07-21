namespace UNBUGGABLE.Commands;

public class AddBpmRegionCommand(double time, double bpm) : ICommand
{
    private readonly BpmRegion _bpmRegion = new(time + Chart.Metadata.ChartOffset, bpm);
    
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