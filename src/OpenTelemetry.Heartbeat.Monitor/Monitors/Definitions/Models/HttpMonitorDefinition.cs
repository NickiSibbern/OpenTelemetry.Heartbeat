namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

public record HttpMonitorDefinition(Uri Url, int TimeOut, int ResponseCode);