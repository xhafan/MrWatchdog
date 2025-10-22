using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record ArchiveWatchdogSearchCommand(long WatchdogSearchId) : Command;