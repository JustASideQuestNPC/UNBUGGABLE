namespace UNBUGGABLE;

public class BpmRegion(double startTime, double bpm)
{
    /// <summary>
    /// The BPM inside the region. DO NOT SET THIS VALUE DIRECTLY, it will mess up snap lines. Use
    /// <c>Chart.EditBpmRegion()</c> to change it instead.
    /// </summary>
    public double Bpm { get; set; } = bpm;
    public double StartTime { get; set; } = startTime;

    public BpmRegion? Previous { get; set; }
    public BpmRegion? Next { get; set;}
    
    public double EndTime => Next?.StartTime ?? Chart.Length;
    
    public double MsPerBeat => 60000 / Bpm;
}