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

    /// <summary>
    /// Name of the metric to expose
    /// </summary>
    public required string MetricName { get; set; }

    /// <summary>
    /// Description of the metric to expose
    /// </summary>
    public required string MetricDescription { get; set; }
    
    /// <summary>
    /// The endpoint to send the metric to
    /// </summary>
    public required string MetricExporterEndpoint { get; set; }
}