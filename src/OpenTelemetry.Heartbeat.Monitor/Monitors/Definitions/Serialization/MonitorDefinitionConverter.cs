using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Serialization;

internal class MonitorDefinitionConverter : JsonConverter<IMonitorDefinition>
{
    public override IMonitorDefinition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        var monitorTypeElement = jsonDocument.RootElement.EnumerateObject().FirstOrDefault(p =>
            string.Compare(p.Name, nameof(IMonitorDefinition.Type), StringComparison.OrdinalIgnoreCase) == 0).Value;

        string? monitorType = monitorTypeElement.ValueKind switch
        {
            JsonValueKind.Number => ((MonitorDefinitionType)monitorTypeElement.GetInt32()).ToString(),
            JsonValueKind.String => monitorTypeElement.ToString(),
            _ => null
        };

        var result = monitorType?.ToLowerInvariant() switch
        {
            "http" => JsonSerializer.Deserialize<HttpMonitorDefinition>(jsonDocument.RootElement.GetRawText(), options),
            _ => null
        };

        return result;
    }

    public override void Write(Utf8JsonWriter writer, IMonitorDefinition value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case HttpMonitorDefinition httpMonitorDefinition:
                JsonSerializer.Serialize(writer, httpMonitorDefinition, options);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}