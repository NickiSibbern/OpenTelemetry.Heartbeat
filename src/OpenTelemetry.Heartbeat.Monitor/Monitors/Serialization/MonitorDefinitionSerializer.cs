using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Serialization;

public interface IMonitorDefinitionSerializer
{
    Task<MonitorDefinition?> DeserializeAsync(Stream json, CancellationToken cancellationToken);
}

public sealed class MonitorDefinitionSerializer : IMonitorDefinitionSerializer
{
    private readonly ILogger<MonitorDefinitionSerializer> _logger;

    public MonitorDefinitionSerializer(ILogger<MonitorDefinitionSerializer> logger)
    {
        _logger = logger;
    }

    public async Task<MonitorDefinition?> DeserializeAsync(Stream json, CancellationToken cancellationToken)
    {
        try
        {
            var definition = await JsonSerializer.DeserializeAsync<MonitorDefinition>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Converters = { new JsonStringEnumConverter() }
                },
                cancellationToken);

            return definition;
        }
        catch (Exception e)
        {
            _logger.LogInformation("Unable to deserialize monitor definition: {Exception}", e);
            return null;
        }
    }
}