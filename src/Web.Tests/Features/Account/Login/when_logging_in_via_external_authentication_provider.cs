using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account.Login;

namespace MrWatchdog.Web.Tests.Features.Account.Login;

[TestFixture]
public class when_logging_in_via_external_authentication_provider : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private LoginModel _model = null!;
    private string _callbackUrl = null!;

    [SetUp]
    public void Context()
    {
        const string returnUrl = "/Watchdogs/Searches";
        _callbackUrl = $"/api/CompleteLogin/CompleteExternalLoginCallback?returnUrl={Uri.EscapeDataString(returnUrl)}";

        var urlHelper = A.Fake<IUrlHelper>();
        A.CallTo(() => urlHelper.Action(A<UrlActionContext>._)).Returns(_callbackUrl);

        _model = new LoginModelBuilder(UnitOfWork)
            .WithUrlHelper(urlHelper)
            .Build();

        _actionResult = _model.OnGetExternalLogin("Google", returnUrl: returnUrl);
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<ChallengeResult>();
        var challengeResult = (ChallengeResult) _actionResult;
        challengeResult.AuthenticationSchemes.ShouldBe(["Google"]);
        challengeResult.Properties.ShouldNotBeNull();
        challengeResult.Properties.RedirectUri.ShouldBe(_callbackUrl);
        challengeResult.Properties.Items.ShouldContainKeyAndValue(AccountUrlConstants.ReturnUrl, "/Watchdogs/Searches");
    }
}