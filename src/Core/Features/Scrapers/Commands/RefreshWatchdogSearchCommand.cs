using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record RefreshWatchdogSearchCommand(long WatchdogSearchId) : Command;