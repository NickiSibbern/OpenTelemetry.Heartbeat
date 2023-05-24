namespace OpenTelemetry.Heartbeat.Monitor.Settings;

public class HeartbeatSettings
{
    /// <summary>
    /// The interval in milliseconds between each job
    /// </summary>
    public required int JobInterval { get; set; }
    
    /// <summary>
    /// The amount of monitors to be run in parallel in each job
    /// </summary>
    public required int JobBatchSize { get; set; }
}