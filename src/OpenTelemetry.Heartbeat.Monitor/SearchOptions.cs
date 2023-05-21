namespace OpenTelemetry.Heartbeat.Monitor;

public class SearchOptions
{
    public const string SectionName = "SearchSettings";

    public required string RootDirectory { get; init; }
    public required string SearchPattern { get; init; }
    public required bool IncludeSubDirectories { get; init; }
}