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
        HeartbeatConfig config,
        IDateTimeService dateTimeService,
        Meter meter)
    {
        // Arrange
        monitorDefinition.Monitor = null;
        var httpClient = mockHttpClient.ToHttpClient();

        // Act
        var act = () => new HttpMonitor(monitorDefinition, httpClient, dateTimeService, config, meter);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory, AutoNSubstituteData]
    public void ExecuteAsync_Should_Throw_If_CancellationToken_Is_Null(
        HttpMonitor sut,
        CancellationToken cancellationToken)
    {
        // Arrange && Act
        var act = async () => await sut.ExecuteAsync(cancellationToken);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Cancel_If_Root_Cancellation_Token_Cancels(
        MonitorDefinition monitorDefinition,
        MockHttpMessageHandler mockHttpClient,
        HeartbeatConfig config,
        IDateTimeService dateTimeService,
        Meter meter,
        CancellationTokenSource cancellationTokenSource,
        Uri requestUri)
    {
        // Arrange
        monitorDefinition.Monitor = new HttpMonitorDefinition { Url = requestUri, TimeOut = 10000, ResponseCode = 200 };

        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond(HttpStatusCode.Accepted);
        cancellationTokenSource.Cancel();

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition,
            httpClient,
            dateTimeService,
            config,
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
        HeartbeatConfig config,
        IDateTimeService dateTimeService,
        Meter meter,
        Uri requestUri,
        CancellationToken cancellationToken)
    {
        // Arrange
        monitorDefinition.Monitor = new HttpMonitorDefinition
        {
            Url = requestUri, TimeOut = 1000, ResponseCode = (int)HttpStatusCode.OK
        };

        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond(HttpStatusCode.OK);

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition,
            httpClient,
            dateTimeService,
            config,
            meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(true);
    }

    [Theory, AutoNSubstituteData]
    public async Task
        ExecuteAsync_Should_Return_Failure_If_HttpResponse_Does_Not_Match_ResponseStatusCode_From_Definition(
            MonitorDefinition monitorDefinition,
            MockHttpMessageHandler mockHttpClient,
            HeartbeatConfig config,
            IDateTimeService dateTimeService,
            Meter meter,
            Uri requestUri,
            CancellationToken cancellationToken)
    {
        // Arrange
        monitorDefinition.Monitor = new HttpMonitorDefinition
        {
            Url = requestUri, TimeOut = 1000, ResponseCode = (int)HttpStatusCode.OK
        };

        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond(HttpStatusCode.InternalServerError);

        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(
            monitorDefinition,
            httpClient,
            dateTimeService,
            config,
            meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
    }

    [Theory, AutoNSubstituteData]
    public async Task ExecuteAsync_Should_Return_Failure_With_Message_When_Reason_Phrase_Is_Not_Set_By_Server(
        MonitorDefinition monitorDefinition,
        HttpMonitorDefinition httpMonitorDefinition,
        HeartbeatConfig config,
        IDateTimeService dateTimeService,
        Meter meter,
        MockHttpMessageHandler mockHttpClient,
        CancellationToken cancellationToken)
    {
        // Arrange
        monitorDefinition.Monitor = httpMonitorDefinition;
        dateTimeService.Now.Returns(DateTimeOffset.Now);
        mockHttpClient.When("*").Respond(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "" });
        var httpClient = mockHttpClient.ToHttpClient();
        var sut = new HttpMonitor(monitorDefinition, httpClient, dateTimeService, config, meter);

        // Act
        var result = await sut.ExecuteAsync(cancellationToken);

        // Assert
        result.IsSuccess.Should().Be(false);
        result.Errors.Select(error => error.Message).Should()
            .Contain("Unknown error occurred - reason provided by the server was null");
    }
}