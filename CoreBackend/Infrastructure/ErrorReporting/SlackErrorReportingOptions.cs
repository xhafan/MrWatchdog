namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class SlackErrorReportingOptions
{
    public const string SectionName = "SlackErrorReporting";

    public string WebHookUrl { get; set; } = "";
    public string Channel { get; set; } = "";
}
