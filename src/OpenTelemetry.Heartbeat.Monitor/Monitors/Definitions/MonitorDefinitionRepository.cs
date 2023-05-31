using System.Collections.Concurrent;
using System.IO.Abstractions;
using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Serialization;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;

public interface IMonitorDefinitionRepository
{
    Task<IEnumerable<MonitorDefinition>> GetMonitorDefinitions(CancellationToken cancellationToken);
}

public class MonitorDefinitionRepository : IMonitorDefinitionRepository
{
    private readonly SearchConfig _searchConfig;
    private readonly IFileSystem _fileSystem;
    private readonly IMonitorDefinitionSerializer _serializer;

    public MonitorDefinitionRepository(IFileSystem fileSystem, IMonitorDefinitionSerializer serializer, IOptions<SearchConfig> searchOptions)
    {
        _fileSystem = fileSystem;
        _serializer = serializer;
        _searchConfig = searchOptions.Value;
    }

    public async Task<IEnumerable<MonitorDefinition>> GetMonitorDefinitions(CancellationToken cancellationToken)
    {
        var monitorDefinitions = new ConcurrentBag<(DateTime creationTime, MonitorDefinition monitorDefinition)>();

        var files = _fileSystem.Directory.EnumerateFiles(
            _searchConfig.RootDirectory,
            _searchConfig.SearchPattern,
            _searchConfig.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Select(path => _fileSystem.FileInfo.New(path));

        await Task.WhenAll(files.Select(async file =>
        {
            var monitorDefinition = await _serializer.DeserializeAsync(file.OpenRead(), cancellationToken);

            if (monitorDefinition is not null)
            {
                monitorDefinitions.Add((file.CreationTime, monitorDefinition));
            }

        })).ConfigureAwait(false);

        return monitorDefinitions
            .GroupBy(x => x.monitorDefinition.Name)
            .Select(x => x.MaxBy(y => y.creationTime))
            .Select(x => x.monitorDefinition);
    }
}