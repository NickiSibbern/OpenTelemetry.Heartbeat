using Microsoft.Extensions.Options;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;
using OpenTelemetry.Heartbeat.Monitor.Telemetry;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors;

public class HttpMonitorFactory : IMonitorFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TelemetrySource _telemetrySource;
    private readonly HeartbeatConfig _heartbeatConfig;
    private readonly IDateTimeService _dateTimeService;

    public HttpMonitorFactory(IDateTimeService dateTimeService, IHttpClientFactory httpClientFactory,
        TelemetrySource telemetrySource, IOptions<HeartbeatConfig> metricConfig)
    {
        _dateTimeService = dateTimeService;
        _httpClientFactory = httpClientFactory;
        _telemetrySource = telemetrySource;
        _heartbeatConfig = metricConfig.Value;
    }

    public bool CanHandle(MonitorDefinition? monitorDefinition)
        => monitorDefinition?.Monitor?.Type == MonitorDefinitionType.Http;

    public IMonitor Create(MonitorDefinition monitorDefinition)
    {
        if (monitorDefinition.Monitor?.Type is not MonitorDefinitionType.Http)
        {
            throw new InvalidOperationException("Cannot create HttpMonitor from non-Http MonitorDefinition");
        }

        var httpClient = _httpClientFactory.CreateClient(nameof(HttpMonitor));
        return new HttpMonitor(monitorDefinition, httpClient, _dateTimeService, _heartbeatConfig,
            _telemetrySource.Meter);
    }
}