namespace OpenTelemetry.Heartbeat.Monitor.Settings;

public class SearchSettings
{
    public required string RootDirectory { get; init; }
    public required string SearchPattern { get; init; }
    public required bool IncludeSubDirectories { get; init; }
}