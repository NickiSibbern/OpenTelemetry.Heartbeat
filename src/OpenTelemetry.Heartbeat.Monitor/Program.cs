using System.IO.Abstractions;
using OpenTelemetry.Heartbeat.Monitor;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Serialization;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<SearchOptions>().Bind(context.Configuration.GetSection(SearchOptions.SectionName));
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IMonitorDefinitionSerializer, MonitorDefinitionSerializer>();
        services.AddScoped<IMonitorDefinitionRepository, IMonitorDefinitionRepository>();

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();