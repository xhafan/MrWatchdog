using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.TestsShared.Infrastructure.ErrorReporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture]
public class when_suppressing_error_reports_in_memory
{
    [Test]
    public async Task it_suppresses_duplicates_for_a_rolling_window_without_extending_expiry()
    {
        var clock = new ErrorReportTestClock();
        var gate = _gate(clock);
        (await gate.TryReserveAsync(_report(), CancellationToken.None)).ShouldBeTrue();
        clock.Advance(TimeSpan.FromHours(23));
        (await gate.TryReserveAsync(_report() with { RequestId = "another-request" }, CancellationToken.None)).ShouldBeFalse();
        clock.Advance(TimeSpan.FromHours(1));
        (await gate.TryReserveAsync(_report(), CancellationToken.None)).ShouldBeTrue();
        (await gate.TryReserveAsync(_report(), CancellationToken.None)).ShouldBeFalse();
    }

    [Test]
    public async Task it_uses_the_configured_window_and_the_reservation_time()
    {
        var clock = new ErrorReportTestClock();
        var gate = _gate(clock, TimeSpan.FromMinutes(30));
        var report = _report() with { OccurredAtUtc = clock.GetUtcNow().AddDays(-7).UtcDateTime };
        (await gate.TryReserveAsync(report, CancellationToken.None)).ShouldBeTrue();
        clock.Advance(TimeSpan.FromMinutes(29));
        (await gate.TryReserveAsync(report, CancellationToken.None)).ShouldBeFalse();
        clock.Advance(TimeSpan.FromMinutes(1));
        (await gate.TryReserveAsync(report, CancellationToken.None)).ShouldBeTrue();
    }

    [Test]
    public async Task it_isolates_distinct_errors_origins_and_application_instances()
    {
        var clock = new ErrorReportTestClock();
        var gate = _gate(clock);
        (await gate.TryReserveAsync(_report(), CancellationToken.None)).ShouldBeTrue();
        (await gate.TryReserveAsync(_report() with { Fingerprint = "another-error" }, CancellationToken.None)).ShouldBeTrue();
        (await gate.TryReserveAsync(_report() with { Origin = ErrorReportOrigin.Browser }, CancellationToken.None)).ShouldBeTrue();
        (await _gate(clock).TryReserveAsync(_report(), CancellationToken.None)).ShouldBeTrue();
    }

    [Test]
    public async Task it_allows_only_one_concurrent_reservation()
    {
        var gate = _gate(new ErrorReportTestClock());
        var reservations = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ =>
            Task.Run(() => gate.TryReserveAsync(_report(), CancellationToken.None))));
        reservations.Count(reserved => reserved).ShouldBe(1);
    }

    [Test]
    public async Task it_does_not_reserve_when_canceled()
    {
        var gate = _gate(new ErrorReportTestClock());
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Should.ThrowAsync<OperationCanceledException>(() => gate.TryReserveAsync(_report(), cancellation.Token));
        (await gate.TryReserveAsync(_report(), CancellationToken.None)).ShouldBeTrue();
    }

    [TestCase("1.00:00:00", 24d)]
    [TestCase("1.12:30:00", 36.5d)]
    [TestCase("02:30:00", 2.5d)]
    public async Task it_binds_native_timespan_values_and_registers_the_in_memory_gate(string configuredWindow, double hours)
    {
        var clock = new ErrorReportTestClock();
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(clock);
        services.AddErrorReporting(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ErrorReporting:SuppressionWindow"] = configuredWindow
        }).Build());
        services.AddInMemoryErrorReportGate("MrWatchdog:Production");
        using var provider = services.BuildServiceProvider();
        var gate = provider.GetRequiredService<IErrorReportGate>();
        gate.ShouldBeOfType<InMemoryErrorReportGate>();
        gate.ShouldBeSameAs(provider.GetRequiredService<IErrorReportGate>());
        (await gate.TryReserveAsync(_report(), CancellationToken.None)).ShouldBeTrue();
        clock.Advance(TimeSpan.FromHours(hours) - TimeSpan.FromSeconds(1));
        (await gate.TryReserveAsync(_report(), CancellationToken.None)).ShouldBeFalse();
        clock.Advance(TimeSpan.FromSeconds(1));
        (await gate.TryReserveAsync(_report(), CancellationToken.None)).ShouldBeTrue();
    }

    [Test]
    public async Task it_groups_manual_reports_without_request_ids_or_timestamps()
    {
        var gate = _gate(new ErrorReportTestClock());
        var report = _report() with { Fingerprint = null };
        (await gate.TryReserveAsync(report, CancellationToken.None)).ShouldBeTrue();
        (await gate.TryReserveAsync(report with { RequestId = "second", OccurredAtUtc = DateTime.UtcNow.AddDays(1) }, CancellationToken.None)).ShouldBeFalse();
        (await gate.TryReserveAsync(report with { Message = "Different failure" }, CancellationToken.None)).ShouldBeTrue();
    }

    private static IErrorReportGate _gate(TimeProvider clock, TimeSpan? window = null) =>
        new InMemoryErrorReportGate(Options.Create(new ErrorReportingOptions
        {
            SuppressionWindow = window ?? TimeSpan.FromHours(24)
        }), clock, "MrWatchdog:Production");

    private static ErrorReport _report() => new("Failure", "request", DateTime.UtcNow, ErrorReportOrigin.Server,
        Fingerprint: "same-error");
}
