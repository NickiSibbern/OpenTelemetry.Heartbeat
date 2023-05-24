using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public interface IMonitorRepository
{
    IReadOnlyList<IMonitor> Monitors { get; }
    void AddOrUpdate(string key, IMonitor? monitor);
    void Remove(string key);
    bool Contains(string key);
}

public class MonitorRepository : IMonitorRepository
{
    private readonly ConcurrentDictionary<string, IMonitor> _monitors;
    
    public MonitorRepository()
    {
        _monitors = new ConcurrentDictionary<string, IMonitor>();
    }

    public IReadOnlyList<IMonitor> Monitors => _monitors.Select(x => x.Value).ToImmutableList();

    public void AddOrUpdate(string key, IMonitor? monitor)
    {
        if (monitor is null)
        {
            return;
        }

        _monitors.AddOrUpdate(
            key,
            monitor,
            (_, _) => monitor);
    }

    public void Remove(string key) => _monitors.Remove(key, out _);

    public bool Contains(string key) => _monitors.ContainsKey(key);
}