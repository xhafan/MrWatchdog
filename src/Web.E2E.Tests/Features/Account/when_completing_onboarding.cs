using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Account;

[TestFixture]
public class when_completing_onboarding : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _user = null!;
    private HttpClient _webApplicationClient = null!;
    private Guid _completeOnboardingCommandGuid;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();

        _webApplicationClient = RunOncePerTestRun.WebApplicationFactory.Value.CreateDefaultClient(new CookieContainerHandler(new CookieContainer()));
        await E2ETestHelper.LogUserIn(_webApplicationClient, _loginToken.Guid);
    }

    [Test]
    public async Task complete_onboarding_command_is_sent_successfully()
    {
        var response = await _webApplicationClient.PostAsync(
            UserUrlConstants.ApiCompleteOnboardingUrlTemplate.WithOnboardingIdentifier(OnboardingIdentifiers.ScraperScrapingResults),
            content: null
        );
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _completeOnboardingCommandGuid = Guid.Parse(await response.Content.ReadAsStringAsync());
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
                await E2ETestHelper.DeleteCommandJob<CompleteOnboardingCommand>(_completeOnboardingCommandGuid, newUnitOfWork);

                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
                await newUnitOfWork.DeleteUserCascade(_user);
            }
        );
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _user = new UserBuilder(newUnitOfWork).Build();

                _loginToken = new LoginTokenBuilder(newUnitOfWork)
                    .WithEmail(_user.Email)
                    .Build();
                _loginToken.Confirm();
            }
        );
    }
}