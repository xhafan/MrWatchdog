using System.Security.Claims;
using FakeItEasy;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Features.Account.Commands;

[TestFixture]
public class when_sending_login_link_to_user : BaseDatabaseTest
{
    private readonly string _email = $"user+{Guid.NewGuid()}@email.com";
    private IOptions<JwtOptions> _iJwtOptions = null!;
    private IEmailSender _emailSender = null!;
    private LoginToken? _loginToken;

    [SetUp]
    public async Task Context()
    {
        _iJwtOptions = OptionsTestRetriever.Retrieve<JwtOptions>();
        
        _emailSender = A.Fake<IEmailSender>();
        
        var handler = new SendLoginLinkToUserCommandMessageHandler(
            new LoginTokenRepository(UnitOfWork),
            _emailSender,
            _iJwtOptions,
            OptionsTestRetriever.Retrieve<RuntimeOptions>()
        );

        await handler.Handle(new SendLoginLinkToUserCommand(_email, ReturnUrl: "/Watchdogs/Alerts"));
        
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
        claimsPrincipal.FindFirstValue(CustomClaimTypes.ReturnUrl).ShouldBe("/Watchdogs/Alerts");
    }

    [Test]
    public void login_link_email_is_sent()
    {
        _loginToken.ShouldNotBeNull();
        A.CallTo(() => _emailSender.SendEmail(
                _email,
                A<string>.That.Matches(p => p.Contains("login link")),
                A<string>.That.Matches(p => p.Contains("log in")
                                            && p.Contains($"https://mrwatchdog_test/Account/ConfirmLogin?token={_loginToken.Token}")
                )
            ))
            .MustHaveHappenedOnceExactly();
    }
}