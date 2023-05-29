using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor;

public interface IHeartbeatMonitor
{
    Task SetupInitialMonitors(CancellationToken cancellationToken);
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public class HeartbeatMonitor : IHeartbeatMonitor
{
    private readonly IMonitorRepository _monitorRepository;
    private readonly IMonitorDefinitionRepository _monitorDefinitionRepository;
    private readonly IEnumerable<IMonitorFactory> _monitorFactories;
    private readonly HeartbeatConfig _heartbeatConfig;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<HeartbeatMonitor> _logger;

    public HeartbeatMonitor(
        IMonitorRepository monitorRepository,
        IMonitorDefinitionRepository monitorDefinitionRepository,
        IEnumerable<IMonitorFactory> monitorFactories,
        IOptions<HeartbeatConfig> heartbeatConfig,
        IDateTimeService dateTimeService,
        ILogger<HeartbeatMonitor> logger)
    {
        _monitorRepository = monitorRepository;
        _monitorDefinitionRepository = monitorDefinitionRepository;
        _monitorFactories = monitorFactories;
        _heartbeatConfig = heartbeatConfig.Value;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task SetupInitialMonitors(CancellationToken cancellationToken)
    {
        var monitorDefinitions = await _monitorDefinitionRepository.GetMonitorDefinitions(cancellationToken);
        foreach (var definition in monitorDefinitions)
        {
            var factory = _monitorFactories.FirstOrDefault(x => x.CanHandle(definition));
            if (factory is not null)
            {
                var key = definition.Name;
                var monitor = factory.Create(definition);
                _monitorRepository.AddOrUpdate(key, monitor);
            }
        }
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var batch in _monitorRepository.Monitors.Chunk(_heartbeatConfig.ConcurrentMonitorChecks).ToArray())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await Task.WhenAll(batch.Select(async monitor =>
            {
                var result = await monitor.ExecuteAsync(cancellationToken);
                if (result.IsSuccess)
                {
                    _logger.LogDebug("Monitor: {MonitorName} executed successfully at {ExecutionTime}", monitor.Name, _dateTimeService.Now);
                }
                else
                {
                    var errors = result.Errors.Where(x => x.GetType() != typeof(WarningResult));
                    if (errors.Any())
                    {
                        _logger.LogWarning("Monitor: {MonitorName} executed with errors: {Errors}", monitor.Name, errors);    
                    }
                }
            }));
        }
    }
}