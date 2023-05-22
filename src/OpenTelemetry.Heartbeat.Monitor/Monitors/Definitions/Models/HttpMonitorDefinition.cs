namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

public record HttpMonitorDefinition(string Url, int TimeOut, int ResponseCode);