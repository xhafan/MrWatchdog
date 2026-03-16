using CoreBackend.Messages;
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record UpdateWatchdogOverviewCommand(WatchdogOverviewArgs WatchdogOverviewArgs) : Command;