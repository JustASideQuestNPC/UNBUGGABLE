namespace UNBUGGABLE;

/// <summary>
/// Represents a section of the chart with a specific BPM.
/// </summary>
/// <param name="startTime">
///     The time when the region starts, in milliseconds. The end time is always the start time of
///     the next region, or the end of the chart if there is no next region.
/// </param>
/// <param name="bpm">The BPM/tempo in the region, in beats per minute.</param>
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