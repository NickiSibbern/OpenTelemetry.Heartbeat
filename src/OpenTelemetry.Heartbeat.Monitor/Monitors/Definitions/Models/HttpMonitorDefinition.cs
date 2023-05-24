namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

public class HttpMonitorDefinition
{
    public required Uri Url { get; init; }

    public required int TimeOut { get; init; }

    public required int ResponseCode { get; init; }
}