using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Playwright;
using NUnit.Framework;

namespace CoreBackend.TestsShared.Playwright;

// todo: move this to a future CoreWeb.TestsShared and remove Playwright nuget reference in this project?
public static class PlaywrightCleanup
{
    public static async Task CleanupPlaywrightWithoutMaskingTestFailureAsync(
        IBrowser? browser,
        IPlaywright? playwright,
        PlaywrightDriverProcessTracker playwrightDriver,
        ExceptionDispatchInfo? testFailure
    )
    {
        var cleanupFailures = new List<Exception>();

        if (browser != null)
        {
            await _captureCleanupFailureAsync(
                () => CloseBrowserWithDriverTimeoutAsync(browser, playwrightDriver),
                cleanupFailures
            );
        }

        if (playwright != null)
        {
            await _captureCleanupFailureAsync(
                () => DisposePlaywrightWithDriverTimeoutAsync(playwright, playwrightDriver),
                cleanupFailures
            );
        }

        if (cleanupFailures.Count == 0)
        {
            return;
        }

        if (testFailure != null)
        {
            foreach (var cleanupFailure in cleanupFailures)
            {
                TestContext.WriteLine(
                    $"Playwright cleanup failed after an earlier test failure and was suppressed to preserve the original failure: {cleanupFailure}"
                );
            }

            return;
        }

        if (cleanupFailures.Count == 1)
        {
            ExceptionDispatchInfo.Capture(cleanupFailures[0]).Throw();
        }

        throw new AggregateException("Playwright cleanup failed.", cleanupFailures);
    }

    public static async Task DisposeBrowserContextWithoutMaskingFailureAsync(
        IBrowserContext context,
        PlaywrightDriverProcessTracker playwrightDriver,
        ExceptionDispatchInfo? failure
    )
    {
        try
        {
            await DisposeBrowserContextWithDriverTimeoutAsync(context, playwrightDriver);
        }
        catch (Exception exception)
        {
            if (failure != null)
            {
                TestContext.WriteLine(
                    $"Browser context cleanup failed after an earlier failure and was suppressed to preserve the original failure: {exception}"
                );
                return;
            }

            throw;
        }
    }

    public static async Task DisposePlaywrightWithDriverTimeoutAsync(
        IPlaywright playwright,
        PlaywrightDriverProcessTracker playwrightDriver
    )
    {
        await RunPlaywrightCleanupWithDriverTimeoutAsync(
            Task.Run(playwright.Dispose),
            "disposing Playwright",
            playwrightDriver
        );
    }

    public static async Task DisposeBrowserContextWithDriverTimeoutAsync(
        IBrowserContext context,
        PlaywrightDriverProcessTracker playwrightDriver
    )
    {
        await RunPlaywrightCleanupWithDriverTimeoutAsync(
            Task.Run(async () => await context.DisposeAsync()),
            "disposing Chromium browser context",
            playwrightDriver
        );
    }

    public static async Task CloseBrowserWithDriverTimeoutAsync(
        IBrowser browser,
        PlaywrightDriverProcessTracker playwrightDriver
    )
    {
        await RunPlaywrightCleanupWithDriverTimeoutAsync(
            browser.CloseAsync(),
            "closing Chromium",
            playwrightDriver
        );
    }

    public static async Task RunPlaywrightCleanupWithDriverTimeoutAsync(
        Task cleanupTask,
        string operation,
        PlaywrightDriverProcessTracker playwrightDriver
    )
    {
        var initialTimeout = TimeSpan.FromSeconds(5);
        if (await _waitForCleanupTaskAsync(cleanupTask, initialTimeout))
        {
            await cleanupTask;
            return;
        }

        var driverTerminationResult = playwrightDriver.KillOwnedDriverProcesses();
        TestContext.WriteLine($"Timed out {operation}; {driverTerminationResult.ToLogMessage()}.");

        var afterKillTimeout = TimeSpan.FromSeconds(10);
        if (await _waitForCleanupTaskAsync(cleanupTask, afterKillTimeout))
        {
            await cleanupTask;
            return;
        }

        _observeLateCleanupFailure(cleanupTask);
        throw new TimeoutException(
            $"Timed out {operation}; cleanup did not complete within {initialTimeout + afterKillTimeout} " +
            $"after driver termination attempt. Result: {driverTerminationResult.ToLogMessage()}."
        );
    }

    private static async Task _captureCleanupFailureAsync(
        Func<Task> cleanup,
        List<Exception> cleanupFailures
    )
    {
        try
        {
            await cleanup();
        }
        catch (Exception exception)
        {
            cleanupFailures.Add(exception);
        }
    }

    private static async Task<bool> _waitForCleanupTaskAsync(Task cleanupTask, TimeSpan timeout)
    {
        return await Task.WhenAny(cleanupTask, Task.Delay(timeout)) == cleanupTask;
    }

