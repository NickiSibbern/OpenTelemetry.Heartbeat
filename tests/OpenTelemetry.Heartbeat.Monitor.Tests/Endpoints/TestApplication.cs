using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Endpoints;

internal sealed class TestApplication : WebApplicationFactory<Program>
{
    private readonly Action<IWebHostBuilder>? _configure;

    public TestApplication(Action<IWebHostBuilder>? configure = default)
    {
        _configure = configure;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            // Remove the worker service as it has no function in our api tests
            ServiceDescriptor? workerDescriptor = services.FirstOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IHostedService) &&
                descriptor.ImplementationType == typeof(Worker));

            if (workerDescriptor != null)
            {
                services.Remove(workerDescriptor);
            }
        });
        
        
        _configure?.Invoke(builder);
    }
}