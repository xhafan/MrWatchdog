using System.Globalization;
using CoreBackend.Account.Features;
using CoreBackend.Account.Features.LoginLink;
using CoreBackend.Account.Features.LoginLink.Domain;
using CoreBackend.Features.Jobs.Domain;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using CoreBackend.TestsShared.Builders;
using CoreDdd.Nhibernate.TestHelpers;
using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Infrastructure.Localization;
using MrWatchdog.Core.TestsShared;
using MrWatchdog.Core.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.LoginLink.Login;

namespace MrWatchdog.Web.Tests.Features.Account.LoginLink.Login;

[TestFixture]
public class when_logging_in_or_registering_user : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private LoginModel _model = null!;
    private ICoreBus _bus = null!;
    private Job _job = null!;
    private LoginToken _loginToken = null!;
    private CultureInfo _originalUiCulture = null!;

    [SetUp]
    public async Task Context()
    {
        _originalUiCulture = CultureInfo.CurrentUICulture;
        
        CultureInfo.CurrentUICulture = CultureConstants.Cs;

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
        redirectResult.Url.ShouldBe(AccountUrlConstants.AccountLoginLinkSentUrlTemplate.WithLoginTokenGuid(_loginToken.Guid));
    }

    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteJobCascade(_job);
                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
            }
        );
        
        CultureInfo.CurrentUICulture = _originalUiCulture;
    }
    
    private void _SimulateLoginTokenAndJobCreationOnSendLoginLinkToUserCommand()
    {
        A.CallTo(() =>
                _bus.Send(
                    A<SendLoginLinkToUserCommand>.That.Matches(p =>
                        p.Email == "user@email.com"
                        && p.Culture == CultureConstants.Cs
                        && p.ReturnUrl == LoginModelBuilder.ReturnUrl
                    )
                )
            )
            .Invokes(call =>
            {
                // simulate the command handler in a separate transaction
                NhibernateUnitOfWorkRunner.Run(
                    () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
                    newUnitOfWork =>
                    {
                        _loginToken = new LoginTokenBuilder(newUnitOfWork)
                            .WithCulture(CultureConstants.Cs)
                            .Build();
                        newUnitOfWork.Save(_loginToken);

                        var command = (SendLoginLinkToUserCommand) call.Arguments.Single()!;
                        command.Guid = Guid.NewGuid();

                        _job = new JobBuilder(newUnitOfWork)
                            .WithGuid(command.Guid)
                            .WithType(nameof(SendLoginLinkToUserCommand))
                            .WithKind(JobKind.Command)
                            .Build();
                        _job.AddAffectedEntity(CoreBackendAccountDomainConstants.AccountLoginTokenEntityName, _loginToken.Id, isCreated: true);
                        newUnitOfWork.Save(_job);
                    }
                );
            });
    }
}