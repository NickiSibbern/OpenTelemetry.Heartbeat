namespace OpenTelemetry.Heartbeat.Monitor.Settings;

public static class OptionsExtensions
{
    public static T BindOptions<T>(this IServiceCollection services, IConfiguration configuration, string sectionName)
        where T : class
    {
        services.AddOptions<T>().Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        var options = configuration.GetSection(sectionName).Get<T>();
        return options ?? throw new InvalidOperationException($"Unable to bind options of type: {typeof(T).Name}");
    }
}