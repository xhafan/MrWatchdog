using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record UpdateWatchdogSearchOverviewCommand(WatchdogSearchOverviewArgs WatchdogSearchOverviewArgs) : Command;