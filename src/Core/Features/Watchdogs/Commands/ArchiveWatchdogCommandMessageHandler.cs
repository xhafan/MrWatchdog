using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class ArchiveWatchdogSearchCommandMessageHandler(
    IRepository<WatchdogSearch> watchdogSearchRepository,
    IUserRepository userRepository
    ) 
    : IHandleMessages<ArchiveWatchdogSearchCommand>
{
    public async Task Handle(ArchiveWatchdogSearchCommand command)
    {
        var actingUser = await userRepository.LoadByIdAsync(command.ActingUserId);
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(command.WatchdogSearchId);

        watchdogSearch.Archive(actingUser);
    }
}