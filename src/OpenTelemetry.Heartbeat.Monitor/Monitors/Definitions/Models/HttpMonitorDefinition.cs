using System.ComponentModel.DataAnnotations;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

public class HttpMonitorDefinition
{
    /// <summary>
    /// Uri to be called in the monitor.
    /// </summary>
    public required Uri Url { get; init; }

    /// <summary>
    /// TimeOut in milliseconds for the request.
    /// </summary>
    public required int TimeOut { get; init; }

    /// <summary>
    /// The response code that should indicate if the monitor is healthy or not.
    /// </summary>
    public required int ResponseCode { get; init; }
}