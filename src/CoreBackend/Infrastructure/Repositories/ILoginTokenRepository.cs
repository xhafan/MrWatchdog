using CoreBackend.Features.Account.Domain;
using CoreDdd.Domain.Repositories;

namespace CoreBackend.Infrastructure.Repositories;

public interface ILoginTokenRepository : IRepository<LoginToken>
{
    Task<LoginToken> LoadByGuidAsync(Guid loginTokenGuid);
}