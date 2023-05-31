using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Atc.Test;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Endpoints;

public class MonitorEndpointTests
{
    private readonly JsonSerializerOptions _defaultOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() }
    };

    [Theory, AutoNSubstituteData]
    internal async Task Get_Should_Return_List_Of_IMonitors(
        IMonitorRepository monitorRepository,
        List<HttpMonitor> monitors,
        CancellationToken cancellationToken)
    {
        // Arrange
        monitorRepository.Monitors.Returns(monitors);
        await using var app = new TestApplication(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(monitorRepository);
            });
        });

        using var client = app.CreateClient();

        // Act
        var response = await client.GetAsync("/monitors", cancellationToken: cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await JsonSerializer.DeserializeAsync<List<TestMonitor>>(await response.Content.ReadAsStreamAsync(cancellationToken), _defaultOptions,
            cancellationToken);
        content.Should().BeEquivalentTo(monitors);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Post_Should_Return_Add_MonitorDefinition_And_Return_OkResult(
        MonitorRepository monitorRepository,
        MonitorDefinition monitorDefinition,
        HttpMonitorDefinition httpMonitorDefinition,
        CancellationToken cancellationToken)
    {
        // Arrange
        monitorDefinition.Monitor = httpMonitorDefinition;

        await using var app = new TestApplication(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IMonitorRepository>(monitorRepository);
            });
        });

        using var client = app.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/monitors", monitorDefinition, cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        monitorRepository.Monitors.Should().Contain(x => x.Name == monitorDefinition.Name);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Post_Should_Return_BadRequest_If_Definition_Is_Not_Supported(
        CancellationToken cancellationToken)
    {
        // Arrange
        await using var app = new TestApplication();
        using var client = app.CreateClient();
        var content = JsonContent.Create(new { Name = "foo", Namespace = "ns", Interval = 98, Monitor = new { Type = "foo", } });

        // Act
        var response = await client.PostAsync(
            "/monitors",
            content,
            cancellationToken);

        // Assert
        var reason = await response.Content.ReadFromJsonAsync<string>(_defaultOptions, cancellationToken);
        reason.Should().Be("Unable to handle monitor definition");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Remove_Should_Remove_Monitor_With_Key_And_Return_OkResult(
        MonitorRepository monitorRepository,
        HttpMonitor monitor,
        HttpMonitor toDeleteMonitor,
        CancellationToken cancellationToken)
    {
        // Arrange
        await using var app = new TestApplication(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IMonitorRepository>(monitorRepository);
            });
        });

        monitorRepository.AddOrUpdate("foo", monitor);
        monitorRepository.AddOrUpdate("bar", toDeleteMonitor);

        using var client = app.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/monitors/bar", cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        monitorRepository.Monitors.Should().NotContain(x => x.Name == toDeleteMonitor.Name);
        monitorRepository.Monitors.Should().Contain(x => x.Name == monitor.Name);
    }
}