    private static void _observeLateCleanupFailure(Task cleanupTask)
    {
        _ = cleanupTask.ContinueWith(
            task => _ = task.Exception,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously
        );
    }
}

public sealed class PlaywrightDriverProcessTracker
{
    private readonly string? _driverNodePath;
    private readonly HashSet<int> _existingProcessIds;
    private readonly Dictionary<int, ProcessSnapshot> _ownedProcesses = [];
    private readonly DateTime _capturedAtUtc;
    private static readonly StringComparison _pathComparison =
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private PlaywrightDriverProcessTracker(string? driverNodePath)
    {
        _driverNodePath = driverNodePath;
        _capturedAtUtc = DateTime.UtcNow;
        _existingProcessIds = _driverNodePath is null
            ? []
            : _matchingDriverProcessIds();
    }

    public static PlaywrightDriverProcessTracker Capture()
    {
        var nodeFileName = OperatingSystem.IsWindows() ? "node.exe" : "node";
        var nodeDirectory = Path.Combine(AppContext.BaseDirectory, ".playwright", "node");
        var driverNodePath = Directory.Exists(nodeDirectory)
            ? Directory.GetFiles(nodeDirectory, nodeFileName, SearchOption.AllDirectories).FirstOrDefault()
            : null;

        return new PlaywrightDriverProcessTracker(driverNodePath);
    }

    public DriverProcessTerminationResult KillOwnedDriverProcesses()
    {
        var result = new DriverProcessTerminationResult();
        foreach (var ownedProcess in _ownedProcesses.Values)
        {
            Process? process = null;
            try
            {
                process = Process.GetProcessById(ownedProcess.ProcessId);
                if (!_isSameProcess(process, ownedProcess))
                {
                    result.RecordSkipped(ownedProcess.ProcessId, "process identity no longer matches the tracked driver");
                    continue;
                }

                if (process.HasExited)
                {
                    result.RecordAlreadyExited(process.Id);
                    continue;
                }

                process.Kill(entireProcessTree: true);
                if (process.WaitForExit(5000))
                {
                    result.RecordTerminated(process.Id);
                }
                else
                {
                    result.RecordStillRunning(process.Id);
                }
            }
            catch (InvalidOperationException)
            {
                result.RecordAlreadyExited(ownedProcess.ProcessId);
            }
            catch (ArgumentException)
            {
                result.RecordAlreadyExited(ownedProcess.ProcessId);
            }
            catch (System.ComponentModel.Win32Exception exception)
            {
                result.RecordFailure(ownedProcess.ProcessId, "kill", exception);
            }
            finally
            {
                process?.Dispose();
            }
        }

        return result;
    }

    public void TrackDriverProcess(IPlaywright playwright)
    {
        if (_tryTrackDriverProcessFromPlaywright(playwright))
        {
            return;
        }

        TestContext.WriteLine(
            "Could not identify the Playwright driver process by reflection; falling back to bundled node path and start time."
        );
        _trackNewDriverProcessesByPath();
    }

