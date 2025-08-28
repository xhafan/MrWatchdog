using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Account.Queries;

public class GetLoginTokenByGuidQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    ILoginTokenRepository loginTokenRepository
) : BaseNhibernateQueryHandler<GetLoginTokenByGuidQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetLoginTokenByGuidQuery query)
    {
        var loginToken = await loginTokenRepository.LoadByGuidAsync(query.LoginTokenGuid);
        return (TResult) (object) loginToken.GetDto();
    }
}