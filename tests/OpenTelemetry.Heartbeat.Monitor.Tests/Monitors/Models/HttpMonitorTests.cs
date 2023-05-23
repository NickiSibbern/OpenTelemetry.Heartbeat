using System.Diagnostics.Metrics;
using System.Net;
using Atc.Test;
using FluentAssertions;
using NSubstitute;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
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
        IDateTimeService dateTimeService,
        Meter meter)
    {
        // Arrange
        var httpClient = mockHttpClient.ToHttpClient();

        // Act
        var act = () => new HttpMonitor(monitorDefinition with { Http = null }, httpClient, dateTimeService, settings, meter);

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
        IDateTimeService dateTimeService,
        Meter meter,
        CancellationTokenSource cancellationTokenSource,
        Uri requestUri)
    {
        // Arrange
        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond(HttpStatusCode.Accepted);
        cancellationTokenSource.Cancel();

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition with { Http = new HttpMonitorDefinition(requestUri, 10000, 200) },
            httpClient,
            dateTimeService,
            settings,
            meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.Errors.Select(error => error.Message).Should().Contain("The operation was canceled.");
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Success_If_HttpResponse_Matched_ResponseStatusCode_From_Definition(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        IDateTimeService dateTimeService,
        Meter meter,
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        // Arrange
        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond(HttpStatusCode.OK);

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition with { Http = new HttpMonitorDefinition(requestUri, 1000, (int)HttpStatusCode.OK) },
            httpClient,
            dateTimeService,
            settings,
            meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(true);
    }
    
    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_If_It_Should_Not_Run_Yet(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        IDateTimeService dateTimeService,
        Meter meter,
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        // Arrange
        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond(HttpStatusCode.InternalServerError);

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition with { Interval = 20000, Http = new HttpMonitorDefinition(requestUri, 1000, (int)HttpStatusCode.OK) },
            httpClient,
            dateTimeService,
            settings,
            meter);

        // Act
        await sut.ExecuteAsync(cancellationToken);
        var result = await sut.ExecuteAsync(cancellationToken); // Second run to test that we don't run it again, before interval has passed

        // Assert
        result.IsSuccess.Should().Be(false);
        result.Errors.Select(error => error.Message).Should().Contain($"Monitor for: {monitorDefinition.Name} should not run yet");
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_If_HttpResponse_Does_Not_Match_ResponseStatusCode_From_Definition(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        IDateTimeService dateTimeService,
        Meter meter,
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        // Arrange
        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond(HttpStatusCode.InternalServerError);

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition with { Http = new HttpMonitorDefinition(requestUri, 1000, (int)HttpStatusCode.OK) },
            httpClient,
            dateTimeService,
            settings,
            meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_If_Exception_Is_Thrown(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        IDateTimeService dateTimeService,
        Meter meter,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Arrange
        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond(() => throw exception);
        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(monitorDefinition, httpClient, dateTimeService, settings, meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.Errors.Select(error => error.Message).Should().Contain(exception.Message);
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_With_Message_When_Reason_Phrase_Is_Not_Set_By_Server(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        MetricSettings settings,
        IDateTimeService dateTimeService,
        Meter meter,
        CancellationToken cancellationToken)
    {
        // Arrange
        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond((_) => new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "" });
        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(monitorDefinition, httpClient, dateTimeService, settings, meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.Errors.Select(error => error.Message).Should().Contain("Unknown error occurred - reason provided by the server was null");
    }
}