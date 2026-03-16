using CoreBackend.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record ArchiveScraperCommand(long ScraperId) : Command;