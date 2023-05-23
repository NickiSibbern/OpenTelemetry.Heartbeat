using System.IO.Abstractions;
using OpenTelemetry.Heartbeat.Monitor;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Settings;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<SearchSettings>().Bind(context.Configuration.GetSection(nameof(SearchSettings)));
        services.AddOptions<MetricSettings>().Bind(context.Configuration.GetSection(nameof(MetricSettings)));
        
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IMonitorDefinitionSerializer, MonitorDefinitionSerializer>();
        services.AddScoped<IMonitorDefinitionRepository, MonitorDefinitionRepository>();

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();