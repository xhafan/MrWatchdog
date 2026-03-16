namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SmtpServerEmailSenderOptions
{
    public string SmtpServer { get; set; } = null!;
    public int Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}