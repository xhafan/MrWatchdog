using CoreDdd.Nhibernate.UnitOfWorks;
using CoreUtils;
using MrWatchdog.Core.Features.Account.Domain;

namespace MrWatchdog.Core.Infrastructure.Repositories;

public class LoginTokenRepository(NhibernateUnitOfWork unitOfWork) : NhibernateRepository<LoginToken>(unitOfWork), ILoginTokenRepository
{
    public async Task<LoginToken> LoadByGuidAsync(Guid loginTokenGuid)
    {
        var loginToken = await Session.QueryOver<LoginToken>()
            .Where(x => x.Guid == loginTokenGuid)
            .SingleOrDefaultAsync();
        Guard.Hope(loginToken != null, $"LoginToken not found, token: {loginTokenGuid}");
        return loginToken;
    }
    
    public async Task<IEnumerable<LoginToken>> QueryByEmailAsync(string email)
    {
        var loginTokens = await Session.QueryOver<LoginToken>()
            .Where(x => x.Email == email)
            .ListAsync();
        return loginTokens;
    }    
}