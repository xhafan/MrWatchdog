using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account.Login;

namespace MrWatchdog.Web.Tests.Features.Account.Login;

[TestFixture]
public class when_logging_in_via_external_authentication_provider_without_return_url : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private LoginModel _model = null!;

    [SetUp]
    public void Context()
    {
        _model = new LoginModelBuilder(UnitOfWork).Build();
        _model.ReturnUrl = null;

        _actionResult = _model.OnGetExternalLogin("Google");
    }
    
    [Test]
    public void challenge_result_does_not_contain_return_url()
    {
        _actionResult.ShouldBeOfType<ChallengeResult>();
        var challengeResult = (ChallengeResult) _actionResult;
        challengeResult.Properties.ShouldNotBeNull();
        challengeResult.Properties.Items.ShouldNotContainKey(AccountUrlConstants.ReturnUrl);
    }
}