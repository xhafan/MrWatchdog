using Microsoft.AspNetCore.Mvc;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account.LoginLinkSent;

namespace MrWatchdog.Web.Tests.Features.Account.LoginLinkSent;

[TestFixture]
public class when_viewing_login_link_sent_page_with_empty_login_token_guid : BaseDatabaseTest
{
    private LoginLinkSentModel _model = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _model = new LoginLinkSentModelBuilder(UnitOfWork)
            .WithLoginTokenGuid(Guid.Empty)
            .Build();
        
        _actionResult = await _model.OnGet();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Test]
    public void model_is_invalid()
    {
        _model.ModelState.IsValid.ShouldBe(false);
        var loginTokenGuidErrors = _model.ModelState[nameof(LoginLinkSentModel.LoginTokenGuid)]?.Errors;
        loginTokenGuidErrors.ShouldNotBeNull();
        loginTokenGuidErrors.ShouldContain(x => x.ErrorMessage.Contains("must not have the default value"));
    }
}