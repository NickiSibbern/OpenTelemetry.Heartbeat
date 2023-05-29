using System.Diagnostics.Metrics;
using FluentResults;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;

namespace OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

public sealed class HttpMonitor : MonitorBase
{
    private readonly MonitorDefinition _monitorDefinition;
    private readonly HttpClient _httpClient;

    public HttpMonitor(
        MonitorDefinition monitorDefinition,
        HttpClient httpClient,
        IDateTimeService dateTime,
        HeartbeatConfig config,
        Meter meter)
        : base(monitorDefinition, dateTime, config, meter)
    {
        ArgumentNullException.ThrowIfNull(monitorDefinition.Monitor);

        _monitorDefinition = monitorDefinition;
        _httpClient = httpClient;
    }

    public override async Task<Result> ExecuteAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);

        var httpMonitorDefinition = _monitorDefinition.Monitor as HttpMonitorDefinition;
        if (httpMonitorDefinition is null)
        {
            return Result.Fail($"Monitor was expected to be of type {typeof(HttpMonitorDefinition)} but was not");
        }

        return await base.ExecuteMonitorAsync(async () =>
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedTokenSource.CancelAfter(httpMonitorDefinition.TimeOut);

            var response = await _httpClient.GetAsync(httpMonitorDefinition.Url, linkedTokenSource.Token);

            if ((int)response.StatusCode == httpMonitorDefinition.ResponseCode)
            {
                base.UpMetric = 1;
                return Result.Ok().WithSuccess("Monitor executed successfully");
            }

            base.UpMetric = 0;
            return Result.Fail($"Monitor for: {_monitorDefinition.Name} failed with errors")
                .WithError(new Error(string.IsNullOrWhiteSpace(response.ReasonPhrase)
                    ? "Unknown error occurred - reason provided by the server was null"
                    : response.ReasonPhrase));
        });
    }
}