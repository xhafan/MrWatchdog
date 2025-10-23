﻿namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SmtpClientDirectlyToRecipientMailServerEmailSenderOptions
{
    public string DkimDomain { get; set; } = null!;
    public string DkimSelector { get; set; } = null!;
    public string DkimPrivateKeyFilePath { get; set; } = null!;
}