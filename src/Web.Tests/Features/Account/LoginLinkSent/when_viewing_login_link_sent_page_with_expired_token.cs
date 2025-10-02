using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.LoginLinkSent;

namespace MrWatchdog.Web.Tests.Features.Account.LoginLinkSent;

[TestFixture]
public class when_viewing_login_link_sent_page_with_expired_token : BaseDatabaseTest
{
    private LoginLinkSentModel _model = null!;
    private LoginToken _loginToken = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _model = new LoginLinkSentModelBuilder(UnitOfWork)
            .WithLoginTokenGuid(_loginToken.Guid)
            .Build();
    }
    
    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(async () => await _model.OnGet());
        
        ex.Message.ShouldContain("The token is expired");
    }
    
    private void _BuildEntities()
    {
        var jwtOptions = OptionsTestRetriever.Retrieve<JwtOptions>().Value;
        
        var loginTokenGuid = Guid.NewGuid();
        _loginToken = new LoginTokenBuilder(UnitOfWork)
            .WithGuid(loginTokenGuid)
            .WithToken(TokenGenerator.GenerateToken(
                loginTokenGuid, 
                $"user+{Guid.NewGuid()}@email.com", 
                returnUrl: "/Watchdogs/Alerts", 
                jwtOptions,
                validFrom: DateTime.UtcNow.AddMinutes(-jwtOptions.ExpireMinutes - 1)
            ))
            .Build();
    }
}