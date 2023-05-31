using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Heartbeat.Monitor;
using OpenTelemetry.Heartbeat.Monitor.Abstractions;
using OpenTelemetry.Heartbeat.Monitor.Endpoints;
using OpenTelemetry.Heartbeat.Monitor.Monitors;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Definitions.Serialization;
using OpenTelemetry.Heartbeat.Monitor.Monitors.Models;
using OpenTelemetry.Heartbeat.Monitor.Settings;
using OpenTelemetry.Heartbeat.Monitor.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.BindOptions<SearchConfig>(builder.Configuration, nameof(SearchConfig));
var heartbeatConfig = builder.Services.BindOptions<HeartbeatConfig>(builder.Configuration, nameof(HeartbeatConfig));

builder.Services.AddTelemetry(heartbeatConfig);
builder.Services.AddSingleton<IMonitorRepository, MonitorRepository>();

builder.Services.AddHttpClient(nameof(HttpMonitor));

builder.Services.TryAddSingleton<IFileSystem, FileSystem>();
builder.Services.TryAddSingleton<IDateTimeService, DateTimeService>();
builder.Services.TryAddSingleton<IMonitorDefinitionSerializer, MonitorDefinitionSerializer>();
builder.Services.TryAddSingleton<IMonitorDefinitionRepository, MonitorDefinitionRepository>();
builder.Services.TryAddSingleton<IMonitorFactory, HttpMonitorFactory>();
builder.Services.TryAddSingleton<IHeartbeatMonitor, HeartbeatMonitor>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterMonitorEndpoints();

app.Run();

// used for testing
[ExcludeFromCodeCoverage]
public partial class Program
{
}