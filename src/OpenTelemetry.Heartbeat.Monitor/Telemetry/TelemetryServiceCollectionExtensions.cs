using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Heartbeat.Monitor.Settings;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace OpenTelemetry.Heartbeat.Monitor.Telemetry;

public static class TelemetryServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, HeartbeatSettings heartbeatSettings)
    {
        services.AddSingleton<TelemetrySource>();
        services.AddOpenTelemetry()
            .ConfigureResource(builder => builder
                .AddService(Assembly.GetExecutingAssembly().GetName().Name ?? "HeartbeatMonitor"))
            .WithMetrics(builder => builder
                .AddMeter("*")
                .AddOtlpExporter(o =>
                {
                    o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    o.Endpoint = new Uri(heartbeatSettings.MetricExporterEndpoint);
                }));
        
        return services;
    }
}