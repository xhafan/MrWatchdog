using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using MrWatchdog.Web.Features.Account.Login;
using MrWatchdog.Web.Features.Shared.ReinforcedTypings;

namespace MrWatchdog.Web.Tests.Features.Account.Login;

[TestFixture]
public class when_logging_in_or_registering_user : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private LoginModel _model = null!;
    private ICoreBus _bus = null!;
    private Job _job = null!;
    private LoginToken _loginToken = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<ICoreBus>();
        
        _SimulateLoginTokenAndJobCreationOnSendLoginLinkToUserCommand();
        
        _model = new LoginModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithEmail(" user@email.com  ")
            .Build();
        
        _actionResult = await _model.OnPost();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<RedirectResult>();
        var redirectResult = (RedirectResult) _actionResult;
        redirectResult.Url.ShouldBe(AccountUrlConstants.AccountLoginLinkSentUrl.Replace(AccountUrlConstants.LoginTokenGuidVariable, _loginToken.Guid.ToString()));
    }

    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        await newUnitOfWork.DeleteJobCascade(_job);
        await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
    }
    
    private void _SimulateLoginTokenAndJobCreationOnSendLoginLinkToUserCommand()
    {
        A.CallTo(() =>
                _bus.Send(
                    A<SendLoginLinkToUserCommand>.That.Matches(p =>
                        p.Email == "user@email.com"
                        && p.ReturnUrl == LoginModelBuilder.ReturnUrl
                    )
                )
            )
            .Invokes(call =>
            {
                // simulate the command handler in a separate transaction
                using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
                newUnitOfWork.BeginTransaction();

                _loginToken = new LoginTokenBuilder(newUnitOfWork).Build();
                newUnitOfWork.Save(_loginToken);

                var command = (SendLoginLinkToUserCommand) call.Arguments.Single()!;
                command.Guid = Guid.NewGuid();
                
                _job = new JobBuilder(newUnitOfWork)
                    .WithGuid(command.Guid)
                    .WithType(nameof(SendLoginLinkToUserCommand))
                    .WithKind(JobKind.Command)
                    .Build();
                _job.AddAffectedEntity(DomainConstants.AccountLoginTokenEntityName, _loginToken.Id, isCreated: true);
                newUnitOfWork.Save(_job);
            });
    }
}