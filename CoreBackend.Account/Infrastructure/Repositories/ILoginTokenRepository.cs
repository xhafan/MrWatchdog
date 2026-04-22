using CoreBackend.Account.Features.LoginLink.Domain;
using CoreDdd.Domain.Repositories;

namespace CoreBackend.Account.Infrastructure.Repositories;

public interface ILoginTokenRepository : IRepository<LoginToken>
{
    Task<LoginToken> LoadByGuidAsync(Guid loginTokenGuid);
}