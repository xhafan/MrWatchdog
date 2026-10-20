using Castle.Windsor;
using CoreBackend.Infrastructure;
using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.Infrastructure.Rebus.ErrorHandlers;
using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreWeb.Features.Logs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Web.HostedServices;
using System.Net;
using System.Net.Http.Json;

namespace MrWatchdog.Web.E2E.Tests.Features.Logs;

[TestFixture, NonParallelizable]
public class when_reporting_errors_by_email : BaseDatabaseTest
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task browser_and_backend_reports_use_the_correct_recipients_and_preserve_job_links(bool useCastle)
    {
        var probe = Guid.NewGuid().ToString();
        var browserMessage = $"Browser error {probe}";
        var serverMessage = $"Server error {probe}";
        var jobMessage = $"Failed job {probe}";
        var jobGuid = Guid.NewGuid();
        var factory = _createReportingFactory(useCastle);
        try
        {
            using var client = factory.CreateClient();
            factory.Services.GetServices<IFailedMessageReporter>().ShouldBeEmpty();
            factory.Services.GetRequiredService<IErrorReporter>().ShouldBeOfType<EmailErrorReporter>();
            using var request = new HttpRequestMessage(HttpMethod.Post, LogsUrlConstants.LogErrorUrl)
            {
                Content = JsonContent.Create(browserMessage)
            };
            request.Headers.Add(LogConstants.LogErrorApiSecretHeaderName, "browser-error-secret");
            using var response = await client.SendAsync(request);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger<when_reporting_errors_by_email>()
                .LogError(serverMessage);
            var failure = new FailedMessageReport(jobGuid, typeof(ArchiveScraperCommand), "job-request", jobMessage, DateTime.UtcNow);
            if (useCastle)
            {
                using var child = new WindsorContainer();
                RebusHostedServiceWindsorContainerRegistrator.RegisterServices(child, factory.Services);
                await child.Resolve<IFailedMessageReporter>().Report(failure);
            }
            else
            {
                using var child = (ServiceProvider)RebusHostedServiceServiceProviderRegistrator.RegisterServices(factory.Services);
                await child.GetRequiredService<IFailedMessageReporter>().Report(failure);
            }

            var commands = await E2ETestHelper.WaitForSendEmailCommandJobs(probe, expectedCount: 3);
            commands.Length.ShouldBe(3);
            commands.Where(command => command.Subject.StartsWith(browserMessage, StringComparison.Ordinal)).ShouldHaveSingleItem()
                .RecipientEmail.ShouldBe("frontend-errors@example.test");
            commands.Where(command => command.Subject.StartsWith(serverMessage, StringComparison.Ordinal)).ShouldHaveSingleItem()
                .RecipientEmail.ShouldBe("backend-errors@example.test");
            var jobEmail = commands.Where(command => command.Subject == "Job ArchiveScraperCommand failed").ShouldHaveSingleItem();
            jobEmail.RecipientEmail.ShouldBe("backend-errors@example.test");
            jobEmail.HtmlMessage.ShouldContain($"<a href=\"https://mrwatchdog_test/api/Jobs/{jobGuid}\">ArchiveScraperCommand</a>");
            commands.ShouldAllBe(command => command.IsErrorNotification);
        }
        finally
        {
            await factory.DisposeAsync();
            await NhibernateUnitOfWorkRunner.RunAsync(
                () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
                async unitOfWork =>
                {
                    await E2ETestHelper.DeleteSendEmailCommandJob(browserMessage, unitOfWork);
                    await E2ETestHelper.DeleteSendEmailCommandJob(serverMessage, unitOfWork);
                    await E2ETestHelper.DeleteSendEmailCommandJob(jobMessage, unitOfWork);
                });
        }
    }

    [Test]
    public async Task reporting_continues_after_another_host_stops()
    {
        var probe = Guid.NewGuid().ToString();
        var firstReporter = new RecordingReporter(probe);
        var secondReporter = new RecordingReporter(probe);
        await using var first = _createRecordingFactory(firstReporter);
        await using var second = _createRecordingFactory(secondReporter);
        _ = first.Services;
        var logger = second.Services.GetRequiredService<ILoggerFactory>().CreateLogger<when_reporting_errors_by_email>();

        await first.DisposeAsync();
        logger.LogError("Server error {Probe}", probe);

        await secondReporter.Reported.Task.WaitAsync(TimeSpan.FromSeconds(5));
        firstReporter.Reported.Task.IsCompleted.ShouldBeFalse();
    }

    [Test]
    public async Task hosts_starting_together_report_through_their_own_reporters()
    {
        var firstProbe = Guid.NewGuid().ToString();
        var secondProbe = Guid.NewGuid().ToString();
        using var barrier = new Barrier(2);
        var firstReporter = new RecordingReporter(firstProbe);
        var secondReporter = new RecordingReporter(secondProbe);
        await using var first = _createRecordingFactory(firstReporter).WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(_ => barrier.SignalAndWait(TimeSpan.FromSeconds(5))));
        await using var second = _createRecordingFactory(secondReporter).WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(_ => barrier.SignalAndWait(TimeSpan.FromSeconds(5))));

        var hosts = await Task.WhenAll(Task.Run(() => first.Services), Task.Run(() => second.Services));
        hosts[0].GetRequiredService<ILoggerFactory>().CreateLogger<when_reporting_errors_by_email>()
            .LogError("Server error {Probe}", firstProbe);
        hosts[1].GetRequiredService<ILoggerFactory>().CreateLogger<when_reporting_errors_by_email>()
            .LogError("Server error {Probe}", secondProbe);

        await Task.WhenAll(firstReporter.Reported.Task, secondReporter.Reported.Task)
            .WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static WebApplicationFactory<Program> _createReportingFactory(bool useCastle = false)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureAppConfiguration((_, configuration) => configuration
                .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Test.json"))
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Rebus:IoCContainer"] = useCastle ? "CastleWindsor" : "ServiceProvider",
                    ["Rebus:Transport"] = "InMemory",
                    ["ErrorReporting:Enabled"] = "true",
                    ["EmailSenderChain:Service"] = nameof(NullEmailSenderChain),
                    ["EmailAddresses:FrontendErrors"] = "frontend-errors@example.test",
                    ["EmailAddresses:BackendErrors"] = "backend-errors@example.test",
                    ["Runtime:Url"] = "https://mrwatchdog_test",
                    ["Logging:LogErrorApiSecret"] = "browser-error-secret"
                }));
        });
    }

    private static WebApplicationFactory<Program> _createRecordingFactory(RecordingReporter reporter) =>
        _createReportingFactory().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton<IErrorReporter>(reporter)));

    private sealed class RecordingReporter(string probe) : IErrorReporter
    {
        public TaskCompletionSource Reported { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Report(ErrorReport report, CancellationToken cancellationToken = default)
        {
            if (report.Message.Contains(probe, StringComparison.Ordinal)) Reported.TrySetResult();
            return Task.CompletedTask;
        }
    }
}
