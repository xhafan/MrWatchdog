using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBackend.Infrastructure.ErrorReporting;

public static class ErrorReportingServiceCollectionExtensions
{
    public static IServiceCollection AddErrorReporting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ErrorReportingOptions>()
            .Bind(configuration.GetSection(ErrorReportingOptions.SectionName))
            .Validate(options => options.QueueCapacity > 0, "ErrorReporting:QueueCapacity must be positive.")
            .Validate(options => options.DeliveryTimeout > TimeSpan.Zero, "ErrorReporting:DeliveryTimeout must be positive.")
            .Validate(options => options.ShutdownTimeout > TimeSpan.Zero, "ErrorReporting:ShutdownTimeout must be positive.")
            .ValidateOnStart();
        services.AddSingleton<ErrorReportQueue>();
        services.AddSingleton<ErrorReportingSink>();
        services.AddHostedService<ErrorReportDeliveryBackgroundService>();
        return services;
    }
}
