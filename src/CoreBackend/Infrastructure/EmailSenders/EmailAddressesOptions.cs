namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class EmailAddressesOptions
{
    public string NoReply { get; set; } = null!;
    public string Admin { get; set; } = null!;
    public string Support { get; set; } = null!;
    public string BackendErrors { get; set; } = null!;
    public string FrontendErrors { get; set; } = null!;
}