namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

public record HttpMonitorDefinition(string Url, int TimeOut, int ResponseCode);