    private bool _tryTrackDriverProcessFromPlaywright(IPlaywright playwright)
    {
        try
        {
            var process = _tryGetPlaywrightDriverProcess(playwright);
            if (process == null || process.HasExited)
            {
                return false;
            }

            _ownedProcesses[process.Id] = _snapshot(process);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
        catch (MemberAccessException)
        {
            return false;
        }
        catch (TargetException)
        {
            return false;
        }
        catch (TargetInvocationException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static Process? _tryGetPlaywrightDriverProcess(IPlaywright playwright)
    {
        // Playwright .NET does not expose the driver process publicly; in 1.59 this reaches the actual StdIOTransport process.
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var connection = playwright.GetType().GetField("_connection", flags)?.GetValue(playwright);
        var onMessage = connection?.GetType().GetProperty("OnMessage", flags)?.GetValue(connection) as Delegate;
        var transport = onMessage?.Target?.GetType().GetField("transport", flags)?.GetValue(onMessage.Target);

        return transport?.GetType().GetField("_process", flags)?.GetValue(transport) as Process;
    }

    private void _trackNewDriverProcessesByPath()
    {
        // Reflection should normally find the exact PID; this fallback is bounded by executable path, prior snapshot, and start time.
        var candidates = new List<ProcessSnapshot>();
        foreach (var process in _getProcessesByBundledNodeName())
        {
            try
            {
                if (!_existingProcessIds.Contains(process.Id) &&
                    _isMatchingDriverProcess(process) &&
                    process.StartTime.ToUniversalTime() >= _capturedAtUtc.AddSeconds(-2))
                {
                    candidates.Add(_snapshot(process));
                }
            }
            catch (InvalidOperationException)
            {
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
            finally
            {
                process.Dispose();
            }
        }

        if (candidates.Count == 1)
        {
            _ownedProcesses[candidates[0].ProcessId] = candidates[0];
            return;
        }

        if (candidates.Count == 0)
        {
            TestContext.WriteLine("Fallback driver process detection found no new bounded candidates.");
            return;
        }

        TestContext.WriteLine(
            $"Fallback driver process detection found {candidates.Count} bounded candidates " +
            $"({string.Join(", ", candidates.Select(candidate => candidate.ProcessId))}); " +
            "skipping destructive cleanup because the driver process is ambiguous."
        );
    }

    private HashSet<int> _matchingDriverProcessIds()
    {
        var processIds = new HashSet<int>();
        foreach (var process in _getProcessesByBundledNodeName())
        {
            try
            {
                if (_isMatchingDriverProcess(process))
                {
                    processIds.Add(process.Id);
                }
            }
            finally
            {
                process.Dispose();
            }
        }

        return processIds;
    }

    private IEnumerable<Process> _getProcessesByBundledNodeName()
    {
        if (_driverNodePath is null)
        {
            return [];
        }

        return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_driverNodePath));
    }

    private bool _isMatchingDriverProcess(Process process)
    {
        try
        {
            return string.Equals(
                process.MainModule?.FileName,
                _driverNodePath,
                _pathComparison
            );
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }

    private ProcessSnapshot _snapshot(Process process)
    {
        return new ProcessSnapshot(
            process.Id,
            _tryGetStartTimeUtc(process),
            _tryGetMainModuleFileName(process)
        );
    }

    private bool _isSameProcess(Process process, ProcessSnapshot ownedProcess)
    {
        var verified = false;

        if (ownedProcess.StartTimeUtc is { } ownedStartTimeUtc)
        {
            var startTimeUtc = _tryGetStartTimeUtc(process);
            if (startTimeUtc != ownedStartTimeUtc)
            {
                return false;
            }

            verified = true;
        }

        if (ownedProcess.FileName is { } ownedFileName)
        {
            var fileName = _tryGetMainModuleFileName(process);
            if (!string.Equals(fileName, ownedFileName, _pathComparison))
            {
                return false;
            }

            verified = true;
        }

        return verified;
    }

    private static DateTime? _tryGetStartTimeUtc(Process process)
    {
        try
        {
            return process.StartTime.ToUniversalTime();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    private static string? _tryGetMainModuleFileName(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    private sealed record ProcessSnapshot(int ProcessId, DateTime? StartTimeUtc, string? FileName);
}

public sealed class DriverProcessTerminationResult
{
    private readonly List<int> _terminatedProcessIds = [];
    private readonly List<int> _alreadyExitedProcessIds = [];
    private readonly List<int> _stillRunningProcessIds = [];
    private readonly List<DriverProcessTerminationFailure> _failures = [];
    private readonly List<DriverProcessTerminationSkip> _skippedProcesses = [];

    public void RecordTerminated(int processId)
    {
        _terminatedProcessIds.Add(processId);
    }

    public void RecordAlreadyExited(int processId)
    {
        _alreadyExitedProcessIds.Add(processId);
    }

    public void RecordStillRunning(int processId)
    {
        _stillRunningProcessIds.Add(processId);
    }

    public void RecordFailure(int processId, string stage, Exception exception)
    {
        _failures.Add(new DriverProcessTerminationFailure(
            processId,
            stage,
            $"{exception.GetType().Name}: {exception.Message}"
        ));
    }

    public void RecordSkipped(int processId, string reason)
    {
        _skippedProcesses.Add(new DriverProcessTerminationSkip(processId, reason));
    }

    public string ToLogMessage()
    {
        var parts = new List<string>();
        if (_terminatedProcessIds.Count > 0)
        {
            parts.Add($"terminated driver process id(s): {string.Join(", ", _terminatedProcessIds)}");
        }

        if (_stillRunningProcessIds.Count > 0)
        {
            parts.Add($"driver process id(s) still running after kill timeout: {string.Join(", ", _stillRunningProcessIds)}");
        }

        if (_failures.Count > 0)
        {
            parts.Add(
                "failed to terminate driver process id(s): " +
                string.Join(", ", _failures.Select(failure => $"{failure.ProcessId} ({failure.Stage}: {failure.Message})"))
            );
        }

        if (_alreadyExitedProcessIds.Count > 0)
        {
            parts.Add($"driver process id(s) already exited before kill completed: {string.Join(", ", _alreadyExitedProcessIds)}");
        }

        if (_skippedProcesses.Count > 0)
        {
            parts.Add(
                "skipped driver process id(s): " +
                string.Join(", ", _skippedProcesses.Select(skip => $"{skip.ProcessId} ({skip.Reason})"))
            );
        }

        return parts.Count == 0
            ? "no owned driver process was available to terminate"
            : string.Join("; ", parts);
    }

    private sealed record DriverProcessTerminationFailure(int ProcessId, string Stage, string Message);

    private sealed record DriverProcessTerminationSkip(int ProcessId, string Reason);
}
