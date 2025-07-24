using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record CreateWatchdogAlertCommand(long WatchdogId, string? SearchTerm) : Command;