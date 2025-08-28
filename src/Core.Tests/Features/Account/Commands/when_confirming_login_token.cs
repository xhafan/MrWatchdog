using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Account.Commands;

[TestFixture]
public class when_confirming_login_token : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;

    [SetUp]
    public async Task Context()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();

        var handler = new ConfirmLoginTokenCommandMessageHandler(new LoginTokenRepository(UnitOfWork));

        await handler.Handle(new ConfirmLoginTokenCommand(_loginToken.Guid));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _loginToken = UnitOfWork.LoadById<LoginToken>(_loginToken.Id);
    }

    [Test]
    public void login_token_is_confirmed()
    {
        _loginToken.Confirmed.ShouldBe(true);
    }
}