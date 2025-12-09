using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class RefreshWatchdogSearchCommandMessageHandler(
    IRepository<WatchdogSearch> watchdogSearchRepository
) 
    : IHandleMessages<RefreshWatchdogSearchCommand>
{
    public async Task Handle(RefreshWatchdogSearchCommand command)
    {
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(command.WatchdogSearchId);

        watchdogSearch.Refresh();
    }
}