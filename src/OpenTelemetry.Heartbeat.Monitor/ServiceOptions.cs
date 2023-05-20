namespace OpenTelemetry.Heartbeat.Monitor;

public class ServiceOptions
{
    public const string SectionName = "settings";

    public required string SearchDirectory { get; set; }

    public required string SearchPattern { get; set; }
}