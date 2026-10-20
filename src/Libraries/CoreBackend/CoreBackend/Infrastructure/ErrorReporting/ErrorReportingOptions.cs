namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class ErrorReportingOptions
{
    public const string SectionName = "ErrorReporting";

    public bool Enabled { get; set; } = true;
    public int QueueCapacity { get; set; } = 256;
    public TimeSpan DeliveryTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);
}
