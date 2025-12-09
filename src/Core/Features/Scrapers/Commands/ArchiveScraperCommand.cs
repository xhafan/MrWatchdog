using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record ArchiveWatchdogCommand(long WatchdogId) : Command;