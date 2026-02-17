using System.Globalization;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Localization;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Features.Account.Commands;

[TestFixture]
public class when_creating_user : BaseDatabaseTest
{
    private readonly string _email = $"user+{Guid.NewGuid()}@email.com";
    private User? _user;
    private readonly CultureInfo _culture = CultureConstants.En;

    [SetUp]
    public async Task Context()
    {
        var handler = new CreateUserCommandMessageHandler(new UserRepository(UnitOfWork));

        await handler.Handle(new CreateUserCommand(_email, _culture));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var userRepository = new UserRepository(UnitOfWork);
        _user = await userRepository.GetByEmailAsync(_email);
    }

    [Test]
    public void user_is_created()
    {
        _user.ShouldNotBeNull();
        _user.Email.ShouldBe(_email);
        _user.Culture.ShouldBe(_culture);
    }
}