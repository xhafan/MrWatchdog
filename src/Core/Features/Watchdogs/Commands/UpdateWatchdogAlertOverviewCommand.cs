using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record UpdateWatchdogAlertOverviewCommand(WatchdogAlertOverviewArgs WatchdogAlertOverviewArgs) : Command;