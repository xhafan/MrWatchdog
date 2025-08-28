using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Account.Commands;

[TestFixture]
public class when_marking_login_token_as_used : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;

    [SetUp]
    public async Task Context()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
        _loginToken.Confirm();

        var handler = new MarkLoginTokenAsUsedCommandMessageHandler(new LoginTokenRepository(UnitOfWork));

        await handler.Handle(new MarkLoginTokenAsUsedCommand(_loginToken.Guid));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _loginToken = UnitOfWork.LoadById<LoginToken>(_loginToken.Id);
    }

    [Test]
    public void login_token_is_used()
    {
        _loginToken.Used.ShouldBe(true);
    }
}