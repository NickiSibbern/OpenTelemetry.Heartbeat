using System.Text.Json.Serialization;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

public record MonitorDefinition
{
    public required string Name { get; init; }

    public required string Namespace { get; init; }
    
    [JsonPropertyName("type")]
    public required MonitorDefinitionType MonitorType { get; init; }
    
    public required int Interval { get; init; }
    
    [JsonPropertyName("http")]
    public HttpMonitorDefinition? Http { get; init; }
}