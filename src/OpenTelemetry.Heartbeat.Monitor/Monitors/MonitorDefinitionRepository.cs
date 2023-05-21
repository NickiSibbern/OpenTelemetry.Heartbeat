using System.Collections.Concurrent;
using System.IO.Abstractions;
using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Serialization;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public interface IMonitorDefinitionRepository
{
    Task<IEnumerable<MonitorDefinition>> GetMonitorDefinitions(CancellationToken cancellationToken);
}

public class MonitorDefinitionRepository : IMonitorDefinitionRepository
{
    private readonly SearchOptions _searchOptions;
    private readonly IFileSystem _fileSystem;
    private readonly IMonitorDefinitionSerializer _serializer;

    public MonitorDefinitionRepository(IFileSystem fileSystem, IMonitorDefinitionSerializer serializer, IOptions<SearchOptions> searchOptions)
    {
        _fileSystem = fileSystem;
        _serializer = serializer;
        _searchOptions = searchOptions.Value;
    }
    
    public async Task<IEnumerable<MonitorDefinition>> GetMonitorDefinitions(CancellationToken cancellationToken)
    {
        var monitorDefinitions = new ConcurrentBag<(DateTime creationTime, MonitorDefinition monitorDefinition)>();

        var files = _fileSystem.Directory.EnumerateFiles(
            _searchOptions.RootDirectory,
            _searchOptions.SearchPattern,
            _searchOptions.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Select(path => _fileSystem.FileInfo.New(path));
        
        await Task.WhenAll(files.Select(async file =>
        {
            var monitorDefinition = await _serializer
                .DeserializeAsync(file.OpenRead(), cancellationToken)
                .ConfigureAwait(false);

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