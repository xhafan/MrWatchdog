using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Account.Queries;

public class GetLoginTokenByIdQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    ILoginTokenRepository loginTokenRepository
) : BaseNhibernateQueryHandler<GetLoginTokenByIdQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetLoginTokenByIdQuery query)
    {
        var loginToken = await loginTokenRepository.LoadByIdAsync(query.LoginTokenId);
        return (TResult) (object) loginToken.GetDto();
    }
}