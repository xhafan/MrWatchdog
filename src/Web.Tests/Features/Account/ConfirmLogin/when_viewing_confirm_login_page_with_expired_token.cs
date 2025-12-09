using FakeItEasy;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.ConfirmLogin;

namespace MrWatchdog.Web.Tests.Features.Account.ConfirmLogin;

[TestFixture]
public class when_viewing_confirm_login_page_with_expired_token : BaseDatabaseTest
{
    private ConfirmLoginModel _model = null!;
    private ICoreBus _bus = null!;
    private LoginToken _loginToken = null!;
    
    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _model = new ConfirmLoginModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithToken(Uri.EscapeDataString(_loginToken.Token))
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
                returnUrl: "/Scrapers/Searches", 
                jwtOptions,
                validFrom: DateTime.UtcNow.AddMinutes(-jwtOptions.ExpireMinutes - 1)
            ))
            .Build();
    }
}