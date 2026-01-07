namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SmtpClientDirectlyToRecipientMailServerEmailSenderOptions
{
    public string DkimDomain { get; set; } = null!;
    public string DkimSelector { get; set; } = null!;
    public string DkimPrivateKey { get; set; } = null!;
    public string EhloDomainName { get; set; } = null!;
}