namespace UNBUGGABLE;

public class BpmRegion(long startTime, double bpm)
{
    /// <summary>
    /// The BPM inside the region. DO NOT SET THIS VALUE DIRECTLY, it will mess up snap lines. Use
    /// <c>Chart.EditBpmRegion()</c> to change it instead.
    /// </summary>
    public double Bpm { get; set; } = bpm;
    public long StartTime { get; set; } = startTime;

    public BpmRegion? Previous { get; set; }
    public BpmRegion? Next { get; set;}
    
    public long EndTime => Next?.StartTime ?? Chart.Length;
    
    public double MsPerBeat => 60000 / Bpm;
}