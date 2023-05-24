using System.Text.Json.Serialization;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

public class MonitorDefinition
{
    [JsonIgnore]
    public string? FilePath { get; set; }
    
    public required string Name { get; set; }

    public required string Namespace { get; set; }
    
    [JsonPropertyName("type")]
    public required MonitorDefinitionType MonitorType { get; set; }
    
    public required int Interval { get; set; }
    
    [JsonPropertyName("http")]
    public HttpMonitorDefinition? Http { get; set; }
}