using OpenTelemetry.Heartbeat.Monitor;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<ServiceOptions>().Bind(context.Configuration.GetSection(ServiceOptions.SectionName));
        
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();