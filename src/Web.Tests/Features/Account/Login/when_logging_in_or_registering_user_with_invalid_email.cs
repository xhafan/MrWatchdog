using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account.Login;

namespace MrWatchdog.Web.Tests.Features.Account.Login;

[TestFixture]
public class when_logging_in_or_registering_user_with_invalid_email : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private LoginModel _model = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<ICoreBus>();
        
        _model = new LoginModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithEmail("user_email.com")
            .Build();
        
        _actionResult = await _model.OnPost();
    }
    
    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(A<SendLoginLinkToUserCommand>._)).MustNotHaveHappened();
    }    
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
        var pageResult = (PageResult) _actionResult;
        pageResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
    }

    [Test]
    public void model_is_invalid()
    {
        _model.ModelState.IsValid.ShouldBe(false);
        var emailErrors = _model.ModelState[$"{nameof(LoginModel.Email)}"]?.Errors;
        emailErrors.ShouldNotBeNull();
        emailErrors.ShouldContain(x => x.ErrorMessage.Contains("not a valid email"));
    }
}