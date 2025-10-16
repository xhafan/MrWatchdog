﻿namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class EmailSenderOptions
{
    public string SmtpServer { get; set; } = null!;
    public int Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}