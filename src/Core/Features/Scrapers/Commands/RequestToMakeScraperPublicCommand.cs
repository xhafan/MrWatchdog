using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record RequestToMakeWatchdogPublicCommand(long WatchdogId) : Command;