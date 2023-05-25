using System.IO.Abstractions;
using OpenTelemetry.Heartbeat.Monitor;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<SearchSettings>().Bind(context.Configuration.GetSection(nameof(SearchSettings))).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<MetricSettings>().Bind(context.Configuration.GetSection(nameof(MetricSettings))).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<HeartbeatSettings>().Bind(context.Configuration.GetSection(nameof(HeartbeatSettings))).ValidateDataAnnotations().ValidateOnStart();
        services.AddSingleton<Telemetry>();
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