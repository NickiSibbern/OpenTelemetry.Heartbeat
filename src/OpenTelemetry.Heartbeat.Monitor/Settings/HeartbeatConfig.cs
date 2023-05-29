using System.ComponentModel.DataAnnotations;

namespace OpenTelemetry.Heartbeat.Monitor.Settings;

public class HeartbeatConfig
{
    /// <summary>
    /// The interval in milliseconds between each job, i.e. the time between each run where all monitors are checked if they should be executed.
    /// </summary>
    /// <value>Defaults to 100ms</value>
    [Range(100, int.MaxValue)]
    public required int JobInterval { get; set; } = 100;

    /// <summary>
    /// The amount of monitors to be executed in parallel.
    /// </summary>
    /// <value>Defaults to 10</value>
    [Range(1, int.MaxValue)]
    public required int ConcurrentMonitorChecks { get; set; } = 10;

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