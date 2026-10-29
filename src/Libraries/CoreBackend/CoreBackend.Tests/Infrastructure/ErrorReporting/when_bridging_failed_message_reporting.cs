using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.TestsShared.Infrastructure.ErrorReporting;
using CoreBackend.Infrastructure.Configurations;
using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Rebus.ErrorHandlers;
using CoreBackend.Messages;
using CoreBackend.Resources;
using CoreDdd.Nhibernate.Configurations;
using FakeItEasy;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using ServiceProviderRegistration = CoreBackend.Register.ServiceProvider.CoreBackendServicesServiceProviderRegistrationExtensions;
using CastleRegistration = CoreBackend.Register.Castle.CoreBackendServicesCastleWindsorRegistrationExtensions;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture]
public class when_bridging_failed_message_reporting
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task service_provider_uses_the_main_delivery_queue_in_both_registration_orders(bool bridgeFirst)
    {
        var delivered = new RecordingReporter();
        using var main = _mainServices(delivered).BuildServiceProvider();
        var services = new ServiceCollection();
        if (bridgeFirst) ServiceProviderRegistration.AddCoreBackendServicesFromMainServiceProvider(services, main);
        ServiceProviderRegistration.AddCoreDddAndCoreBackendServices(services,
            main.GetRequiredService<IConfiguration>(), A.Fake<IBus>(), main.GetRequiredService<INhibernateConfigurator>());
        if (!bridgeFirst) ServiceProviderRegistration.AddCoreBackendServicesFromMainServiceProvider(services, main);

        using (var child = services.BuildServiceProvider())
        {
            var reporter = child.GetRequiredService<IFailedMessageReporter>();
            child.GetServices<IFailedMessageReporter>().ShouldHaveSingleItem();
            reporter.ShouldBeOfType<QueuedFailedMessageReporter>();
            main.GetServices<IFailedMessageReporter>().ShouldBeEmpty();
            child.GetRequiredService<ErrorReportQueue>().ShouldBeSameAs(main.GetRequiredService<ErrorReportQueue>());
            reporter.ShouldBeSameAs(child.GetRequiredService<IFailedMessageReporter>());
            await reporter.Report(_failure());
        }

        await _drain(main);
        delivered.Reports.ShouldHaveSingleItem().Message.ShouldContain("Handler exhausted its retries");
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task castle_uses_the_main_delivery_queue_in_both_registration_orders(bool bridgeFirst)
    {
        var delivered = new RecordingReporter();
        using var main = _mainServices(delivered).BuildServiceProvider();
        using (var child = new WindsorContainer())
        {
            if (bridgeFirst)
            {
                CastleRegistration.AddCoreBackendServicesFromMainServiceProvider(child, main);
            }
            else
            {
                child.Register(Component.For<IConfiguration>()
                    .Instance(main.GetRequiredService<IConfiguration>()).Named("test-configuration"));
            }
            CastleRegistration.AddCoreDddAndCoreBackendServices(child, A.Fake<IBus>());
            if (!bridgeFirst) CastleRegistration.AddCoreBackendServicesFromMainServiceProvider(child, main);
            var reporter = child.Resolve<IFailedMessageReporter>();
            child.ResolveAll<IFailedMessageReporter>().ShouldHaveSingleItem();
            reporter.ShouldBeOfType<QueuedFailedMessageReporter>();
            main.GetServices<IFailedMessageReporter>().ShouldBeEmpty();
            child.Resolve<ErrorReportQueue>().ShouldBeSameAs(main.GetRequiredService<ErrorReportQueue>());
            reporter.ShouldBeSameAs(child.Resolve<IFailedMessageReporter>());
            await reporter.Report(_failure());
        }

        await _drain(main);
        delivered.Reports.ShouldHaveSingleItem().RequestId.ShouldBe("request-123");
    }

    [Test]
    public void the_main_provider_does_not_register_a_failed_message_reporter()
    {
        using var main = _mainServices().BuildServiceProvider();

        main.GetServices<IFailedMessageReporter>().ShouldBeEmpty();
    }

    [Test]
    public void service_provider_requires_the_main_delivery_queue()
    {
        var mainServices = _mainServices();
        mainServices.RemoveAll<ErrorReportQueue>();
        using var main = mainServices.BuildServiceProvider();
        var services = new ServiceCollection();
        Should.Throw<InvalidOperationException>(() =>
                ServiceProviderRegistration.AddCoreBackendServicesFromMainServiceProvider(services, main))
            .Message.ShouldContain(nameof(ErrorReportQueue));
    }

    [Test]
    public void castle_requires_the_main_delivery_queue()
    {
        var mainServices = _mainServices();
        mainServices.RemoveAll<ErrorReportQueue>();
        using var main = mainServices.BuildServiceProvider();
        using var child = new WindsorContainer();
        Should.Throw<InvalidOperationException>(() =>
                CastleRegistration.AddCoreBackendServicesFromMainServiceProvider(child, main))
            .Message.ShouldContain(nameof(ErrorReportQueue));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task hosted_containers_own_their_reporters_and_share_the_main_delivery_queue(bool useCastle)
    {
        var delivered = new RecordingReporter();
        using var main = _mainServices(delivered).BuildServiceProvider();
        IFailedMessageReporter first;
        if (useCastle)
        {
            using (var child = new WindsorContainer())
            {
                CastleRegistration.AddCoreBackendServicesFromMainServiceProvider(child, main);
                CastleRegistration.AddCoreDddAndCoreBackendServices(child, A.Fake<IBus>());
                first = child.Resolve<IFailedMessageReporter>();
                await first.Report(_failure());
            }
            using var second = new WindsorContainer();
            CastleRegistration.AddCoreBackendServicesFromMainServiceProvider(second, main);
            CastleRegistration.AddCoreDddAndCoreBackendServices(second, A.Fake<IBus>());
            var reporter = second.Resolve<IFailedMessageReporter>();
            reporter.ShouldNotBeSameAs(first);
            await reporter.Report(_failure() with { ErrorDetails = "Another handler failure" });
        }
        else
        {
            var services = new ServiceCollection();
            ServiceProviderRegistration.AddCoreBackendServicesFromMainServiceProvider(services, main);
            ServiceProviderRegistration.AddCoreDddAndCoreBackendServices(services,
                main.GetRequiredService<IConfiguration>(), A.Fake<IBus>(), main.GetRequiredService<INhibernateConfigurator>());
            using (var child = services.BuildServiceProvider())
            {
                first = child.GetRequiredService<IFailedMessageReporter>();
                await first.Report(_failure());
            }
            using var second = services.BuildServiceProvider();
            var reporter = second.GetRequiredService<IFailedMessageReporter>();
            reporter.ShouldNotBeSameAs(first);
            await reporter.Report(_failure() with { ErrorDetails = "Another handler failure" });
        }

        await _drain(main);
        delivered.Reports.Count.ShouldBe(2);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task it_delivers_a_hosted_failed_job_as_a_backend_email_with_a_job_link(bool useCastle)
    {
        var bus = A.Fake<ICoreBus>();
        SendEmailCommand? sent = null;
        A.CallTo(() => bus.Send(A<Command>._)).Invokes((Command command) => sent = (SendEmailCommand)command);
        var services = _mainServices();
        services.AddSingleton(bus);
        services.Configure<EmailAddressesOptions>(options => options.BackendErrors = "backend-errors@example.test");
        services.Configure<RuntimeOptions>(options => options.Url = "https://mrwatchdog_test");
        services.AddSingleton<IErrorReporter, EmailErrorReporter>();
        using var main = services.BuildServiceProvider();
        var failure = _failure();

        if (useCastle)
        {
            using var child = new WindsorContainer();
            CastleRegistration.AddCoreBackendServicesFromMainServiceProvider(child, main);
            CastleRegistration.AddCoreDddAndCoreBackendServices(child, A.Fake<IBus>());
            await child.Resolve<IFailedMessageReporter>().Report(failure);
        }
        else
        {
            var childServices = new ServiceCollection();
            ServiceProviderRegistration.AddCoreBackendServicesFromMainServiceProvider(childServices, main);
            ServiceProviderRegistration.AddCoreDddAndCoreBackendServices(childServices,
                main.GetRequiredService<IConfiguration>(), A.Fake<IBus>(), main.GetRequiredService<INhibernateConfigurator>());
            using var child = childServices.BuildServiceProvider();
            await child.GetRequiredService<IFailedMessageReporter>().Report(failure);
        }

        await _drain(main);

        sent.ShouldNotBeNull();
        sent.RecipientEmail.ShouldBe("backend-errors@example.test");
        sent.Subject.ShouldBe("Job TestCommand failed");
        sent.HtmlMessage.ShouldContain($"<a href=\"https://mrwatchdog_test/api/Jobs/{failure.JobGuid}\">TestCommand</a>");
        sent.IsErrorNotification.ShouldBeTrue();
        A.CallTo(() => bus.Send(A<Command>._)).MustHaveHappenedOnceExactly();
    }

    private static ServiceCollection _mainServices(RecordingReporter? reporter = null)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(A.Fake<INhibernateConfigurator>());
        services.AddSingleton(A.Fake<IHostEnvironment>());
        services.AddSingleton(A.Fake<HybridCache>());
        services.AddSingleton(A.Fake<IMessageTypeGetter>());
        services.AddSingleton(A.Fake<ISharedTranslationsGetter>());
        services.AddLogging();
        services.AddHttpClient();
        services.AddOptions();
        services.AddSingleton<IErrorReporter>(reporter ?? new RecordingReporter());
        services.AddErrorReporting(configuration);
        services.AddSingleton<IErrorReportGate>(_ => ErrorReportTestServices.CreateGate());
        return services;
    }

    private static FailedMessageReport _failure() => new(
        Guid.NewGuid(), typeof(TestCommand), "request-123", "Handler exhausted its retries", DateTime.UtcNow
    );

    private static async Task _drain(ServiceProvider main)
    {
        var worker = main.GetServices<IHostedService>().OfType<ErrorReportDeliveryBackgroundService>().Single();
        await worker.StartAsync(CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);
    }

    private sealed record TestCommand : Command;

    private sealed class RecordingReporter : IErrorReporter
    {
        public List<ErrorReport> Reports { get; } = [];
        public Task Report(ErrorReport report, CancellationToken cancellationToken = default)
        {
            Reports.Add(report);
            return Task.CompletedTask;
        }
    }

}
