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

    public HttpMonitor(MonitorDefinition monitorDefinition, HttpClient httpClient, IDateTimeService dateTime, MetricSettings settings, Meter meter)
        : base(monitorDefinition, dateTime, settings, meter)
    {
        ArgumentNullException.ThrowIfNull(monitorDefinition.Http);

        _monitorDefinition = monitorDefinition;
        _httpClient = httpClient;
    }

    public override async Task<Result<string>> ExecuteAsync(CancellationToken? cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);

        return await base.ExecuteMonitorAsync(async () =>
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value);
            linkedTokenSource.CancelAfter(_monitorDefinition.Http!.TimeOut);

            var response = await _httpClient.GetAsync(_monitorDefinition.Http!.Url, linkedTokenSource.Token);

            if ((int)response.StatusCode == _monitorDefinition.Http.ResponseCode)
            {
                base.UpMetric = 1;
                {
                    return Result.Ok($"Monitor for: {_monitorDefinition.Name} completed successfully");
                }
            }

            base.UpMetric = 0;
            return Result.Fail($"Monitor for: {_monitorDefinition.Name} failed with errors").WithError(
                new Error(string.IsNullOrWhiteSpace(response.ReasonPhrase)
                    ? "Unknown error occurred - reason provided by the server was null"
                    : response.ReasonPhrase));
        });
    }
}