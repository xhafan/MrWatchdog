using CoreUtils;
using Microsoft.Extensions.Configuration;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders;

[TestFixture]
[Ignore("Email sending is disabled by default")]
public class when_sending_email
{
    [Test]
    public async Task email_is_sent()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<when_sending_email>()
            .Build();

        var username = config["EmailSender:Username"];
        Guard.Hope(username != null, nameof(username) + " is null");

        var password = config["EmailSender:Password"];
        Guard.Hope(password != null, nameof(password) + " is null");
        
        var emailSenderOptions = OptionsRetriever.Retrieve<EmailSenderOptions>();
        emailSenderOptions.Value.Username = username;
        emailSenderOptions.Value.Password = password;
        
        var emailSender = new EmailSender(emailSenderOptions);

        await emailSender.SendEmail("xhafan+mrwatchdog_test@gmail.com", "test", "<span>Test message</span>");
    }
}