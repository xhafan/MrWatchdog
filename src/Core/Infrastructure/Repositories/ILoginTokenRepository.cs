using MrWatchdog.Core.Features.Account.Domain;

namespace MrWatchdog.Core.Infrastructure.Repositories;

public interface ILoginTokenRepository : IRepository<LoginToken>
{
    Task<LoginToken> LoadByGuidAsync(Guid loginTokenGuid);
}