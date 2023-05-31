using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Serialization;

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
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(), new MonitorDefinitionConverter() }
        };

        try
        {
            var definition = await JsonSerializer.DeserializeAsync<MonitorDefinition>(
                json,
                jsonOptions,
                cancellationToken);

            return definition?.Monitor is null ? null : definition;
            
        }
        catch (Exception e)
        {
            _logger.LogError("Unable to deserialize monitor definition: {Exception}", e);
            return null;
        }
    }
}