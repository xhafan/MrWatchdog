using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.LoginLinkSent;

namespace MrWatchdog.Web.Tests.Features.Account.LoginLinkSent;

[TestFixture]
public class when_viewing_login_link_sent_page : BaseDatabaseTest
{
    private LoginLinkSentModel _model = null!;
    private LoginToken _loginToken = null!;
    private IActionResult _actionResult = null!;
    private readonly string _tokenEmail = $"user+{Guid.NewGuid()}@email.com";

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new LoginLinkSentModelBuilder(UnitOfWork)
            .WithLoginTokenGuid(_loginToken.Guid)
            .Build();
        
        _actionResult = await _model.OnGet();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }    
    
    [Test]
    public void model_is_correct()
    {
        _model.Email.ShouldBe(_tokenEmail);
        _model.LoginTokenGuid.ShouldBe(_loginToken.Guid);
    }

    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
    
    private void _BuildEntities()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork)
            .WithEmail(_tokenEmail)
            .Build();
    }
}