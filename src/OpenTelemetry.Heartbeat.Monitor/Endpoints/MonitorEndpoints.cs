using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;

namespace OpenTelemetry.Heartbeat.Monitor.Endpoints;

public static class MonitorEndpoints
{
    public static WebApplication RegisterMonitorEndpoints(this WebApplication app)
    {
        app.MapGet("/monitors",
            (IMonitorRepository monitorRepository) => monitorRepository.Monitors);

        app.MapPost("/monitors",
            (
                [FromServices] IMonitorRepository monitorRepository,
                [FromServices] IEnumerable<IMonitorFactory> monitorFactories,
                [FromBody] MonitorDefinition monitorDefinition) =>
            {
                var factory = monitorFactories.FirstOrDefault(x => x.CanHandle(monitorDefinition));
                if (factory is null)
                {
                    return Results.BadRequest($"Unable to handle monitor definition, no factory for {monitorDefinition.MonitorType} found");
                }

                var monitor = factory.Create(monitorDefinition);
                monitorRepository.AddOrUpdate(monitorDefinition.Name, monitor);

                return Results.Ok();
            });

        app.MapDelete("/monitors/{name}",
            (
                [FromServices] IMonitorRepository monitorRepository,
                string name) =>
            {
                monitorRepository.Remove(name);

                return Results.Ok();
            });

        return app;
    }
}