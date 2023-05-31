using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Serialization;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;


public class MonitorDefinition
{
    /// <summary>
    /// Name of the monitor, must be unique.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// namespace for the monitor, <see cref="https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/resource/semantic_conventions/README.md" />
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// How often the monitor should be executed in milliseconds.
    /// </summary>
    [Range(100, int.MaxValue)]
    public int Interval { get; set; }

    [JsonConverter(typeof(MonitorDefinitionConverter))]
    public IMonitorDefinition? Monitor { get; set; }
}
