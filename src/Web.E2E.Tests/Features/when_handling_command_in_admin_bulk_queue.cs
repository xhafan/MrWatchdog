using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features;

[TestFixture]
public class when_handling_command_in_admin_bulk_queue : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _superAdminUser = null!;
    private HttpClient _webApplicationClient = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();

        _webApplicationClient = RunOncePerTestRun.WebApplicationFactory.Value.CreateDefaultClient(new CookieContainerHandler(new CookieContainer()));
        await E2ETestHelper.LogUserIn(
            _webApplicationClient,
            _loginToken.Guid,
            new Dictionary<string, string>
            {
                {CustomHeaders.QueueForRedirection, RebusQueues.AdminBulk}
            }
        );
    }

    [Test]
    public async Task mark_login_token_as_used_command_job_queue_is_set_correctly()
    {
        var loginTokenRepository = new LoginTokenRepository(UnitOfWork);
        var loginToken = await loginTokenRepository.LoadByGuidAsync(_loginToken.Guid);

        JobAffectedEntity jobAffectedEntityAlias = null!;

        var job = await UnitOfWork.Session!.QueryOver<Job>()
            .JoinAlias(x => x.AffectedEntities, () => jobAffectedEntityAlias)
            .Where(x => x.Type == nameof(MarkLoginTokenAsUsedCommand)
                        && jobAffectedEntityAlias.EntityId == loginToken.Id)
            .SingleOrDefaultAsync();

        job.Queue.ShouldBe($"Test{RebusQueues.AdminBulk}");
    }

    [TearDown]
    public async Task TearDown()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _webApplicationClient?.Dispose();

        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await E2ETestHelper.DeleteMarkLoginTokenAsUsedCommandJob(_loginToken.Guid, newUnitOfWork);

                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
                await newUnitOfWork.DeleteUserCascade(_superAdminUser);
            }
        );
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _superAdminUser = new UserBuilder(newUnitOfWork)
                    .WithSuperAdmin(true)
                    .Build();

                _loginToken = new LoginTokenBuilder(newUnitOfWork)
                    .WithEmail(_superAdminUser.Email)
                    .Build();
                _loginToken.Confirm();
            }
        );
    }
}