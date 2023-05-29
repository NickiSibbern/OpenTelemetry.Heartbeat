using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public interface IMonitorRepository
{
    IReadOnlyList<IMonitor> Monitors { get; }
    bool AddOrUpdate(string key, IMonitor? monitor);
    bool Remove(string key);
    bool Contains(string key);
}

public class MonitorRepository : IMonitorRepository
{
    private readonly ConcurrentDictionary<string, IMonitor> _monitors;
    private readonly ILogger<MonitorRepository> _logger;

    public MonitorRepository(ILogger<MonitorRepository> logger)
    {
        _logger = logger;
        _monitors = new ConcurrentDictionary<string, IMonitor>();
    }

    public IReadOnlyList<IMonitor> Monitors => _monitors.Select(x => x.Value).ToImmutableList();

    public bool AddOrUpdate(string key, IMonitor? monitor)
    {
        if (monitor is null)
        {
            return false;
        }

        _monitors.AddOrUpdate(
            key,
            monitor,
            (_, _) => monitor);
        
        return true;
    }

    public bool Remove(string key) => _monitors.Remove(key, out _);

    public bool Contains(string key) => _monitors.ContainsKey(key);
}