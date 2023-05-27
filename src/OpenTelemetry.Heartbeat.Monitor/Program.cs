using System.IO.Abstractions;
using OpenTelemetry.Heartbeat.Monitor;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;
using OpenTelemetry.Heartbeat.Monitor.Telemetry;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.BindOptions<SearchSettings>(context.Configuration, nameof(SearchSettings));
        var heartbeatSettings = services.BindOptions<HeartbeatSettings>(context.Configuration, nameof(HeartbeatSettings));

        services.AddTelemetry(heartbeatSettings);
        services.AddSingleton<IMonitorRepository, MonitorRepository>();

        services.AddHttpClient(nameof(HttpMonitor));

        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IMonitorDefinitionSerializer, MonitorDefinitionSerializer>();
        services.AddSingleton<IMonitorDefinitionRepository, MonitorDefinitionRepository>();
        services.AddSingleton<IMonitorFactory, HttpMonitorFactory>();
        services.AddSingleton<IHeartbeatMonitor, HeartbeatMonitor>();

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();