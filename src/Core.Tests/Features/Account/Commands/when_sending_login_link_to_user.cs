using System.Security.Claims;
using FakeItEasy;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Features.Account.Commands;

[TestFixture]
public class when_sending_login_link_to_user : BaseDatabaseTest
{
    private readonly string _email = $"user+{Guid.NewGuid()}@email.com";
    private IOptions<JwtOptions> _iJwtOptions = null!;
    private ICoreBus _bus = null!;
    private LoginToken? _loginToken;

    [SetUp]
    public async Task Context()
    {
        _iJwtOptions = OptionsTestRetriever.Retrieve<JwtOptions>();
        
        _bus = A.Fake<ICoreBus>();
        
        var handler = new SendLoginLinkToUserCommandMessageHandler(
            new LoginTokenRepository(UnitOfWork),
            _bus,
            _iJwtOptions,
            OptionsTestRetriever.Retrieve<RuntimeOptions>()
        );

        await handler.Handle(new SendLoginLinkToUserCommand(_email, ReturnUrl: "/Watchdogs"));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var loginTokenRepository = new LoginTokenRepository(UnitOfWork);
        _loginToken = (await loginTokenRepository.QueryByEmailAsync(_email)).SingleOrDefault();
    }

    [Test]
    public void login_token_is_created()
    {
        _loginToken.ShouldNotBeNull();
        var claimsPrincipal = TokenValidator.ValidateToken(_loginToken.Token, _iJwtOptions.Value);

        claimsPrincipal.FindFirstValue(ClaimTypes.Email).ShouldBe(_email);
        claimsPrincipal.FindFirstValue(CustomClaimTypes.Guid).ShouldBe(_loginToken.Guid.ToString());
        claimsPrincipal.FindFirstValue(CustomClaimTypes.ReturnUrl).ShouldBe("/Watchdogs");
    }

    [Test]
    public void login_link_email_is_sent()
    {
        A.CallTo(() => _bus.Send(A<SendEmailCommand>.That.Matches(p => _MatchingCommand(p)))).MustHaveHappenedOnceExactly();
    }

    private bool _MatchingCommand(SendEmailCommand command)
    {
        _loginToken.ShouldNotBeNull();

        command.RecipientEmail.ShouldBe(_email);
        command.Subject.ShouldContain("login link");
        command.HtmlMessage.ShouldContain("log in");
        command.HtmlMessage.ShouldContain($"https://mrwatchdog_test/Account/ConfirmLogin?token={_loginToken.Token}");
        return true;
    }
}