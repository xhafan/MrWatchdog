using CoreBackend.Account.Features.Account.Domain;
using CoreDdd.Domain.Repositories;

namespace CoreBackend.Account.Infrastructure.Repositories;

public interface ILoginTokenRepository : IRepository<LoginToken>
{
    Task<LoginToken> LoadByGuidAsync(Guid loginTokenGuid);
}