using System.Diagnostics.Metrics;
using System.Net;
using Atc.Test;
using FluentAssertions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Models;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;
using RichardSzalay.MockHttp;

namespace OpenTelemetry.Heartbeat.Monitor.Tests.Monitors.Models;

public class HttpMonitorTests
{
    [Theory, AutoNSubstituteData]
    public void Creation_Should_Throw_If_Http_Object_Is_Null(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        Meter meter)
    {
        // Arrange
        var httpClient = mockHttpClient.ToHttpClient();

        // Act
        var act = () => new HttpMonitor(monitorDefinition with { Http = null }, httpClient, settings, meter);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory, AutoNSubstituteData]
    public void ExecuteAsync_Should_Throw_If_CancellationToken_Is_Null(
        HttpMonitor sut)
    {
        // Arrange && Act
        var act = async () => await sut.ExecuteAsync();

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Cancel_If_Root_Cancellation_Token_Cancels(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        Meter meter,
        CancellationTokenSource cancellationTokenSource,
        Uri requestUri)
    {
        // Arrange
        mockHttpClient.When("*").Respond(HttpStatusCode.Accepted);
        cancellationTokenSource.Cancel();

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition with { Http = new HttpMonitorDefinition(requestUri, 10000, 200) },
            httpClient,
            settings,
            meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.ErrorMessage.Should().Be("The operation was canceled.");
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Success_If_HttpResponse_Matched_ResponseStatusCode_From_Definition(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        Meter meter,
        CancellationToken cancellationToken,
        Uri requestUri)
    {
        // Arrange
        mockHttpClient.When("*").Respond(HttpStatusCode.OK);

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition with { Http = new HttpMonitorDefinition(requestUri, 1000, (int)HttpStatusCode.OK) },
            httpClient,
            settings,
            meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(true);
        result.ErrorMessage.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_If_HttpResponse_Does_Not_Match_ResponseStatusCode_From_Definition(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        Meter meter,
        CancellationToken cancellationToken,
        Uri requestUri)
    {
        // Arrange
        mockHttpClient.When("*").Respond(HttpStatusCode.InternalServerError);

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition with { Http = new HttpMonitorDefinition(requestUri, 1000, (int)HttpStatusCode.OK) },
            httpClient,
            settings,
            meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_If_Exception_Is_Thrown(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        Meter meter,
        CancellationToken cancellationToken,
        Exception exception)
    {
        // Arrange
        mockHttpClient.When("*").Respond(() => throw exception);
        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(monitorDefinition, httpClient, settings, meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.ErrorMessage.Should().Be(exception.Message);
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_With_Message_When_Reason_Phrase_Is_Not_Set_By_Server(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        Meter meter,
        CancellationToken cancellationToken)
    {
        // Arrange
        mockHttpClient.When("*").Respond((_) => new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "" });
        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(monitorDefinition, httpClient, settings, meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.ErrorMessage.Should().Be("Unknown error occurred - reason provided by the server was null");
    }
}