namespace OpenTelemetry.Heartbeat.Monitor.Settings;

public class SearchConfig
{
    public required string RootDirectory { get; init; }
    public required string SearchPattern { get; init; }
    public required bool IncludeSubDirectories { get; init; }
}