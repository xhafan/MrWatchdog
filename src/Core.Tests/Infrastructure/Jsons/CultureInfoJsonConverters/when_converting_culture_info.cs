using CoreBackend.Infrastructure.Jsons;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Infrastructure.Localization;

namespace MrWatchdog.Core.Tests.Infrastructure.Jsons.CultureInfoJsonConverters;

[TestFixture]
public class when_converting_culture_info
{
    [Test]
    public void culture_info_is_serialized_correctly()
    {
        var command = new CreateUserCommand("user@email.com", CultureConstants.En);

        var json = JsonHelper.Serialize(command);

        json.ShouldContain("""
                           "culture":"en"
                           """);
    }

    [Test]
    public void culture_info_is_deserialized_correctly()
    {
        var command = JsonHelper.Deserialize<CreateUserCommand>(
            """
            {
              "email":"user@email.com",
              "culture":"en"
            }
            """
        );

        command.ShouldNotBeNull();
        command.Culture.ShouldBe(CultureConstants.En);
    }
}