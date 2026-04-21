using CoreBackend.Account.Features.LoginLink.Commands;
using CoreBackend.Account.Features.LoginLink.Domain;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.TestHelpers;
using MrWatchdog.Core.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Account.LoginLink.Commands;

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