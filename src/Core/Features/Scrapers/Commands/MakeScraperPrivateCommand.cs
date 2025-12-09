using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record MakeWatchdogPrivateCommand(long WatchdogId) : Command;