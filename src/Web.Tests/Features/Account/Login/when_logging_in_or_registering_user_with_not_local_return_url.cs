using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using MrWatchdog.Core.Features;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using MrWatchdog.Web.Features.Account.Login;

namespace MrWatchdog.Web.Tests.Features.Account.Login;

[TestFixture]
public class when_logging_in_or_registering_user_with_not_local_return_url : BaseDatabaseTest
{
    private LoginModel _model = null!;
    private ICoreBus _bus = null!;
    private LoginToken _loginToken = null!;
    private Job _job = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<ICoreBus>();
        
        _SimulateLoginTokenAndJobCreationOnLoginOrRegisterUserCommand();
        
        _model = new LoginModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithEmail("user@email.com")
            .WithReturnUrl("https://mrwatchdog.com")
            .Build();
        
        await _model.OnPost();
    }
    
    [Test]
    public void command_is_sent_over_message_bus_without_return_url()
    {
        A.CallTo(() => _bus.Send(A<SendLoginLinkToUserCommand>.That.Matches(p => p.ReturnUrl == null))).MustHaveHappened();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        await newUnitOfWork.DeleteJobCascade(_job);
        await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
    }
    
    private void _SimulateLoginTokenAndJobCreationOnLoginOrRegisterUserCommand()
    {
        A.CallTo(() =>
                _bus.Send(
                    A<SendLoginLinkToUserCommand>.That.Matches(p =>
                        p.Email == "user@email.com"
                        && p.ReturnUrl == null
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