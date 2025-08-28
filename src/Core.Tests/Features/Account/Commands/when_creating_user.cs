using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Features.Account.Commands;

[TestFixture]
public class when_creating_user : BaseDatabaseTest
{
    private readonly string _email = $"user+{Guid.NewGuid()}@email.com";
    private User? _user;

    [SetUp]
    public async Task Context()
    {
        var handler = new CreateUserCommandMessageHandler(new UserRepository(UnitOfWork));

        await handler.Handle(new CreateUserCommand(_email));
        
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
    }
}